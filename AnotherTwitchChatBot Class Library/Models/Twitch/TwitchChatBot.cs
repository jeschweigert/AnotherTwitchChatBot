using ATCB.Library.Models.Music;
using ATCB.Library.Models.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Speech.Synthesis;
using TwitchLib.Enums;
using Colorful;
using System.Drawing;
using System;
using ATCB.Library.Models.Giveaways;
using ATCB.Library.Models.Commands;
using ATCB.Library.Helpers;
using TwitchLib.Services;
using TwitchLib.Events.Services.FollowerService;
using TwitchLib.Events.Services.LiveStreamMonitor;
using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Settings;
using ATCB.Library.Models.DiscordApp;
using Discord;
using Discord.Net;
using ATCB.Library.Models.Plugins;

namespace ATCB.Library.Models.Twitch
{
    public class TwitchChatBot
    {
        private static readonly string ClientId = "r0rcrtf3wfququa8p1nkhsttam2io1";
        private static readonly Guid BotState = Guid.Parse("40b2f73b-9d51-4133-8c27-025a9d31bfcb");

        private ApplicationSettings Settings;

        private SpeechSynthesizer speechSynthesizer;
        private TwitchClient botClient, userClient;
        private TwitchAPI twitchApi;
        private FollowerService followerService;
        private LiveStreamMonitor liveStreamMonitor;
        private WebAuthenticator authenticator;
        
        private CommandFactory commandFactory;
        private EventServiceHandler eventsServices;
        private DiscordChatBot discord;

        private Guid appState;
        private string userAccessToken, botAccessToken;

        public string Username { get; private set; }
        public string Botname { get; private set; }
        public bool IsConnected { get; private set; } = false;
        public bool IsLive { get; private set; } = false;

        /// <summary>
        /// Initializes a new TwitchChatBot object, which handles all communications between Twitch and the client.
        /// </summary>
        /// <param name="authenticator">A pre-initialized WebAuthenticator.</param>
        /// <param name="appState">The global AppState.</param>
        public TwitchChatBot(WebAuthenticator authenticator, Guid appState, ApplicationSettings settings)
        {
            Settings = settings;

            this.authenticator = authenticator;
            this.appState = appState;
            twitchApi = new TwitchAPI(ClientId);

            // Retrieve access tokens and usernames for logging in
            var authDetails = authenticator.GetUserAuthenticationDetails(appState).Result;
            userAccessToken = authDetails.UserDetails.AccessToken;
            botAccessToken = authDetails.BotDetails.AccessToken;
            Username = authDetails.UserDetails.Username;
            Botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;

            // If the either of the usernames are blank, then we have to refresh the tokens.
            if (string.IsNullOrEmpty(Username))
            {
                ConsoleHelper.WriteLine("Refreshing user access token...");
                userAccessToken = RefreshAccessToken(appState, ClientId, userAccessToken, authDetails.UserDetails.RefreshToken, authDetails.ClientSecret).Result;
                Username = authenticator.GetUsernameFromOAuthAsync(userAccessToken).Result;
            }
            if (string.IsNullOrEmpty(Botname))
            {
                ConsoleHelper.WriteLine("Refreshing bot access token...");
                botAccessToken = RefreshAccessToken(BotState, ClientId, botAccessToken, authDetails.BotDetails.RefreshToken, authDetails.ClientSecret).Result;
                Botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;
            }

            twitchApi.Settings.AccessToken = userAccessToken;
            userClient = new TwitchClient(new ConnectionCredentials(Username, userAccessToken), Username);
            botClient = new TwitchClient(new ConnectionCredentials(Botname, botAccessToken), Username);
            followerService = new FollowerService(twitchApi);
            liveStreamMonitor = new LiveStreamMonitor(twitchApi, invokeEventsOnStart: true);
            commandFactory = new CommandFactory();
            eventsServices = new EventServiceHandler(followerService, userClient, botClient);
            discord = new DiscordChatBot(authDetails.DiscordDetails, settings);
            speechSynthesizer = new SpeechSynthesizer();

            // Connect to Discord
            if (authDetails.DiscordDetails.GuildId != "0" && settings.Discord.IsSetup == false)
            {
                settings.Discord.IsSetup = true;
                settings.Save();
            }
            if (settings.Discord.IsSetup)
            {
                try
                {
                    discord.StartAsync().GetAwaiter().GetResult();
                }
                catch (HttpException e)
                {

                }
            }

            // ATCB-made events
            ConsoleHelper.OnConsoleCommand += (sender, e) => { PerformConsoleCommand((e as ConsoleCommandEventArgs).Message); };

            // TwitchLib Service events
            followerService.OnNewFollowersDetected += OnNewFollowersDetected;
            liveStreamMonitor.OnStreamOnline += OnStreamOnline;
            liveStreamMonitor.OnStreamOffline += OnStreamOffline;

            // User client events
            userClient.OnConnected += OnUserConnected;
            userClient.OnConnectionError += OnUserConnectionError;
            userClient.OnBeingHosted += OnUserBeingHosted;
            userClient.OnWhisperReceived += OnWhisperReceived;
            
            // Bot client events
            botClient.OnConnected += OnBotConnected;
            botClient.OnConnectionError += OnBotConnectionError;
            botClient.OnMessageReceived += OnMessageReceived;
            botClient.OnUserJoined += OnJoined;
            botClient.OnUserLeft += OnLeft;
            botClient.OnMessageSent += OnBotMessageSent;
            botClient.OnChatCommandReceived += OnChatCommandReceived;
            botClient.OnNewSubscriber += OnNewSubscriber;
            botClient.OnReSubscriber += OnReSubscriber;
            botClient.OnGiftedSubscription += OnGiftSubscriber;

            // Set up TwitchLib services
            var streams = new List<string>();
            streams.Add(Username);
            if (Settings.TwitchFriends != null)
                streams.AddRange(Settings.TwitchFriends);
            liveStreamMonitor.SetStreamsByUsername(streams);
            followerService.SetChannelByName(Username);
        }

