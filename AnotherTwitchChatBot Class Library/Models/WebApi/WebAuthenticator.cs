using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<string> GetUsernameFromOAuthAsync(string accessToken)
        {
            var response = await HttpClient.GetAsync($"{baseUrl}retrieve_username.php?oauth={accessToken}");
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new Exception("For some reason, we didn't get a username. Strange.");
        }
    }
}
