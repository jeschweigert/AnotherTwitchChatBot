using ATCB.Library.Models.Music;
using ATCB.Library.Models.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Twitch
{
    public class TwitchChatBot
    {
        private static readonly string ClientId = "r0rcrtf3wfququa8p1nkhsttam2io1";

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

            botClient = new TwitchClient(new ConnectionCredentials(botname, botAccessToken), Username);
            userClient = new TwitchClient(new ConnectionCredentials(Username, userAccessToken), Username);
            twitchApi = new TwitchAPI(ClientId, botAccessToken);
            playlist = new Playlist();
        }

        public void Start()
        {
            botClient.Connect();
            userClient.Connect();
            IsConnected = true;
        }
    }
}
