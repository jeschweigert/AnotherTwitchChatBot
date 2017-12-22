using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Playlist
{
    public abstract class Song
    {
        /// <summary>
        /// Title of the song.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Artist of the song.
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// Path to the song's MP3 file.
        /// </summary>
        public string FilePath { get; set; }
    }
}
