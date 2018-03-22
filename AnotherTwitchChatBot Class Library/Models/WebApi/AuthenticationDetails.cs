using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.WebApi
{
    public class AuthenticationDetails
    {
        public string ClientSecret { get; set; }
        public UserDetails UserDetails { get; set; }
        public BotDetails BotDetails { get; set; }
        public DiscordDetails DiscordDetails { get; set; }
    }

    public class UserDetails
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresIn { get; set; }
        public string Scopes { get; set; }
    }

    public class BotDetails
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class DiscordDetails
    {
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string GuildId { get; set; }
    }
}
