using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;

namespace ATCB.Library.Models.WebApi
{
    public class WebAuthenticator
    {
        private HttpClient HttpClient;
        private const string baseUrl = "http://bot.sandhead.stream/api/";

        public WebAuthenticator()
        {
            HttpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenByStateAsync(Guid state)
        {
            var url = $"{baseUrl}retrieve_token.php?state={state.ToString()}";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("For some reason, we didn't get an access token. Strange.");
        }

        public async Task<string> GetBotAccessTokenByValidStateAsync(Guid state)
        {
            var url = $"{baseUrl}retrieve_chatbottoken.php?state={state.ToString()}";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("For some reason, we didn't get an access token. Strange.");
        }

        public async Task<string> GetUsernameFromOAuthAsync(string accessToken)
        {
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_username.php?oauth={accessToken}");
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("For some reason, we didn't get a username. Strange.");
        }

        public async Task<string> RefreshAccessToken(Guid state, string clientId, string oldAccessToken)
        {
            string refreshToken, clientSecret;

            // First, we have to get the refresh token
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_refreshtoken.php?state={state.ToString()}");
            if (response.IsSuccessStatusCode)
                refreshToken = await response.Content.ReadAsStringAsync();
            else
                throw new Exception("For some reason, we didn't get a username. Strange.");

            // Next, we have to get the client secret
            response = await HttpClient.GetAsync($"{baseUrl}retrieve_clientsecret.php?state={state.ToString()}");
            if (response.IsSuccessStatusCode)
                clientSecret = await response.Content.ReadAsStringAsync();
            else
                throw new Exception("For some reason, we didn't get a username. Strange.");

            // Okay, now let's make our POST request to Twitch
            var twitchApi = new TwitchAPI(clientId);
            var refreshResponse = await twitchApi.Auth.v5.RefreshAuthTokenAsync(refreshToken, clientSecret, clientId);
            var url = $"{baseUrl}update_tokens.php?state={state.ToString()}&access={refreshResponse.AccessToken}&refresh={refreshResponse.RefreshToken}";
            var updateResponse = await HttpClient.GetAsync(url);
            if (!updateResponse.IsSuccessStatusCode)
                throw new Exception("Unable to update access & refresh tokens in database.");
            return refreshResponse.AccessToken;
        }
    }
}
