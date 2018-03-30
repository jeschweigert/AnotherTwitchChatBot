using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands
{
    public class SettingsCommand : Command
    {
        public SettingsCommand()
        {
            MustBeThisTallToRide = Models.Twitch.UserType.Chat_Bot;
        }

        public override string[] Synonyms() { return new string[] { "settings" }; }

        public override void Run(CommandContext context)
        {
            if (context.ArgumentsAsList.Count > 2)
            {
                if (context.ArgumentsAsList[0] == "friends")
                {
                    if (context.ArgumentsAsList[1] == "add")
                    {
                        if (context.Settings.TwitchFriends == null)
                            context.Settings.TwitchFriends = new List<string>();
                        if (context.Settings.TwitchFriends.Contains(context.ArgumentsAsList[2].ToLower()))
                        {
                            context.SendMessage($"User \"{context.ArgumentsAsList[2].ToLower()}\" is already listed as a friend.");
                            return;
                        }
                        context.SendMessage($"Added user \"{context.ArgumentsAsList[2].ToLower()}\" as a friend.");
                        context.Settings.TwitchFriends.Add(context.ArgumentsAsList[2].ToLower());
                        context.Settings.Save();
                    }
                    else if (context.ArgumentsAsList[1] == "remove")
                    {
                        if (context.Settings.TwitchFriends == null)
                            context.Settings.TwitchFriends = new List<string>();
                        if (!context.Settings.TwitchFriends.Contains(context.ArgumentsAsList[2].ToLower()))
                        {
                            context.SendMessage($"User \"{context.ArgumentsAsList[2].ToLower()}\" isn't listed as a friend of yours.");
                            return;
                        }
                        context.SendMessage($"Removed user \"{context.ArgumentsAsList[2].ToLower()}\" from your friends.");
                        context.Settings.TwitchFriends.Remove(context.ArgumentsAsList[2].ToLower());
                        context.Settings.Save();
                    }
                }
            }
            else if (context.ArgumentsAsList.Count > 1)
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
                else if (context.ArgumentsAsList[0] == "sfx")
                {
                    if (context.ArgumentsAsList[1] == "enable")
                    {
                        context.SendMessage("Sound effects have been enabled.");
                        context.Settings.SoundEffects = true;
                        context.Settings.Save();
                    }
                    else if (context.ArgumentsAsList[1] == "disable")
                    {
                        context.SendMessage("Sound effects have been disabled.");
                        context.Settings.SoundEffects = false;
                        context.Settings.Save();
                    }
                }
                else if (context.ArgumentsAsList[0] == "spokenalerts")
                {
                    if (context.ArgumentsAsList[1] == "enable")
                    {
                        context.SendMessage("Spoken alerts have been enabled.");
                        context.Settings.SpokenAlerts = true;
                        context.Settings.Save();
                    }
                    else if (context.ArgumentsAsList[1] == "disable")
                    {
                        context.SendMessage("Spoken alerts have been disabled.");
                        context.Settings.SpokenAlerts = false;
                        context.Settings.Save();
                    }
                }
            }
        }
    }
}
