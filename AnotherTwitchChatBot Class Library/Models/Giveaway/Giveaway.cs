using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Giveaway
{
    /// <summary>
    /// An abstract class representing a time-sensitive Twitch chat giveaway.
    /// </summary>
    public abstract class Giveaway
    {
        public abstract void Start();

        public abstract bool IsInProgress();

        public abstract void AddName(string name);

        public abstract void End();
    }
}
