using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Music
{
    public class SongEnumerator : IEnumerator
    {
        private Song[] songs;
        private int position = -1;

        public SongEnumerator(List<PreexistingSong> songs)
        {
            this.songs = songs.ToArray();
        }

        object IEnumerator.Current => Current;

        public Song Current
        {
            get
            {
                try
                {
                    return songs[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public bool MoveNext()
        {
            position++;
            return (position < songs.Length);
        }

        public void Reset()
        {
            position = -1;
        }
    }
}
