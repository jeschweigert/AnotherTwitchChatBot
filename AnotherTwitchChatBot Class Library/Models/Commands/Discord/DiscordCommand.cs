using ATCB.Library.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ATCB.Library.Models.Commands.Discord
{
    public class DiscordCommand : Command
    {
        string discordOAuth = "https://discordapp.com/api/oauth2/authorize?client_id=426112428778979359&permissions=70765648&redirect_uri=http%3A%2F%2Fsandhead.stream%2Fbot%2Fapi%2Fdiscord_auth.php&response_type=code&scope=bot%20identify%20messages.read%20connections";

        public override void Run(CommandContext context)
        {
            if (context.ChatMessage.IsChatBot)
            {
                if (context.ArgumentsAsList.Count > 2)
                {
                    if (context.ArgumentsAsList[0] == "channel" && context.ChatMessage.IsChatBot)
                    {
                        if (context.ArgumentsAsList[1] == "set")
                        {
                            var channel = context.DiscordClient.GetChannels().Where(x => x.Name == context.ArgumentsAsList[2]).FirstOrDefault();
                            if (channel != null)
                            {
                                context.SendMessage($"Made {channel.Name} the default Discord text channel.");
                                context.Settings.Discord.GeneralTextChannel = channel.Id;
                                context.Settings.Save();
                            }
                            else
                            {
                                context.SendMessage($"Couldn't find a text channel by the name of \"{context.ArgumentsAsList[2]}\", please check your spelling.");
                            }
                        }
                    }
                    else if (context.ArgumentsAsList[0] == "friendalerts" && context.ChatMessage.IsChatBot)
                    {
                        if (context.ArgumentsAsList[1] == "set")
                        {
                            var channel = context.DiscordClient.GetChannels().Where(x => x.Name == context.ArgumentsAsList[2]).FirstOrDefault();
                            if (channel != null)
                            {
                                context.SendMessage($"Made {channel.Name} the Discord text channel for friend alerts.");
                                context.Settings.Discord.GeneralTextChannel = channel.Id;
                                context.Settings.Save();
                            }
                            else
                            {
                                context.SendMessage($"Couldn't find a text channel by the name of \"{context.ArgumentsAsList[2]}\", please check your spelling.");
                            }
                        }
                    }
                }
                else if (context.ArgumentsAsList.Count > 0)
                {
                    if (context.ArgumentsAsList[0] == "setup" && context.ChatMessage.IsChatBot)
                    {
                        if (!context.Settings.Discord.IsSetup)
                        {
                            ConsoleHelper.WriteLine("Opening up the Discord authentication page, follow the instructions when given a success message.");
                            System.Diagnostics.Process.Start($"{discordOAuth}&state={context.Settings.AppState.ToString()}");
                        }
                        else
                        {
                            ConsoleHelper.WriteLine("You're already connected to Discord!");
                        }
                    }
                    else if (context.ArgumentsAsList[0] == "connect" && context.ChatMessage.IsChatBot)
                    {
                        if (context.DiscordClient.GetConnectionState() == ConnectionState.Connected)
                            ConsoleHelper.WriteLine("You're already connected to Discord!");
                        else
                            context.ConnectDiscord();
                    }
                    else if (context.ArgumentsAsList[0] == "ping")
                    {
                        context.DiscordClient.SendMessage("pong!");
                    }
                }
            }
        }

        public override string[] Synonyms()
        {
            return new string[] { "discord" };
        }
    }
}
