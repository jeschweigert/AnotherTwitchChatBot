using ATCB.Library.Models.Giveaways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public class GiveawayCommand : Command
    {
        private string[] synonyms = { "giveaway" };
        private Giveaway giveaway;

        public GiveawayCommand() { }

        public override bool IsSynonym(string commandText) => synonyms.Contains(commandText);

        public override void Run(ChatCommand context, TwitchClient client)
        {
            if (context.ArgumentsAsList.Count > 0 && (context.ChatMessage.IsModerator || context.ChatMessage.IsBroadcaster))
            {
                switch (context.ArgumentsAsList[0])
                {
                    case "start":
                        {
                            if (giveaway == null)
                            {
                                giveaway = new ViewerGiveaway();
                                giveaway.Start();
                                client.SendMessage("Starting a giveaway! Type \"!giveaway\" to enter!");
                            }
                        }
                        break;
                    case "end":
                        {
                            client.SendMessage($"Giveaway ended! The lucky winner is... @{giveaway.End()}!");
                            giveaway = null;
                        }
                        break;
                    default:
                        client.SendMessage($"\"{context.ArgumentsAsString}\" is not a valid argument for the command \"{context.CommandText}\".");
                        break;
                }
            }
            else
            {
                if (giveaway != null)
                {
                    giveaway.AddName(context.ChatMessage.DisplayName);
                }
            }
        }

        public void Start()
        {

        }

        public void End()
        {

        }
    }
}
