using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands.Music
{
    public class SkipCommand : Command
    {
        public SkipCommand() { }

        public override bool IsSynonym(string commandText) => commandText.Equals("skip");

        public override void Run(CommandContext context)
        {
            if (context.ChatMessage.IsModeratorOrBroadcaster)
                GlobalVariables.GlobalPlaylist.Skip();
        }
    }
}
