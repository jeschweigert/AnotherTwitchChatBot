using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Helpers
{
    public class ConsoleCommandEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public ConsoleCommandEventArgs(string message)
        {
            Message = message;
        }
    }
}
