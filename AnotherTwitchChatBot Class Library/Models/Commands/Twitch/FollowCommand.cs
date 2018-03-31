using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Twitch
{
    public class FollowCommand : Command
    {
        public override void Run(CommandContext context)
        {
            using (WebClient client = new WebClient())
            {
                if (context.ArgumentsAsList.Count > 0)
                {
                    var time = client.DownloadString($"https://decapi.me/twitch/followage/{context.TwitchStream.Username}/{context.ArgumentsAsList[0]}");
                    if (time == "Follow not found")
                        context.SendMessage($"{context.ArgumentsAsList[0]} is not following {context.TwitchStream.Username}.");
                    else if (time == "A user cannot follow themself.")
                        context.SendMessage($"{context.TwitchStream.Username} can't follow themselves.");
                    else
                        context.SendMessage($"{context.ArgumentsAsList[0]} has followed {context.TwitchStream.Username} for {time}.");
                }
                else
                {
                    var time = client.DownloadString($"https://decapi.me/twitch/followage/{context.TwitchStream.Username}/{context.ChatMessage.Username}");
                    if (time == "Follow not found")
                        context.SendMessage($"{context.ChatMessage.Username} is not following {context.TwitchStream.Username}.");
                    else if (time == "A user cannot follow themself.")
                        context.SendMessage($"{context.TwitchStream.Username} can't follow themselves.");
                    else
                        context.SendMessage($"{context.ChatMessage.Username} has followed {context.TwitchStream.Username} for {time}.");
                }
            }
        }

        public override string[] Synonyms()
        {
            return new string[] { "follow", "followage" };
        }
    }
}
