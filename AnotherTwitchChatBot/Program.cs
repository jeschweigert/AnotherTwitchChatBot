using ATCB.Library.Models.Twitch;
using ATCB.Library.Models.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB
{
    class Program
    {
        private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string OAuthUrl = "https://api.twitch.tv/kraken/oauth2/authorize?response_type=code&client_id=r0rcrtf3wfququa8p1nkhsttam2io1&redirect_uri=http%3A%2F%2Fbot.sandhead.stream%2Fapi%2Fclient_auth.php&scope=channel_check_subscription+channel_commercial+channel_editor+channel_feed_edit+channel_feed_read+channel_read+channel_stream+channel_subscriptions+chat_login+collections_edit+communities_edit+communities_moderate+openid+user_blocks_edit+user_blocks_read+user_follows_edit+user_read+user_subscriptions+viewing_activity_read";
        
        private static WebAuthenticator Authenticator;
        private static TwitchChatBot ChatBot;
        
        private static Guid AppState;

        static void Main(string[] args)
        {
            Authenticator = new WebAuthenticator();
            if (!File.Exists($"{AppDirectory}setupcomplete.txt"))
            {
                FirstTimeSetup();
            }
            
            // TODO: create json/xml settings file instead of this
            var FileContents = File.ReadAllText($"{AppDirectory}setupcomplete.txt");
            AppState = Guid.Parse(FileContents);

            Console.WriteLine("Grabbing credentials from database...");
            ChatBot = new TwitchChatBot(Authenticator, AppState);
            Console.WriteLine("Connecting to Twitch...");
            ChatBot.Start();

            while (ChatBot.IsConnected) { }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static void FirstTimeSetup()
        {
            var HasNotAuthenticated = true;
            AppState = Guid.NewGuid();

            Console.WriteLine("Hi there! It looks like you're starting ATCB for the first time.");
            Console.WriteLine("Just so you know, I'm going to need some permissions from your Twitch account to run correctly.");
            while (HasNotAuthenticated)
            {
                Console.WriteLine("I'll open up the authentication page in your default browser, press any key once you've successfully authenticated.");
                Thread.Sleep(5000);
                System.Diagnostics.Process.Start($"{OAuthUrl}&state={AppState.ToString()}");
                Console.ReadKey(true);
                Console.WriteLine("Checking for authentication...");
                try
                {
                    var AccessToken = Authenticator.GetAccessTokenByStateAsync(AppState).Result;
                    HasNotAuthenticated = false;
                }
                catch (Exception)
                {
                    Console.WriteLine("Authentication failed, uh oh. Let's send you back to Twitch's authentication page and try again!");
                }
            }
            Console.WriteLine("Neat-o! We've hooked ourselves an access token! Look's like you're all good to go!");
            File.WriteAllText($"{AppDirectory}setupcomplete.txt", AppState.ToString());
        }
    }
}
