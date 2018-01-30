using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public class _8BallCommand : Command
    {
        private string[] synonyms = { "8ball" };
        private static string[] eightBallResponses = { "Signs point to yes.", "Yes.", "Reply hazy, try again.", "My sources say no.", "You may rely on it.", "Concentrate and ask again.", "Outlook not so good.", "It is decidedly so.", "Better not tell you now.", "Very doubtful.", "Yes - definitely.", "It is certain.", "Cannot predict now.", "Most likely.", "Ask again later.", "My reply is no.", "Outlook good.", "Don't count on it." };

        public _8BallCommand() { }

        public override bool IsSynonym(string commandText) => synonyms.Contains(commandText);

        public override void Run(CommandContext context)
        {
            Random random = new Random();
            context.SendMessage(eightBallResponses[random.Next(eightBallResponses.Length)]);
        }
    }
}
