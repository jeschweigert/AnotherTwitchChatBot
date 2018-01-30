using ATCB.Library.Models.Music;
using ATCB.Library.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Misc
{
    public static class GlobalVariables
    {
        public static Playlist GlobalPlaylist = new Playlist();

        public static ApplicationSettings AppSettings = new ApplicationSettings();
    }
}
