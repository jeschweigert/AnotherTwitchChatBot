using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Playlist
{
    public class PreexistingSong : Song
    {
        public PreexistingSong()
        {
            Title = null;
            Artist = null;
            FilePath = null;
        }
        public PreexistingSong(string Title, string Artist, string FilePath)
        {
            this.Title = Title;
            this.Artist = Artist;
            this.FilePath = FilePath;
        }
    }
}
