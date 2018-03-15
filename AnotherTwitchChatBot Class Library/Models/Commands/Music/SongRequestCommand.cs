using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public override void Run(CommandContext context)
        {
            if (!GlobalVariables.GlobalPlaylist.AcceptRequests)
            {
                context.SendMessage("Requests aren't being taken at the moment, sorry!");
                return;
            }

            if (context.ArgumentsAsList.Count < 1)
            {
                context.SendMessage("You didn't give me anything to add.");
            }
            else
            {
                var success = YoutubeClient.TryParseVideoId(context.ArgumentsAsString, out string videoId);

                if (!success)
                {
                    // Request by YouTube query
                    // TODO: USE https://beta.decapi.me/youtube/videoid?search=QUERY INSTEAD
                    using (WebClient client = new WebClient())
                    {
                        videoId = client.DownloadString($"https://beta.decapi.me/youtube/videoid?search={context.ArgumentsAsString}");
                        MakeRequest(videoId, context);
                    }
                }
                else
                {
                    // Request by YouTube URL
                    MakeRequest(videoId, context);
                }
            }
        }

        private void MakeRequest(string videoId, CommandContext context)
        {
            var videoTitle = Task.Run(() => this.client.GetVideoAsync(videoId)).Result.Title;
            context.SendMessage($"@{context.ChatMessage.DisplayName} Your request, \"{videoTitle}\", is #{GlobalVariables.GlobalPlaylist.RequestedSongCount + 1} in the queue!");
            GlobalVariables.GlobalPlaylist.Enqueue(new RequestedSong(videoId, context.ChatMessage.DisplayName));
        }
    }
}
