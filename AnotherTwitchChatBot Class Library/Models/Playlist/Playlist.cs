using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Music
{
    public class Playlist : IEnumerable
    {
        private Queue<RequestedSong> RequestedSongs;
        private List<PreexistingSong> Songs;
        private int current = -1;

        /// <summary>
        /// Initializes a new Playlist object, which regulates both PreexistingSongs and RequestedSongs.
        /// </summary>
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
        /// Adds a new song into the song request queue.
        /// </summary>
        /// <param name="song">A RequestedSong object.</param>
        public void Enqueue(Song song)
        {
            if (!(song is RequestedSong))
                throw new ArgumentException($"Song \"{song.Title}\" was not of type RequestedSong.");
            RequestedSongs.Enqueue(song as RequestedSong);
        }

        /// <summary>
        /// Adds a song to the playlist.
        /// </summary>
        /// <param name="song">A PreexistingSong object.</param>
        public void Enlist(Song song)
        {
            if (!(song is PreexistingSong))
                throw new ArgumentException($"Song \"{song.Title}\" was not of type PreexistingSong.");
            Songs.Add(song as PreexistingSong);
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