        /// <summary>
        /// Connects to Twitch.
        /// </summary>
        public void Start()
        {
            botClient.Connect();
            userClient.Connect();
            followerService.StartService();
            liveStreamMonitor.StartService();
            IsConnected = true;
        }

        /// <summary>
        /// Disconnects from Twitch.
        /// </summary>
        public void Stop()
        {
            botClient.Disconnect();
            userClient.Disconnect();
            followerService.StopService();
            liveStreamMonitor.StopService();
            IsConnected = false;
        }

        /// <summary>
        /// Refreshes an expired access token and updates the database.
        /// </summary>
        /// <param name="state">A user's app state.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="oldAccessToken">The expired access token.</param>
        /// <returns>The refreshed access token.</returns>
        public async Task<string> RefreshAccessToken(Guid state, string clientId, string oldAccessToken, string refreshToken = null, string clientSecret = null)
        {
            // First, we have to get the refresh token
            if (refreshToken != null)
                refreshToken = await authenticator.GetRefreshTokenByStateAsync(state);

            // Next, we have to get the client secret
            if (clientSecret != null)
                clientSecret = await authenticator.GetClientSecretByValidStateAsync(state);

            // Okay, now let's make our POST request to Twitch
            var refreshResponse = await twitchApi.Auth.v5.RefreshAuthTokenAsync(refreshToken, clientSecret, clientId);
            await authenticator.UpdateAccessAndRefreshTokens(state, refreshResponse.AccessToken, refreshResponse.RefreshToken);
            return refreshResponse.AccessToken;
        }

        /// <summary>
        /// Performs a chat command from the console.
        /// </summary>
        /// <param name="consoleCommand">The command sent.</param>
        public void PerformConsoleCommand(string consoleCommand)
        {
            var commandText = consoleCommand.Split(' ')[0].ToLower();
            Command command = commandFactory.GetCommand(commandText);
            if (command != null)
            {
                var context = new ChatCommand(Botname, new ChatMessage(null, Username, null, Botname, null, true, true, TwitchLib.Enums.UserType.Broadcaster, consoleCommand));
                new Task(() => { command.Run(new CommandContext(botClient, userClient, twitchApi, context, UserType.Chat_Bot, commandFactory, Settings, discord, true)); }).Start();
            }
            else
            {
                ConsoleHelper.WriteLine($"Command \"{commandText}\" was not found.");
            }
        }

        public void RefreshLiveStreamMonitor()
        {
            liveStreamMonitor.StopService();
            var streams = new List<string>();
            streams.Add(Username);
            if (Settings.TwitchFriends != null)
                streams.AddRange(Settings.TwitchFriends);
            liveStreamMonitor.SetStreamsByUsername(streams);
            liveStreamMonitor.StartService();
        }

