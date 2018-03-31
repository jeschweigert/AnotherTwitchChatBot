using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Twitch
{
    public class UptimeCommand : Command
    {
        public override string[] Synonyms() { return new string[] { "uptime" }; }

        public override void Run(CommandContext context)
        {
            using (WebClient client = new WebClient())
            {
                var time = client.DownloadString($"https://decapi.me/twitch/uptime/{context.TwitchStream.Username}");
                if (time.Contains("offline"))
                    context.SendMessage($"{time}.");
                else
                    context.SendMessage($"{context.TwitchStream.Username} has been live for {time}.");
            }
        }
    }
}
