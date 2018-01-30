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
    public class PauseCommand : Command
    {
        public PauseCommand() { }

        public override bool IsSynonym(string commandText) => commandText.Equals("pause");

        public override void Run(CommandContext context)
        {
            if (context.ChatMessage.IsBroadcaster || context.ChatMessage.IsChatBot)
            {
                GlobalVariables.GlobalPlaylist.Pause();
                context.SendMessage("The playlist has been paused.");
            }
        }
    }
}
