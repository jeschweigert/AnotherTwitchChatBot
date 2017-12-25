using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using YoutubeExplode;

namespace ATCB.Library.Models.Commands
{
    public class SongRequestCommand : Command
    {
        private string[] synonyms = { "songrequest", "sr", "songreq" };
        private YoutubeClient client;

        public SongRequestCommand()
        {
            client = new YoutubeClient();
        }

        public override bool IsSynonym(string commandText) => synonyms.Contains(commandText);

        public override void Run(ChatCommand context, TwitchClient client)
        {
            /*if (context.ArgumentsAsList.Count < 1)
            {

            }*/
            client.SendMessage("Sorry, song requests aren't implemented yet! ");
        }
    }
}
