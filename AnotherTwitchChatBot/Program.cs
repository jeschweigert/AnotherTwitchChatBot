using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Music;
using ATCB.Library.Models.Settings;
using ATCB.Library.Models.Twitch;
using ATCB.Library.Models.WebApi;
using Colorful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private static SpotifySongTracker Spotify;

        private static ApplicationSettings Settings;

        static void Main(string[] args)
        {
            Authenticator = new WebAuthenticator();
            Settings = new ApplicationSettings();
            if (!Settings.Exists())
            {
                FirstTimeSetup();
            }
            Settings = Settings.Load();
            GlobalVariables.AppSettings = Settings;

            if (Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle)) != null)
            {
                ConsoleHelper.WriteLine("Hooking into Spotify...");
                Spotify = new SpotifySongTracker();
                Spotify.OnSongUpdate += OnSpotifySongChanged;
            }

            ConsoleHelper.WriteLine("Grabbing credentials from database...");
            ChatBot = new TwitchChatBot(Authenticator, Settings.AppState);
            ConsoleHelper.WriteLine("Connecting to Twitch...");
            ChatBot.Start();
            
            GlobalVariables.GlobalPlaylist.OnSongChanged += OnPlaylistSongChanged;
            if (Spotify != null && Settings.PlaylistLocation != null && Directory.Exists(Settings.PlaylistLocation))
            {
                ConsoleHelper.WriteLine("Loading the playlist...");
                GlobalVariables.GlobalPlaylist.LoadFromFolder(Settings.PlaylistLocation);
                GlobalVariables.GlobalPlaylist.Shuffle();
                GlobalVariables.GlobalPlaylist.Play();
            }

            object locker = new object();
            List<char> charBuffer = new List<char>();

            while (ChatBot.IsConnected) {
                var key = ConsoleHelper.ReadKey();
            }

            ConsoleHelper.WriteLine("Press any key to exit...");
            System.Console.ReadKey(true);
        }

        private static void OnSpotifySongChanged(string data)
        {
            using (StreamWriter writetext = new StreamWriter($"{AppDirectory}current_song.txt"))
            {
                writetext.WriteLine($"{data}                    ");
                ConsoleHelper.WriteLine($"Now Playing: {data}");
            }
        }

        private static void OnPlaylistSongChanged(object sender, SongChangeEventArgs e)
        {
            using (StreamWriter writetext = new StreamWriter($"{AppDirectory}current_song.txt"))
            {
                writetext.WriteLine($"{e.Song.Artist} - {e.Song.Title}                    ");
            }
        }

        private static void FirstTimeSetup()
        {
            var HasNotAuthenticated = true;
            Settings.AppState = Guid.NewGuid();

            ConsoleHelper.WriteLine("Hi there! It looks like you're starting ATCB for the first time.");
            ConsoleHelper.WriteLine("Just so you know, I'm going to need some permissions from your Twitch account to run correctly.");
            while (HasNotAuthenticated)
            {
                ConsoleHelper.WriteLine("I'll open up the authentication page in your default browser, press any key once you've successfully authenticated.");
                Thread.Sleep(5000);
                System.Diagnostics.Process.Start($"{OAuthUrl}&state={Settings.AppState.ToString()}");
                System.Console.ReadKey(true);
                ConsoleHelper.WriteLine("Checking for authentication...");
                try
                {
                    var AccessToken = Authenticator.GetAccessTokenByStateAsync(Settings.AppState).Result;
                    HasNotAuthenticated = false;
                }
                catch (Exception)
                {
                    ConsoleHelper.WriteLine("Authentication failed, uh oh. Let's send you back to Twitch's authentication page and try again!");
                }
            }
            ConsoleHelper.WriteLine("Neat-o! We've hooked ourselves an access token! Look's like you're all good to go!");
            Settings.Save();
        }
    }
}
