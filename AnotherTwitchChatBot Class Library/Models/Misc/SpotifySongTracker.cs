using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ATCB.Library.Models.Misc
{
    public class SpotifySongTracker
    {
        private Timer _timer;
        private string _currentSong;

        public SpotifySongTracker()
        {
            _currentSong = "";
            _timer = new Timer(GetSpotifyTrackInfo, _currentSong, 0, 500);
        }

        public delegate void SongUpdate(string data);

        public SongUpdate OnSongUpdate { get; set; }

        private void GetSpotifyTrackInfo(object state)
        {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            if (proc != null && !string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
            {
                var currentSong = proc.MainWindowTitle;
                if (currentSong != _currentSong)
                {
                    _currentSong = currentSong;
                    OnSongUpdate(currentSong);
                }
            }
        }
    }
}