        /// <summary>
        /// Sends a message through the chat bot.
        /// </summary>
        public void SendMessage(string message)
        {
            botClient.SendMessage(message);
        }

        public UserType DetermineUserType(TwitchLib.Enums.UserType userType, bool isSubscribed = false, bool isConsole = false)
        {
            UserType type = UserType.Default;
            if (isConsole)
                type = UserType.Chat_Bot;
            switch (userType)
            {
                case TwitchLib.Enums.UserType.Broadcaster:
                    type = UserType.Broadcaster;
                    break;
                case TwitchLib.Enums.UserType.Moderator:
                    type = UserType.Moderator;
                    break;
                default:
                    type = UserType.Default;
                    break;
            }
            if (isSubscribed && type == UserType.Default)
                type = UserType.Subscriber;
            return type;
        }

        #region Service Events
        
        private void OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            foreach (var follow in e.NewFollowers)
            {
                if (Settings.SpokenAlerts)
                    speechSynthesizer.SpeakAsync($"Thanks to {follow.User.DisplayName} for following!");
                botClient.SendMessage($"Thanks to {follow.User.DisplayName} for following!");
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            var notify = "@everyone ";
            var channel = Settings.Discord.TwitchLiveAnnounceTextChannel;
            if (e.Channel.ToLower() == Username.ToLower())
                IsLive = true;
            else
            {
                notify = "";
                channel = Settings.Discord.TwitchFriendLiveAnnounceTextChannel;
            }
            ConsoleHelper.WriteLine($"ONLINE: {e.Channel} has gone live!");

            if (Settings.Discord.IsSetup)
            {
                var bio = twitchApi.Users.helix.GetUsersAsync(new List<string>() { e.ChannelId }).GetAwaiter().GetResult().Users.First()?.Description;
                bio = (bio == null) ? "N/A" : bio;
                var game = string.IsNullOrEmpty(e.Stream.Channel.Game) ? "N/A" : e.Stream.Channel.Game;
                var builder = new EmbedBuilder()
                    .WithTitle(e.Stream.Channel.Status)
                    .WithUrl(e.Stream.Channel.Url)
                    .WithColor(new Discord.Color(0x6441A4))
                    .WithImageUrl($"{e.Stream.Preview.Medium}?stamp={DateTime.Now.Ticks}")
                    .WithAuthor(author => {
                        author
                            .WithName(e.Stream.Channel.DisplayName)
                            .WithUrl(e.Stream.Channel.Url)
                            .WithIconUrl(e.Stream.Channel.Logo);
                        })
                    .AddField("Bio", bio)
                    .AddInlineField("Game", game)
                    .AddInlineField("Viewers", e.Stream.Viewers);

                var embed = builder.Build();
                discord.SendMessage($"{notify + e.Stream.Channel.DisplayName} just went live!", channel, embed);
            }
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (e.Channel.ToLower() == Username.ToLower())
                IsLive = false;
            ConsoleHelper.WriteLine($"OFFLINE: {e.Channel} has stopped streaming.");
        }

        #endregion

        #region User Events

        private void OnUserConnected(object sender, OnConnectedArgs e)
        {
            ConsoleHelper.WriteLine($"Hooked into {e.BotUsername}'s account!");
        }

        private void OnUserConnectionError(object sender, OnConnectionErrorArgs e)
        {
            ConsoleHelper.WriteLine($"[ERROR] USER CONNECTION WITH TWITCH HAS BEEN LOST.", System.Drawing.Color.Red);
        }

        private void OnUserBeingHosted(object sender, OnBeingHostedArgs e)
        {
            if (Settings.SpokenAlerts)
                speechSynthesizer.SpeakAsync($"{e.HostedByChannel} began hosting you for {e.Viewers} viewers!");
            botClient.SendMessage($"Thanks for the host, @{e.HostedByChannel}!");
        }

        private void OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            ConsoleHelper.WriteLineWhisper($"{e.WhisperMessage.DisplayName} to {Username}: {e.WhisperMessage.Message}");
        }

        #endregion

        #region Bot Events

        private void OnBotConnected(object sender, OnConnectedArgs e)
        {
            ConsoleHelper.WriteLine($"Bot \"{e.BotUsername}\" connected to {Username}'s stream!");
        }

