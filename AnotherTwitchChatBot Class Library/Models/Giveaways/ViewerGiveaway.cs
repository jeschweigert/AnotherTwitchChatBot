using ATCB.Library.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Giveaways
{
    public class ViewerGiveaway : Giveaway
    {
        private List<string> RaffleNames;
        private bool allowEntries = false;

        public ViewerGiveaway()
        {
            RaffleNames = new List<string>();
        }

        public override bool AllowEntries() => allowEntries;

        public override void AddName(string name)
        {
            if (!RaffleNames.Contains(name) && allowEntries)
            {
                RaffleNames.Add(name);
                ConsoleHelper.WriteLine($"Added \"{name}\" to the giveaway.");
            }
        }

        public override string End()
        {
            allowEntries = false;
            Random rand = new Random();
            return RaffleNames[rand.Next(RaffleNames.Count)];
        }

        public override void Start()
        {
            allowEntries = true;
        }
    }
}
