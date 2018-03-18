using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands
{
    public class SettingsCommand : Command
    {
        public override string[] Synonyms() { return new string[] { "settings" }; }

        public override void Run(CommandContext context)
        {
            if (!context.ChatMessage.IsChatBot)
            {
                context.SendMessage("The !settings command can only be used from within the console itself.");
                return;
            }

            if (context.ArgumentsAsList.Count > 1)
            {
                if (context.ArgumentsAsList[0] == "songrequests")
                {
                    if (context.ArgumentsAsList[1] == "enable")
                    {
                        context.SendMessage("Song requests have been enabled.");
                        context.Settings.SongRequests = true;
                        context.Settings.Save();
                    }
                    else if (context.ArgumentsAsList[1] == "disable")
                    {
                        context.SendMessage("Song requests have been disabled.");
                        context.Settings.SongRequests = false;
                        context.Settings.Save();
                    }
                }
            }
        }
    }
}
