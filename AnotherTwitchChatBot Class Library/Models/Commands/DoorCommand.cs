using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public class DoorCommand : Command
    {
        public override string[] Synonyms() { return new string[] { "door" }; }

        public override void Run(CommandContext context)
        {
            context.SendMessage("Command \"🚪\" was not found.");
        }
    }
}
