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
        public DoorCommand() { }

        public override bool IsSynonym(string commandText) => commandText.Equals("door");

        public override void Run(CommandContext context)
        {
            context.SendMessage("Command \"🚪\" was not found.");
        }
    }
}
