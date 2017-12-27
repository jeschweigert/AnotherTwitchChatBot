using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Id3.Id3v2;

namespace ATCB.Library.Models.Music
{
    public class Playlist : IEnumerable
    {
        private Queue<RequestedSong> RequestedSongs;
        private List<PreexistingSong> Songs;
        private IWavePlayer waveOutDevice;
        private WaveStream audioFileReader;
        private SampleChannel sampleChannel;
        private int current = -1;

        /// <summary>
        /// Initializes a new Playlist object, which regulates both PreexistingSongs and RequestedSongs.
        /// </summary>
        public Playlist()
        {
            RequestedSongs = new Queue<RequestedSong>();
            Songs = new List<PreexistingSong>();
            waveOutDevice = new WaveOutEvent();
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/downloads");
        }

        /// <summary>
        /// The number of songs currently in the request queue.
        /// </summary>
        public int RequestedSongCount => RequestedSongs.Count;

        /// <summary>
        /// Gets all media files in a folder and adds them to the playlist.
        /// </summary>
        /// <param name="filePath">The path to the folder.</param>
        public void LoadFromFolder(string filePath)
        {
            DirectoryInfo d = new DirectoryInfo(filePath);
            foreach (var file in d.GetFiles())
            {
                var tag = new Id3.Mp3File(file.FullName);
                var title = tag.GetAllTags().FirstOrDefault(x => !string.IsNullOrEmpty(x.Title))?.Title ?? file.Name;
                var artists = string.Join(", ", tag.GetAllTags().FirstOrDefault(x => x.Artists != null)?.Artists.Value ?? new string[] { });
                Enlist(new PreexistingSong(title, artists, file.FullName));
            }
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
            if (RequestedSongs.Count == 1)
                DownloadNextInQueueAsync().GetAwaiter().GetResult();
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
        /// Shuffles the songs currently in the playlist, not including song requests.
        /// </summary>
        public void Shuffle()
        {
            Random r = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
            int n = Songs.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                PreexistingSong temp = Songs[k];
                Songs[k] = Songs[n];
                Songs[n] = temp;
            }
        }

        public void Start()
        {
            var song = GetNext();
            audioFileReader = new AudioFileReader(song.FilePath);
            sampleChannel = new SampleChannel(audioFileReader);
            sampleChannel.Volume = 0.125f;
            waveOutDevice.Init(sampleChannel);
            waveOutDevice.PlaybackStopped += (sender, e) => { PlayNext(); };
            waveOutDevice.Play();
            ConsoleHelper.WriteLine($"Now Playing: \"{song.Title}\" by {song.Artist}");
        }

        /// <summary>
        /// Plays the playlist.
        /// </summary>
        public void Play()
        {
            if (audioFileReader == null)
                Start();
            else
                waveOutDevice.Play();
        }

        public void Pause()
        {
            waveOutDevice.Pause();
        }

        /// <summary>
        /// Skips to the next song.
        /// </summary>
        public void Skip()
        {
            waveOutDevice.Stop();
            while (waveOutDevice.PlaybackState != PlaybackState.Stopped) { }
        }

        private void PlayNext()
        {
            waveOutDevice.Stop();
            waveOutDevice.Dispose();
            audioFileReader.Dispose();

            var song = GetNext();
            waveOutDevice = new WaveOutEvent();
            audioFileReader = new AudioFileReader(song.FilePath);
            sampleChannel = new SampleChannel(audioFileReader);
            sampleChannel.Volume = 0.125f;
            waveOutDevice.Init(sampleChannel);
            waveOutDevice.PlaybackStopped += (sender, e) => { PlayNext(); };
            waveOutDevice.Play();
            ConsoleHelper.WriteLine($"Now Playing: \"{song.Title}\" by {song.Artist}");
            if (RequestedSongs.Count > 0)
                DownloadNextInQueueAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Starts downloading the next requested song in queue.
        /// </summary>
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
