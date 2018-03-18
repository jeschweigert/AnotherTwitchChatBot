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
    public class PlayCommand : Command
    {
        public override string[] Synonyms() { return new string[] { "play" }; }

        public override void Run(CommandContext context)
        {
            if (context.ChatMessage.IsChatBot)
            {
                GlobalVariables.GlobalPlaylist.Play();
                context.SendMessage("The playlist is now playing.");
            }
            else
            {
                context.SendMessage("The !play command can only be used from within the console itself.");
            }
        }
    }
}
