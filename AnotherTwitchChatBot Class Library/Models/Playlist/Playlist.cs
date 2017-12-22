using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Playlist
{
    public class Playlist : IEnumerable
    {
        public Queue<RequestedSong> RequestedSongs;
        public List<PreexistingSong> Songs;
        private int current = -1;

        public Playlist()
        {
            RequestedSongs = new Queue<RequestedSong>();
            Songs = new List<PreexistingSong>();
        }

        /// <summary>
        /// Gets the next song to be played, in queue or otherwise.
        /// </summary>
        /// <returns>The song to be played next.</returns>
        public Song GetNext()
        {
            if (RequestedSongs.Count > 0 && RequestedSongs.Peek().IsDownloaded)
                return RequestedSongs.Dequeue();
            try
            {
                return Songs[++current];
            }
            catch (Exception)
            {
                current = 0;
                return Songs[current];
            }
        }

        /// <summary>
        /// Gets the previously played song.
        /// </summary>
        /// <returns>The previously played preexisting song.</returns>
        public Song GetPrevious()
        {
            try
            {
                return Songs[--current];
            }
            catch (Exception)
            {
                current = Songs.Count() - 1;
                return Songs[current];
            }
        }

        /// <summary>
        /// Starts downloading the next requested song in queue.
        /// </summary>
        /// <returns></returns>
        public async Task DownloadNextInQueueAsync()
        {
            if (!RequestedSongs.Peek().IsDownloaded)
                await RequestedSongs.Peek().DownloadAsync();
        }

        public IEnumerator GetEnumerator()
        {
            return new SongEnumerator(Songs);
        }
    }
}
