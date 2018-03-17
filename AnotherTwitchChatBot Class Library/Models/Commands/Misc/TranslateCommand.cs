using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Misc
{
    public class TranslateCommand : Command
    {
        public override bool IsSynonym(string commandText) => commandText.Equals("translate");

        public override void Run(CommandContext context)
        {
            Translator t = new Translator();
            t.Translate(context.ArgumentsAsString, "auto|en");
            while (t.IsComplete != true) { }
            context.SendMessage(t.Result.Text);
        }
    }
}
