using ATCB.Library.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace ATCB.Library.Models.Music
{
    public class RequestedSong : Song
    {
        private static string APP_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The person who requested the song.
        /// </summary>
        public string Requester { get; set; }

        /// <summary>
        /// Whether or not the song in question has been downloaded yet.
        /// </summary>
        public bool IsDownloaded { get; internal set; } = false;

        private YoutubeClient youtubeClient;
        private Video Video;
        
        public RequestedSong()
        {
            youtubeClient = new YoutubeClient();
            Video = null;
            Title = null;
            Artist = null;
            FilePath = null;
            Requester = null;
        }
        public RequestedSong(string videoId, string requester)
        {
            youtubeClient = new YoutubeClient();
            Video = TryGetVideo(videoId);
            Title = Video.Title;
            Artist = Video.Author;
            FilePath = null;
            Requester = requester;
        }

        /// <summary>
        /// Downloads the requested song to the "Downloads" folder, if it hasn't already been downloaded.
        /// </summary>
        /// <returns>Boolean representing if the download was successful.</returns>
        public async Task<bool> DownloadAsync()
        {
            if (IsDownloaded)
                return IsDownloaded;
            if (Video == null)
                return false;

            var fileName = ToSafeFileName(Video.Title);
            var path = APP_DIRECTORY + "downloads\\" + fileName;

            var mediaStreamInfos = await youtubeClient.GetVideoMediaStreamInfosAsync(Video.Id);
            var streamInfo = mediaStreamInfos.Audio.Where(x => x.Container != Container.WebM).First();
            var extension = streamInfo.Container.GetFileExtension();

            if (File.Exists($"{path}.{extension}"))
            {
                FilePath = $"{path}.{extension}";
                IsDownloaded = true;
                return true;
            }

            ConsoleHelper.WriteLine($"Now Downloading: \"{Video.Title}\"");
            await youtubeClient.DownloadMediaStreamAsync(streamInfo, $"{path}.{extension}").ContinueWith(task => { OnDownloadCompletion(); });
            FilePath = $"{path}.{extension}";
            return true;
        }

        private void OnDownloadCompletion()
        {
            ConsoleHelper.WriteLine($"Finished Downloading: \"{Video.Title}\"");
            IsDownloaded = true;
        }

        private Video TryGetVideo(string videoId)
        {
            try
            {
                return youtubeClient.GetVideoAsync(videoId).Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // thanks stack overflow
        private static string ToSafeFileName(string s)
        {
            return s
                .Replace("\\", "")
                .Replace("/", "")
                .Replace("\"", "")
                .Replace("*", "")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|", "");
        }
    }
}
