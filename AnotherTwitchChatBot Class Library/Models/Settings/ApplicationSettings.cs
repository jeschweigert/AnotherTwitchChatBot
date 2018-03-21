using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Settings
{
    public class ApplicationSettings
    {
        private string Location = $"{AppDomain.CurrentDomain.BaseDirectory}settings.json";

        public ApplicationSettings()
        {
            AppState = new Guid();
            PlaylistLocation = null;
        }

        public Guid AppState { get; set; }

        public string PlaylistLocation { get; set; }

        public bool SongRequests { get; internal set; }
        public bool SoundEffects { get; internal set; }

        public ApplicationSettings Load()
        {
            return JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(Location));
        }

        public void Save()
        {
            var serializedJson = JsonConvert.SerializeObject(this);
            File.WriteAllText(Location, serializedJson);
        }

        public bool Exists()
        {
            return File.Exists(Location);
        }
    }
}
