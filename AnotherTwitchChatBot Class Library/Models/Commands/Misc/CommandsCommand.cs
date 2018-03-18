using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Misc
{
    public class CommandsCommand : Command
    {
        private readonly double limit = 8.0;

        public override string[] Synonyms() { return new string[] { "commands" }; }

        public override void Run(CommandContext context)
        {
            var index = 1;
            var pageLimit = (int)Math.Round(context.Commands.Count / limit);
            if (context.ArgumentsAsList.Count > 0)
                index = int.Parse(context.ArgumentsAsList[0]);
            if (index < 1)
                index = 1;

            string list = "";
            for (int i = (int)limit * (index - 1); i < Math.Min(limit * index, context.Commands.Count); i++)
            {
                list += $"!{context.Commands[i]}";
                if (i != Math.Min(limit * index, context.Commands.Count) - 1)
                    list += ", ";
            }

            context.SendMessage($"Commands: {list} [Page {index}/{pageLimit}]");
        }
    }
}
