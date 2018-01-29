using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace ATCB.Library.Models.Commands.Music
{
    public class PlaylistCommand : Command
    {
        public override bool IsSynonym(string commandText) => commandText == "playlist";

        public override void Run(CommandContext context)
        {
            if (context.ChatMessage.IsBroadcaster || context.ChatMessage.IsChatBot)
            {
                if (context.ArgumentsAsList.Count > 0)
                {
                    var commandProperty = context.ArgumentsAsList[0].ToLower();
                    context.ArgumentsAsList.RemoveAt(0);

                    switch (commandProperty)
                    {
                        case "load":
                            Load(context.ArgumentsAsList);
                            break;
                        case "shuffle":
                            GlobalVariables.GlobalPlaylist.Shuffle();
                            break;
                        case "togglereq":
                            GlobalVariables.GlobalPlaylist.ToggleRequests();
                            break;
                        case "import":
                            ImportAsync(context.ArgumentsAsList);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    context.SendMessage("hey sorry I will add something here later");
                }
            }
            else
            {
                context.SendMessage("This command can only be used by the broadcaster.");
            }
        }

        private void Load(List<String> arguments)
        {
            if (arguments.Count > 0)
            {
                GlobalVariables.GlobalPlaylist.LoadFromFolder(String.Join(" ", arguments.ToArray()));
                GlobalVariables.GlobalPlaylist.Shuffle();
                GlobalVariables.GlobalPlaylist.Reset();
            }
        }

        private async Task ImportAsync(List<String> arguments)
        {
            if (arguments.Count > 0)
            {
                var isPlaylist = YoutubeClient.TryParsePlaylistId(String.Join(" ", arguments.ToArray()), out string playlistId);

                if (isPlaylist)
                {
                    var playlistDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}playlists\\{playlistId}";
                    var client = new YoutubeClient();
                    Directory.CreateDirectory(playlistDirectory);

                    var playlist = await client.GetPlaylistAsync(playlistId).ConfigureAwait(false);
                    var downloadStart = DateTime.Now;
                    ConsoleHelper.WriteLine($"Now Downloading: Playlist \"{playlist.Title}\"");
                    foreach (var video in playlist.Videos)
                    {
                        var path = $"{playlistDirectory}\\{video.Id}";

                        var mediaStreamInfos = await client.GetVideoMediaStreamInfosAsync(video.Id);
                        var streamInfo = mediaStreamInfos.Audio.Where(x => x.Container != Container.WebM).First();
                        var extension = streamInfo.Container.GetFileExtension();

                        if (File.Exists($"{path}.{extension}"))
                        {
                            ConsoleHelper.WriteLine($"Video \"{video.Title}\" already downloaded, skipping.");
                            return;
                        }

                        ConsoleHelper.WriteLine($"Now Downloading: \"{video.Title}\"");
                        await client.DownloadMediaStreamAsync(streamInfo, $"{path}.{extension}").ContinueWith(task => { ConsoleHelper.WriteLine($"Download Finished: \"{video.Title}\""); });
                        using (TagLib.File file = TagLib.File.Create($"{path}.{extension}"))
                        {
                            file.Tag.Performers = new string[] { video.Author };
                            file.Tag.Title = video.Title;
                            file.Save();
                        }
                    }

                    var downloadDuration = DateTime.Now - downloadStart;
                    ConsoleHelper.WriteLine($"Download Finished: Playlist \"{playlist.Title}\" ({downloadDuration.Hours}H {downloadDuration.Minutes}M {downloadDuration.Seconds}S)");
                    GlobalVariables.GlobalPlaylist.LoadFromFolder(playlistDirectory);
                    GlobalVariables.GlobalPlaylist.Shuffle();
                    GlobalVariables.GlobalPlaylist.Reset();
                }
            }
        }
    }
}
