using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using ATCB.Library.Helpers;
using ATCB.Library.Models.WebApi;
using ATCB.Library.Models.Settings;

namespace ATCB.Library.Models.DiscordApp
{
    public class DiscordChatBot
    {
        private DiscordSocketClient discordClient;
        private string token, accessToken, refreshToken, guildId;
        private SocketGuild guild;
        private ApplicationSettings settings;

        public DiscordChatBot(DiscordDetails details, ApplicationSettings settings)
        {
            token = details.Token;
            accessToken = details.AccessToken;
            refreshToken = details.RefreshToken;
            guildId = details.GuildId;
            this.settings = settings;
            discordClient = new DiscordSocketClient();
            discordClient.Connected += OnConnected;
            discordClient.Disconnected += OnDisconnected;
        }

        public ConnectionState GetConnectionState() => discordClient.ConnectionState;

        public async Task StartAsync()
        {
            await discordClient.LoginAsync(TokenType.Bot, token);
            await discordClient.StartAsync();
        }

        public void SendMessage(string message, ulong channelId = 0, Embed embed = null)
        {
            ulong id = (channelId != 0) ? channelId : settings.Discord.GeneralTextChannel;
            var channel = guild.GetTextChannel(id);
            if (channel != null)
            {
                ConsoleHelper.WriteLine($"[Discord] #{channel.Name} - {discordClient.CurrentUser.Username}: {message}");
                channel.SendMessageAsync(message, embed: embed);
            }
        }

        public List<SocketTextChannel> GetChannels()
        {
            return guild.TextChannels.ToList();
        }

        public SocketTextChannel GetChannel(ulong id)
        {
            return guild.GetTextChannel(id);
        }

        private Task OnConnected()
        {
            ConsoleHelper.WriteLine("Discord bot connected!");
            guild = discordClient.GetGuild(ulong.Parse(guildId));
            return Task.CompletedTask;
        }

        private Task OnDisconnected(Exception arg)
        {
            ConsoleHelper.WriteLine("Discord bot disconnected.");
            return Task.CompletedTask;
        }
    }
}
