using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public class CurrentSongCommand : Command
    {
        private string[] synonyms = { "song", "currentsong" };

        public CurrentSongCommand() { }

        public override bool IsSynonym(string commandText) => synonyms.Contains(commandText);

        public override void Run(ChatCommand context, TwitchClient client)
        {
            client.SendMessage($"The current song is \"{GlobalVariables.GlobalPlaylist.CurrentSong.Title}\" by {GlobalVariables.GlobalPlaylist.CurrentSong.Artist}");
        }
    }
}
