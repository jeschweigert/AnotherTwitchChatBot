using ATCB.Library.Models.Music;
using ATCB.Library.Models.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using TwitchLib.Enums;
using Colorful;
using System.Drawing;
using System;
using ATCB.Library.Models.Giveaways;
using ATCB.Library.Models.Commands;
using ATCB.Library.Helpers;
using SpeechLib.Synthesis;
using TwitchLib.NetCore.Extensions.NetCore;

namespace ATCB.Library.Models.Twitch
{
    public class TwitchChatBot
    {
        private static readonly string ClientId = "r0rcrtf3wfququa8p1nkhsttam2io1";
        private static readonly Guid BotState = Guid.Parse("40b2f73b-9d51-4133-8c27-025a9d31bfcb");

        private SpeechSynthesis speechSynthesizer;
        private TwitchClient botClient, userClient;
        private TwitchAPI twitchApi;
        private WebAuthenticator authenticator;
        
        private CommandFactory commandFactory;

        private Guid appState;
        private string userAccessToken, botAccessToken;

        public string Username { get; private set; }
        public string Botname { get; private set; }
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Initializes a new TwitchChatBot object, which handles all communications between Twitch and the client.
        /// </summary>
        /// <param name="authenticator">A pre-initialized WebAuthenticator.</param>
        /// <param name="appState">The global AppState.</param>
        public TwitchChatBot(WebAuthenticator authenticator, Guid appState)
        {
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

            twitchApi.Settings.AccessToken = botAccessToken;
            userClient = new TwitchClient(new ConnectionCredentials(Username, userAccessToken), Username);
            botClient = new TwitchClient(new ConnectionCredentials(Botname, botAccessToken), Username);
            commandFactory = new CommandFactory();
            speechSynthesizer = new SpeechSynthesis();

            // ATCB-made events
            ConsoleHelper.OnConsoleCommand += (sender, e) => { PerformConsoleCommand((e as ConsoleCommandEventArgs).Message); };

            // User client events
            userClient.OnConnected += OnUserConnected;
            userClient.OnBeingHosted += OnUserBeingHosted;
            
            // Bot client events
            botClient.OnConnected += OnBotConnected;
            botClient.OnConnectionError += OnBotConnectionError;
            botClient.OnMessageReceived += OnMessageReceived;
            botClient.OnMessageSent += OnBotMessageSent;
            botClient.OnChatCommandReceived += OnChatCommandReceived;
            botClient.OnNewSubscriber += OnNewSubscriber;
            botClient.OnReSubscriber += OnReSubscriber;
        }

        /// <summary>
        /// Connects to Twitch.
        /// </summary>
        public void Start()
        {
            botClient.Connect();
            userClient.Connect();
            IsConnected = true;
        }

        /// <summary>
        /// Disconnects from Twitch.
        /// </summary>
        public void Stop()
        {
            botClient.Disconnect();
            userClient.Disconnect();
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
                command.Run(context, botClient);
            }
            else
            {
                botClient.SendMessage($"Command \"{commandText}\" was not found.");
            }
        }

        #region User Events

        private void OnUserConnected(object sender, OnConnectedArgs e)
        {
            ConsoleHelper.WriteLine($"Hooked into {Username}'s account!");
        }

        private void OnUserBeingHosted(object sender, OnBeingHostedArgs e)
        {
            speechSynthesizer.Speak($"{e.HostedByChannel} began hosting you for {e.Viewers} viewers!");
            botClient.SendMessage($"Thanks for the host, @{e.HostedByChannel}!");
        }

        #endregion

        #region Bot Events

        private void OnBotConnected(object sender, OnConnectedArgs e)
        {
            ConsoleHelper.WriteLine($"Bot \"{e.BotUsername}\" connected to {Username}'s stream!");
            speechSynthesizer.Speak($"Bot \"{e.BotUsername}\" connected to {Username}'s stream!");
        }

        private void OnBotConnectionError(object sender, OnConnectionErrorArgs e)
        {
            ConsoleHelper.WriteLine($"[ERROR] CONNECTION WITH TWITCH HAS BEEN LOST.", Color.Red);
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            StyleSheet styleSheet = new StyleSheet(Color.White);
            styleSheet.AddStyle(e.ChatMessage.DisplayName, e.ChatMessage.Color);

            ConsoleHelper.WriteLineStyled($"[{DateTime.Now.ToString("T")}] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}", styleSheet);

            if (e.ChatMessage.Bits > 0)
                speechSynthesizer.Speak($"Thanks to {e.ChatMessage.DisplayName} for cheering {e.ChatMessage.Bits} bits!");
        }

        private void OnBotMessageSent(object sender, OnMessageSentArgs e)
        {
            StyleSheet styleSheet = new StyleSheet(Color.White);
            styleSheet.AddStyle(e.SentMessage.DisplayName, ColorTranslator.FromHtml(e.SentMessage.ColorHex));

            ConsoleHelper.WriteLineStyled($"[{DateTime.Now.ToString("T")}] {e.SentMessage.DisplayName}: {e.SentMessage.Message}", styleSheet);
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime:
                    speechSynthesizer.Speak($"Much thanks to {e.Subscriber.DisplayName} for subscribing via Twitch Prime!");
                    break;
                case SubscriptionPlan.Tier2:
                    speechSynthesizer.Speak($"Much thanks to {e.Subscriber.DisplayName} for their tier 2 subscription!");
                    break;
                case SubscriptionPlan.Tier3:
                    speechSynthesizer.Speak($"Eternal gratitude to {e.Subscriber.DisplayName} for their tier 3 subscription!");
                    break;
                default:
                    speechSynthesizer.Speak($"Much thanks to {e.Subscriber.DisplayName} for subscribing!");
                    break;
            }
        }

        private void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            speechSynthesizer.Speak($"Much thanks to {e.ReSubscriber.DisplayName} for re-subscribing for {e.ReSubscriber.Months} months!");
            speechSynthesizer.Speak(e.ReSubscriber.ResubMessage);
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            var commandText = e.Command.CommandText.ToLower();
            Command command = commandFactory.GetCommand(commandText);
            if (command != null)
            {
                command.Run(e.Command, botClient);
            }
            else
            {
                botClient.SendMessage($"Command \"{commandText}\" was not found.");
            }
        }

        #endregion
    }
}
