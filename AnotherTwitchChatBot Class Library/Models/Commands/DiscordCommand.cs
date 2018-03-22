using ATCB.Library.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ATCB.Library.Models.Commands
{
    public class DiscordCommand : Command
    {
        string discordOAuth = "https://discordapp.com/api/oauth2/authorize?client_id=426112428778979359&permissions=70765648&redirect_uri=http%3A%2F%2Fsandhead.stream%2Fbot%2Fapi%2Fdiscord_auth.php&response_type=code&scope=bot%20identify%20messages.read%20connections";

        public override void Run(CommandContext context)
        {
            if (context.ArgumentsAsList.Count > 0)
            {
                if (context.ArgumentsAsList[0] == "setup" && context.ChatMessage.IsChatBot)
                {
                    if (!context.Settings.Discord)
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
                else if (context.ArgumentsAsList[0] == "channel" && context.ChatMessage.IsChatBot)
                {
                    if (context.DiscordClient.GetConnectionState() == ConnectionState.Connected)
                        ConsoleHelper.WriteLine("You're already connected to Discord!");
                    else
                        context.ConnectDiscord();
                }
            }
        }

        public override string[] Synonyms()
        {
            return new string[] { "discord" };
        }
    }
}