        private void OnBotConnectionError(object sender, OnConnectionErrorArgs e)
        {
            ConsoleHelper.WriteLine($"[ERROR] BOT CONNECTION WITH TWITCH HAS BEEN LOST.", System.Drawing.Color.Red);
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            var badges = "";
            foreach (var badge in e.ChatMessage.Badges)
            {
                switch (badge.Key)
                {
                    case "admin":
                        badges += "[A] ";
                        break;
                    case "broadcaster":
                        badges += "[B] ";
                        break;
                    case "global_mod":
                        badges += "[GM] ";
                        break;
                    case "staff":
                        badges += "[STAFF] ";
                        break;
                    case "moderator":
                        badges += "[M] ";
                        break;
                    case "subscriber":
                        badges += "[S] ";
                        break;
                    case "turbo":
                        badges += "[T] ";
                        break;
                    default:
                        badges += $"[{badge.Key}] ";
                        break;
                }
            }

            ConsoleHelper.WriteLineChat($"{badges}{e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");

            if (e.ChatMessage.Bits > 0 && Settings.SpokenAlerts)
                speechSynthesizer.SpeakAsync($"Thanks to {e.ChatMessage.DisplayName} for cheering {e.ChatMessage.Bits} bits!");
        }

        private void OnJoined(object sender, OnUserJoinedArgs e)
        {
            ConsoleHelper.WriteLine($"{e.Username} joined the chat.");
        }

        private void OnLeft(object sender, OnUserLeftArgs e)
        {
            ConsoleHelper.WriteLine($"{e.Username} left the chat.");
        }

        private void OnBotMessageSent(object sender, OnMessageSentArgs e)
        {
            ConsoleHelper.WriteLineChat($"{e.SentMessage.DisplayName}: {e.SentMessage.Message}");
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            ConsoleHelper.WriteLine($"{e.Subscriber.DisplayName} just subscribed!");
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime:
                    if (Settings.SpokenAlerts)
                        speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for subscribing via Twitch Prime!");
                    break;
                case SubscriptionPlan.Tier2:
                    if (Settings.SpokenAlerts)
                        speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for their tier 2 subscription!");
                    break;
                case SubscriptionPlan.Tier3:
                    if (Settings.SpokenAlerts)
                        speechSynthesizer.SpeakAsync($"Eternal gratitude to {e.Subscriber.DisplayName} for their tier 3 subscription!");
                    break;
                default:
                    if (Settings.SpokenAlerts)
                        speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for subscribing!");
                    break;
            }
        }

        private void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            ConsoleHelper.WriteLine($"{e.ReSubscriber.DisplayName} just re-subscribed for {e.ReSubscriber.Months} months!");
            if (Settings.SpokenAlerts)
            {
                speechSynthesizer.SpeakAsync($"Much thanks to {e.ReSubscriber.DisplayName} for re-subscribing for {e.ReSubscriber.Months} months!");
                speechSynthesizer.SpeakAsync(e.ReSubscriber.ResubMessage);
            }
        }

        private void OnGiftSubscriber(object sender, OnGiftedSubscriptionArgs e)
        {
            ConsoleHelper.WriteLine($"{e.GiftedSubscription.DisplayName} just bought {e.GiftedSubscription.MsgParamRecipientDisplayName} a {e.GiftedSubscription.MsgParamSubPlan} subscription!");
            if (Settings.SpokenAlerts)
                speechSynthesizer.SpeakAsync($"Much thanks to {e.GiftedSubscription.DisplayName} for buying {e.GiftedSubscription.MsgParamRecipientDisplayName} a sub!");
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            var commandText = e.Command.CommandText.ToLower();
            var userType = DetermineUserType(e.Command.ChatMessage.UserType, e.Command.ChatMessage.IsSubscriber);
            Command command = commandFactory.GetCommand(commandText);
            if (command != null && command.MustBeThisTallToRide <= userType)
            {
                new Task(() => { command.Run(new CommandContext(botClient, userClient, twitchApi, e.Command, DetermineUserType(e.Command.ChatMessage.UserType, e.Command.ChatMessage.IsSubscriber), commandFactory, Settings, discord)); }).Start();
            }
            else
            {
                botClient.SendMessage($"Command \"{commandText}\" was not found.");
            }
        }

        #endregion
    }

    public enum UserType
    {
        Default,
        Subscriber,
        Moderator,
        Broadcaster,
        Chat_Bot
    }
}
