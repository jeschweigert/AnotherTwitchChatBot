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

namespace ATCB.Library.Models.Commands
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
                client.SendMessage("Sorry! Requesting songs by query doesn't quite work yet, but requesting by URL does!");
            }
            if (!string.IsNullOrEmpty(videoId))
            {
                var video = this.client.GetVideoAsync(videoId).Result;
                client.SendMessage($"@{context.ChatMessage.DisplayName} Your request, \"{video.Title}\", is #{GlobalVariables.GlobalPlaylist.RequestedSongCount + 1} in the queue!");
                GlobalVariables.GlobalPlaylist.Enqueue(new RequestedSong(videoId, context.ChatMessage.DisplayName));
            }
        }
    }
}
