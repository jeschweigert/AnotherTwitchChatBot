using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Giveaways
{
    /// <summary>
    /// An abstract class representing a time-sensitive Twitch chat giveaway.
    /// </summary>
    public abstract class Giveaway
    {
        public abstract bool AllowEntries();

        public abstract void Start();

        public abstract void AddName(string name);

        public abstract string End();
    }
}
