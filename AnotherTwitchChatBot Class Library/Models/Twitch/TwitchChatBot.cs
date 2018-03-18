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
        private Translator translator;

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
            userAccessToken = authenticator.GetAccessTokenByStateAsync(appState).Result;
            botAccessToken = authenticator.GetBotAccessTokenByValidStateAsync(appState).Result;
            Username = authenticator.GetUsernameFromOAuthAsync(userAccessToken).Result;
            Botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;

            // If the either of the usernames are blank, then we have to refresh the tokens.
            if (string.IsNullOrEmpty(Username))
            {
                ConsoleHelper.WriteLine("Refreshing user access token...");
                userAccessToken = RefreshAccessToken(appState, ClientId, userAccessToken).Result;
                Username = authenticator.GetUsernameFromOAuthAsync(userAccessToken).Result;
            }
            if (string.IsNullOrEmpty(Botname))
            {
                ConsoleHelper.WriteLine("Refreshing bot access token...");
                botAccessToken = RefreshAccessToken(BotState, ClientId, botAccessToken).Result;
                Botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;
            }

            twitchApi.Settings.AccessToken = userAccessToken;
            userClient = new TwitchClient(new ConnectionCredentials(Username, userAccessToken), Username);
            botClient = new TwitchClient(new ConnectionCredentials(Botname, botAccessToken), Username);
            followerService = new FollowerService(twitchApi);
            liveStreamMonitor = new LiveStreamMonitor(twitchApi);
            commandFactory = new CommandFactory();
            translator = new Translator();
            speechSynthesizer = new SpeechSynthesizer();

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
            userClient.OnMessageReceived += OnMessageReceived;
            userClient.OnWhisperReceived += OnWhisperReceived;
            
            // Bot client events
            botClient.OnConnected += OnBotConnected;
            botClient.OnConnectionError += OnBotConnectionError;
            botClient.OnUserJoined += OnJoined;
            botClient.OnUserLeft += OnLeft;
            botClient.OnMessageSent += OnBotMessageSent;
            botClient.OnChatCommandReceived += OnChatCommandReceived;
            botClient.OnNewSubscriber += OnNewSubscriber;
            botClient.OnReSubscriber += OnReSubscriber;
            botClient.OnGiftedSubscription += OnGiftSubscriber;

            // Set up TwitchLib services
            followerService.SetChannelByName(Username);
            liveStreamMonitor.SetStreamsByUsername(new List<string>(new[] { Username }));
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
        public async Task<string> RefreshAccessToken(Guid state, string clientId, string oldAccessToken)
        {
            string refreshToken, clientSecret;

            // First, we have to get the refresh token
            refreshToken = await authenticator.GetRefreshTokenByStateAsync(state);

            // Next, we have to get the client secret
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
                var context = new ChatCommand(Botname, new ChatMessage(null, Username, null, Botname, null, true, true, UserType.Broadcaster, consoleCommand));
                new Task(() => { command.Run(new CommandContext(botClient, userClient, twitchApi, context, commandFactory, Settings, true)); }).Start();
            }
            else
            {
                ConsoleHelper.WriteLine($"Command \"{commandText}\" was not found.");
            }
        }

        /// <summary>
        /// Sends a message through the chat bot.
        /// </summary>
        public void SendMessage(string message)
        {
            botClient.SendMessage(message);
        }

        #region Service Events
        
        private void OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            foreach (var follow in e.NewFollowers)
            {
                speechSynthesizer.SpeakAsync($"Thanks to {follow.User.DisplayName} for following!");
                botClient.SendMessage($"Thanks to {follow.User.DisplayName} for following!");
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            IsLive = true;
            ConsoleHelper.WriteLine($"ONLINE: {e.Channel} has gone live!");
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
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
            ConsoleHelper.WriteLine($"[ERROR] USER CONNECTION WITH TWITCH HAS BEEN LOST.", Color.Red);
        }

        private void OnUserBeingHosted(object sender, OnBeingHostedArgs e)
        {
            speechSynthesizer.SpeakAsync($"{e.HostedByChannel} began hosting you for {e.Viewers} viewers!");
            botClient.SendMessage($"Thanks for the host, @{e.HostedByChannel}!");
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.DisplayName.ToLower() != Botname.ToLower())
            {
                var msg = e.ChatMessage.Message;
                var lang = (translator.DetectLanguage(msg) == "eng" ? "English" : translator.DetectLanguage(msg)) ?? "N/A";
                if (lang != "English")
                {
                    translator.Translate(msg, "auto|en");
                    while (translator.IsComplete != true) { }
                    msg = translator.Result.Text;
                    lang = translator.Result.Language;
                }
                ConsoleHelper.WriteLineChat($"({lang}) {e.ChatMessage.DisplayName}: {msg}");
            }

            if (e.ChatMessage.Bits > 0)
                speechSynthesizer.SpeakAsync($"Thanks to {e.ChatMessage.DisplayName} for cheering {e.ChatMessage.Bits} bits!");
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
            ConsoleHelper.WriteLine($"[ERROR] BOT CONNECTION WITH TWITCH HAS BEEN LOST.", Color.Red);
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
            ConsoleHelper.WriteLineChat($"[{DateTime.Now.ToString("T")}] (English) {e.SentMessage.DisplayName}: {e.SentMessage.Message}");
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            ConsoleHelper.WriteLine($"{e.Subscriber.DisplayName} just subscribed!");
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime:
                    speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for subscribing via Twitch Prime!");
                    break;
                case SubscriptionPlan.Tier2:
                    speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for their tier 2 subscription!");
                    break;
                case SubscriptionPlan.Tier3:
                    speechSynthesizer.SpeakAsync($"Eternal gratitude to {e.Subscriber.DisplayName} for their tier 3 subscription!");
                    break;
                default:
                    speechSynthesizer.SpeakAsync($"Much thanks to {e.Subscriber.DisplayName} for subscribing!");
                    break;
            }
        }

        private void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            ConsoleHelper.WriteLine($"{e.ReSubscriber.DisplayName} just re-subscribed for {e.ReSubscriber.Months} months!");
            speechSynthesizer.SpeakAsync($"Much thanks to {e.ReSubscriber.DisplayName} for re-subscribing for {e.ReSubscriber.Months} months!");
            speechSynthesizer.SpeakAsync(e.ReSubscriber.ResubMessage);
        }

        private void OnGiftSubscriber(object sender, OnGiftedSubscriptionArgs e)
        {
            ConsoleHelper.WriteLine($"{e.GiftedSubscription.DisplayName} just bought {e.GiftedSubscription.MsgParamRecipientDisplayName} a {e.GiftedSubscription.MsgParamSubPlan} subscription!");
            speechSynthesizer.SpeakAsync($"Much thanks to {e.GiftedSubscription.DisplayName} for buying {e.GiftedSubscription.MsgParamRecipientDisplayName} a sub!");
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            var commandText = e.Command.CommandText.ToLower();
            Command command = commandFactory.GetCommand(commandText);
            if (command != null)
            {
                new Task(() => { command.Run(new CommandContext(botClient, userClient, twitchApi, e.Command, commandFactory, Settings)); }).Start();
            }
            else
            {
                botClient.SendMessage($"Command \"{commandText}\" was not found.");
            }
        }

        #endregion
    }
}
