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
    public class PlayCommand : Command
    {
        public PlayCommand() { }

        public override bool IsSynonym(string commandText) => commandText.Equals("play");

        public override void Run(ChatCommand context, TwitchClient client)
        {
            GlobalVariables.GlobalPlaylist.Play();
        }
    }
}
