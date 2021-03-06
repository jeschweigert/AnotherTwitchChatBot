﻿using ATCB.Library.Helpers;
using ATCB.Library.Models.Misc;
using ATCB.Library.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using YoutubeExplode;
using YoutubeSearch;

namespace ATCB.Library.Models.Commands.Music
{
    public class SongRequestCommand : Command
    {
        private YoutubeClient client;

        public SongRequestCommand()
        {
            client = new YoutubeClient();
        }

        public override string[] Synonyms() { return new string[] { "songrequest", "sr", "songreq" }; }

        public override void Run(CommandContext context)
        {
            if (!context.Settings.SongRequests)
            {
                context.SendMessage("Requests aren't being taken at the moment, sorry!");
                return;
            }

            if (context.ArgumentsAsList.Count < 1)
            {
                context.SendMessage("You didn't give me anything to add.");
            }
            else
            {
                var success = YoutubeClient.TryParseVideoId(context.ArgumentsAsString, out string videoId);

                if (!success)
                {
                    if (context.ArgumentsAsString.Contains("soundcloud.com"))
                    {
                        var song = SCExtractor.Extract(context.ArgumentsAsString);
                        if (song != null)
                        {
                            context.SendMessage($"@{context.ChatMessage.DisplayName} Your request, \"{song.title}\", is #{GlobalVariables.GlobalPlaylist.RequestedSongCount + 1} in the queue!");
                            GlobalVariables.GlobalPlaylist.Enqueue(new RequestedSong(song.title, song.user.username, context.ChatMessage.DisplayName, $"{AppDomain.CurrentDomain.BaseDirectory}downloads\\{song.title}.{song.original_format}"));
                        }
                        else
                        {
                            context.SendMessage($"@{context.ChatMessage.DisplayName} Uh oh! I couldn't grab that Soundcloud song.");
                        }
                    }
                    else
                    {
                        // Request by YouTube query
                        using (WebClient client = new WebClient())
                        {
                            videoId = client.DownloadString($"https://beta.decapi.me/youtube/videoid?search={context.ArgumentsAsString}");
                            MakeRequest(videoId, context);
                        }
                    }
                }
                else
                {
                    // Request by YouTube URL
                    MakeRequest(videoId, context);
                }
            }
        }

        private void MakeRequest(string videoId, CommandContext context)
        {
            var videoTitle = Task.Run(() => this.client.GetVideoAsync(videoId)).Result.Title;
            context.SendMessage($"@{context.ChatMessage.DisplayName} Your request, \"{videoTitle}\", is #{GlobalVariables.GlobalPlaylist.RequestedSongCount + 1} in the queue!");
            GlobalVariables.GlobalPlaylist.Enqueue(new RequestedSong(videoId, context.ChatMessage.DisplayName));
        }
    }
}
