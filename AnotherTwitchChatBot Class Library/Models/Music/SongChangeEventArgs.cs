using ATCB.Library.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Music
{
    public class SongChangeEventArgs : EventArgs
    {
        public SongChangeEventArgs(Song song) : base()
        {
            Song = song;
        }

        public Song Song { get; private set; }
    }
}
