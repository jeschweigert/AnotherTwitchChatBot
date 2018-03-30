using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Misc
{
    public class MultiCommand : Command
    {
        private DateTime lastCheck;

        public MultiCommand()
        {
            lastCheck = DateTime.Now;
        }

        public override void Run(CommandContext context)
        {
            if (context.TwitchStream.Title.Contains("w/"))
            {

            }
            else
            {
                context.SendMessage($"@{context.ChatMessage.DisplayName} {context.TwitchStream.Username} isn't streaming with anyone at the moment.");
            }
        }

        public override string[] Synonyms()
        {
            return new string[] { "multi", "costream" };
        }
    }
}
