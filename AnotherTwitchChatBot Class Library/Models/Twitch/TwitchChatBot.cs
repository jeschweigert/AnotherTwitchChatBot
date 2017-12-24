using ATCB.Library.Models.Music;
using ATCB.Library.Models.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Speech.Synthesis;
using TwitchLib.Enums;

namespace ATCB.Library.Models.Twitch
{
    public class TwitchChatBot
    {
        private static readonly string ClientId = "r0rcrtf3wfququa8p1nkhsttam2io1";
        private static readonly Guid BotState = Guid.Parse("888c0b6f-3354-468f-ad96-a176b1a7849a");

        private SpeechSynthesizer speechSynthesizer;
        private TwitchClient botClient, userClient;
        private TwitchAPI twitchApi;
        private WebAuthenticator authenticator;
        private Playlist playlist;

        private Guid appState;
        private string userAccessToken, botAccessToken;

        public string Username { get; private set; }
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

            userAccessToken = authenticator.GetAccessTokenByStateAsync(appState).Result;
            botAccessToken = authenticator.GetBotAccessTokenByValidStateAsync(appState).Result;
            Username = authenticator.GetUsernameFromOAuthAsync(userAccessToken).Result;
            var botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;

            // If the either of the usernames are blank, then we have to refresh the tokens.
            if (string.IsNullOrEmpty(Username))
            {
                Console.WriteLine("Refreshing user access token...");
                userAccessToken = authenticator.RefreshAccessToken(appState, ClientId, userAccessToken).Result;
                Username = authenticator.GetUsernameFromOAuthAsync(userAccessToken).Result;
            }
            if (string.IsNullOrEmpty(botname))
            {
                Console.WriteLine("Refreshing bot access token...");
                botAccessToken = authenticator.RefreshAccessToken(BotState, ClientId, botAccessToken).Result;
                botname = authenticator.GetUsernameFromOAuthAsync(botAccessToken).Result;
            }

            userClient = new TwitchClient(new ConnectionCredentials(Username, userAccessToken), Username);
            botClient = new TwitchClient(new ConnectionCredentials(botname, botAccessToken), Username);
            twitchApi = new TwitchAPI(ClientId, botAccessToken);
            playlist = new Playlist();
            speechSynthesizer = new SpeechSynthesizer();

            userClient.OnConnected += OnUserConnected;
            userClient.OnBeingHosted += OnUserBeingHosted;

            botClient.OnConnected += OnBotConnected;
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
        
        #region User Events

        private void OnUserConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Hooked into {Username}'s account!");
        }

        private void OnUserBeingHosted(object sender, OnBeingHostedArgs e)
        {
            speechSynthesizer.SpeakAsync($"{e.HostedByChannel} began hosting you for {e.Viewers} viewers!");
            botClient.SendMessage($"Thanks for the host, @{e.HostedByChannel}!");
        }

        #endregion

        #region Bot Events

        private void OnBotConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Chat bot user connected to {Username}'s stream!");
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("T")}] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
        }

        private void OnBotMessageSent(object sender, OnMessageSentArgs e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("T")}] {e.SentMessage.DisplayName}: {e.SentMessage.Message}");
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
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
            speechSynthesizer.SpeakAsync($"Much thanks to {e.ReSubscriber.DisplayName} for re-subscribing for {e.ReSubscriber.Months} months!");
            speechSynthesizer.SpeakAsync(e.ReSubscriber.ResubMessage);
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            
        }

        #endregion
    }
}
