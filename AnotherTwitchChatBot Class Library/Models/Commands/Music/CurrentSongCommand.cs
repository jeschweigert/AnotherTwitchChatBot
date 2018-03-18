using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands.Music
{
    public class CurrentSongCommand : Command
    {
        public override string[] Synonyms() { return new string[] { "song", "currentsong" }; }

        public override void Run(CommandContext context)
        {
            var currentSong = GlobalVariables.GlobalPlaylist.CurrentSong;
            if (currentSong is RequestedSong)
                context.SendMessage($"The current song is \"{currentSong.Title}\", as requested by {((RequestedSong)currentSong).Requester}");
            else
                context.SendMessage($"The current song is \"{currentSong.Title}\" by {currentSong.Artist}");
        }
    }
}
