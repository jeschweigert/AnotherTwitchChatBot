using ATCB.Library.Helpers;
using SpotifyAPI.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Misc
{
    public class SpotifySongTracker
    {
        private SpotifyLocalAPI _spotify;

        public SpotifySongTracker()
        {
            _spotify = new SpotifyLocalAPI();
            _spotify.OnTrackChange += OnSpotifyTrackChange;
        }

        public bool Connect()
        {
            if (!SpotifyLocalAPI.IsSpotifyRunning())
                return false;
            if (_spotify.Connect())
            {
                _spotify.ListenForEvents = true;
                return true;
            }
            return false;
        }

        public delegate void SongUpdate(string title, string artist);

        public SongUpdate OnSongUpdate { get; set; }

        private void OnSpotifyTrackChange(object sender, TrackChangeEventArgs e)
        {
            ConsoleHelper.WriteLine($"Now Playing: {e.NewTrack.ArtistResource.Name} - {e.NewTrack.TrackResource.Name}");
            OnSongUpdate(e.NewTrack.TrackResource.Name, e.NewTrack.ArtistResource.Name);
        }
    }
}
