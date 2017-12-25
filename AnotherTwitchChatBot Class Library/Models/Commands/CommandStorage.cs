using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands
{
    public class CommandStorage
    {
        private List<Command> Commands;

        public CommandStorage()
        {
            Commands = new List<Command>();

            var commandsEnum = ReflectiveEnumerator.GetEnumerableOfType<Command>();
            foreach (Command c in commandsEnum)
            {
                Commands.Add(c);
            }
        }

        public Command GetCommand(string commandName)
        {
            return Commands.Where(x => x.IsSynonym(commandName)).FirstOrDefault();
        }
    }
}
