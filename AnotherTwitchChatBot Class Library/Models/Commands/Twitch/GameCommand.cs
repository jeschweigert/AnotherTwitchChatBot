using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Twitch
{
    public class GameCommand : Command
    {
        public override bool IsSynonym(string commandText) => commandText == "game";

        public override void Run(CommandContext context)
        {
            context.SendMessage($"The current game is \"{context.TwitchStream.Game}\".");
        }
    }
}
