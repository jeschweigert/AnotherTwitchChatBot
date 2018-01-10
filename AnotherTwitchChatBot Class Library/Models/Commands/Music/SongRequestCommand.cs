using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using YoutubeExplode;
using YoutubeSearch;

namespace ATCB.Library.Models.Commands.Music
{
    public class SongRequestCommand : Command
    {
        private string[] synonyms = { "songrequest", "sr", "songreq" };
        private YoutubeClient client;

        public SongRequestCommand()
        {
            client = new YoutubeClient();
        }

        public override bool IsSynonym(string commandText) => synonyms.Contains(commandText);

        public override void Run(ChatCommand context, TwitchClient client)
        {
            string videoId = "";
            if (context.ArgumentsAsList.Count < 1)
            {
                throw new Exception("Invalid parameters");
            }
            else if (context.ArgumentsAsList.Count > 1 || YoutubeClient.TryParseVideoId(context.ArgumentsAsList[0], out videoId) == false)
            {
                // Request by YouTube query
                var search = new VideoSearch();
                var video = search.SearchQuery(context.ArgumentsAsString, 1).Where(x => TimeSpanHelper.ConvertDurationToTimeSpan(x.Duration).TotalMinutes < 8.0).FirstOrDefault();
                if (video != null && YoutubeClient.TryParseVideoId(video.Url, out videoId))
                {
                    MakeRequest(videoId, context, client);
                }
                else
                {
                    client.SendMessage("Sorry! I couldn't find a song like the one you wanted!");
                }
            }
            else if (!string.IsNullOrEmpty(videoId))
            {
                // Request by YouTube URL
                MakeRequest(videoId, context, client);
            }
        }

        private void MakeRequest(string videoId, ChatCommand context, TwitchClient client)
        {
            var video = this.client.GetVideoAsync(videoId).Result;
            client.SendMessage($"@{context.ChatMessage.DisplayName} Your request, \"{video.Title}\", is #{GlobalVariables.GlobalPlaylist.RequestedSongCount + 1} in the queue!");
            GlobalVariables.GlobalPlaylist.Enqueue(new RequestedSong(videoId, context.ChatMessage.DisplayName));
        }
    }
}
