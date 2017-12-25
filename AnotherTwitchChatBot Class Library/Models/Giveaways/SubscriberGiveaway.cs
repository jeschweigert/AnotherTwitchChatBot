using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Giveaways
{
    public class SubscriberGiveaway : Giveaway
    {
        private List<string> RaffleNames;

        public SubscriberGiveaway()
        {
            RaffleNames = new List<string>();
        }

        public override void AddName(string name)
        {
            // TODO: validate user subs to current channel
            if (!RaffleNames.Contains(name))
                RaffleNames.Add(name);
        }

        public override bool AllowEntries()
        {
            throw new NotImplementedException();
        }

        public override string End()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }
    }
}
