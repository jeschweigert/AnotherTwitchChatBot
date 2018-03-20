using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands
{
    public class CommandFactory
    {
        private List<Command> Commands;

        public CommandFactory()
        {
            Commands = new List<Command>();

            // Check to see if plugins folder exists, then try loading
            if (Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}plugins"))
            {
                ConsoleHelper.WriteLine("Plugins folder found, attempting to load...");
                var directoryInfo = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}plugins");
                int pluginCount = 0;
                foreach (var plugin in directoryInfo.GetFiles("*.dll"))
                {
                    var DLL = Assembly.LoadFile(plugin.FullName);

                    foreach (Type type in DLL.GetExportedTypes().Where(x => typeof(Command).IsAssignableFrom(x)))
                    {
                        var c = Activator.CreateInstance(type) as Command;
                        Commands.Add(c);
                    }

                    pluginCount++;
                }

                ConsoleHelper.WriteLine($"Added {pluginCount} plugin(s).");
            }

            var commandsEnum = ReflectiveEnumerator.GetEnumerableOfType<Command>();
            foreach (Command c in commandsEnum)
            {
                Commands.Add(c);
            }

            ConsoleHelper.WriteLine($"Added {Commands.Count} command(s).");
        }

        public Command GetCommand(string commandName)
        {
            return Commands.Where(x => x.Synonyms().Contains(commandName)).FirstOrDefault();
        }

        public List<string> ToList()
        {
            List<string> commands = new List<string>();
            foreach (var command in Commands)
            {
                if (command.ContainsSeveralCommands)
                    commands.AddRange(command.Synonyms());
                else
                    commands.Add(command.Synonyms()[0]);
            }
            return commands.OrderBy(x => x).ToList();
        }
    }
}
