using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Misc
{
    public class CommandsCommand : Command
    {
        public override bool IsSynonym(string commandText) => commandText == "commands";

        public override void Run(CommandContext context)
        {
            context.SendMessage($"@{context.ChatMessage.DisplayName} The list of commands can be found here: https://github.com/sand-head/AnotherTwitchChatBot/wiki/Commands");
        }
    }
}
