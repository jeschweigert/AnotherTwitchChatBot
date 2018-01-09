using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Settings
{
    public class ApplicationSettings
    {
        public static Guid AppState { get; set; }

        public static string PlaylistLocation { get; set; }
    }
}
