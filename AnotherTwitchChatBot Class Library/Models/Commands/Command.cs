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
        public abstract void Run(ChatCommand context, TwitchClient client);

        public abstract bool IsSynonym(string commandText);
    }
}
