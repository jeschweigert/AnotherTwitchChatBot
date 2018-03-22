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

        public async Task<AuthenticationDetails> GetUserAuthenticationDetails(Guid state)
        {
            var url = $"{baseUrl}get_clientinfo.php?state={state.ToString()}";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<AuthenticationDetails>(await response.Content.ReadAsStringAsync());
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new Exception("Could not obtain auth details. Check if client secret is valid.");
            else
                throw new Exception("Could not obtain auth details. Check if state is correct or if database is down.");
        }

        /// <summary>
        /// Grabs the relevant access token from the database using a related app state.
        /// </summary>
        /// <param name="state">The user's app state.</param>
        /// <returns>A Twitch access token.</returns>
        public async Task<string> GetAccessTokenByStateAsync(Guid state)
        {
            var url = $"{baseUrl}retrieve_token.php?state={state.ToString()}";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new Exception("Could not obtain access token. Check if client secret is valid.");
            else
                throw new Exception("Could not obtain access token. Check if state is correct or if database is down.");
        }

        /// <summary>
        /// Grabs the relevant refresh token from the database using a related app state.
        /// </summary>
        /// <param name="state">The user's app state.</param>
        /// <returns>A Twitch refresh token.</returns>
        public async Task<string> GetRefreshTokenByStateAsync(Guid state)
        {
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_refreshtoken.php?state={state.ToString()}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                throw new Exception("Could not obtain refresh token. Check if state is correct or if database is down.");
        }

        /// <summary>
        /// Grabs the bot access token from the database using a valid app state.
        /// </summary>
        /// <param name="state">Any valid app state.</param>
        /// <returns>A Twitch access token.</returns>
        public async Task<string> GetBotAccessTokenByValidStateAsync(Guid state)
        {
            var url = $"{baseUrl}retrieve_chatbottoken.php?state={state.ToString()}";
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("Could not obtain bot access token. Check if state is correct or if database is down.");
        }

        /// <summary>
        /// Grabs the username of the user through their access token.
        /// </summary>
        /// <param name="state">The user's access token.</param>
        /// <returns>A Twitch username.</returns>
        public async Task<string> GetUsernameFromOAuthAsync(string accessToken)
        {
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_username.php?oauth={accessToken}");
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("Could not recieve username from Twitch, token may be expired.");
        }

        public async Task<string> GetClientSecretByValidStateAsync(Guid state)
        {
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_clientsecret.php?state={state.ToString()}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                throw new Exception("Could not obtain client secret.");
        }

        public async Task UpdateAccessAndRefreshTokens(Guid state, string accessToken, string refreshToken)
        {
            var updateResponse = await HttpClient.GetAsync($"{baseUrl}update_tokens.php?state={state.ToString()}&access={accessToken}&refresh={refreshToken}");
            if (!updateResponse.IsSuccessStatusCode)
                throw new Exception("Unable to update access & refresh tokens in database.");
        }
    }
}
