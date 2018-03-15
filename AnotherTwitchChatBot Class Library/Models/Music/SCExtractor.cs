using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Music
{
    public static class SCExtractor
    {
        public static SoundcloudTrack Extract(string url)
        {
            SoundcloudTrack fromJson;
            SoundcloudStreams streams;
            string filename = "";
            using (WebClient client = new WebClient())
            {
                fromJson = JsonConvert.DeserializeObject<SoundcloudTrack>(client.DownloadString($"http://api.soundcloud.com/resolve.json?url={url}&client_id=589e0ff400a0bc83ecd8eb20b94b57de"));
                streams = JsonConvert.DeserializeObject<SoundcloudStreams>(client.DownloadString($"https://api.soundcloud.com/tracks/{fromJson.id}/streams?client_id=589e0ff400a0bc83ecd8eb20b94b57de"));
                filename = $"{fromJson.title}.{fromJson.original_format}";
                client.DownloadFile(streams.http_mp3_128_url.Replace("\\u0026", "&"), $"{AppDomain.CurrentDomain.BaseDirectory}downloads\\{filename}");
            }
            return fromJson;
        }
    }

    public class SoundcloudStreams
    {
        public string http_mp3_128_url { get; set; }
        public string hls_mp3_128_url { get; set; }
        public string hls_opus_64_url { get; set; }
        public string preview_mp3_128_url { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string kind { get; set; }
        public string permalink { get; set; }
        public string username { get; set; }
        public string last_modified { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string avatar_url { get; set; }
    }

    public class SoundcloudTrack
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int duration { get; set; }
        public bool commentable { get; set; }
        public string state { get; set; }
        public int original_content_size { get; set; }
        public string last_modified { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public bool streamable { get; set; }
        public string embeddable_by { get; set; }
        public object purchase_url { get; set; }
        public object purchase_title { get; set; }
        public object label_id { get; set; }
        public string genre { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public object label_name { get; set; }
        public object release { get; set; }
        public object track_type { get; set; }
        public object key_signature { get; set; }
        public object isrc { get; set; }
        public object video_url { get; set; }
        public object bpm { get; set; }
        public object release_year { get; set; }
        public object release_month { get; set; }
        public object release_day { get; set; }
        public string original_format { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public User user { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public string stream_url { get; set; }
        public string download_url { get; set; }
        public int playback_count { get; set; }
        public int download_count { get; set; }
        public int favoritings_count { get; set; }
        public int reposts_count { get; set; }
        public int comment_count { get; set; }
        public bool downloadable { get; set; }
        public string waveform_url { get; set; }
        public string attachments_uri { get; set; }
    }
}
