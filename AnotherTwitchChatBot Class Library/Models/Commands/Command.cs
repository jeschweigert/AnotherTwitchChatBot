using ATCB.Library.Models.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public abstract class Command
    {
        public bool ContainsSeveralCommands = false;

        public UserType MustBeThisTallToRide = UserType.Default;

        public abstract string[] Synonyms();

        public abstract void Run(CommandContext context);
    }
}
