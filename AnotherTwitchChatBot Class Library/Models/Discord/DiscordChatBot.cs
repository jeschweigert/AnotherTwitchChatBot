using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using ATCB.Library.Helpers;
using ATCB.Library.Models.WebApi;

namespace ATCB.Library.Models.Discord
{
    public class DiscordChatBot
    {
        private DiscordSocketClient discordClient;
        private string token, accessToken, refreshToken, guildId;
        private SocketGuild guild;

        public DiscordChatBot(DiscordDetails details)
        {
            token = details.Token;
            accessToken = details.AccessToken;
            refreshToken = details.RefreshToken;
            guildId = details.GuildId;
            discordClient = new DiscordSocketClient();
            discordClient.Connected += OnConnected;
        }

        public ConnectionState GetConnectionState() => discordClient.ConnectionState;

        public async Task StartAsync()
        {
            await discordClient.LoginAsync(TokenType.Bot, token);
            await discordClient.StartAsync();
        }

        public async Task SendMessageAsync(SocketChannel channel, string message)
        {
            throw new NotImplementedException();
        }

        private Task OnConnected()
        {
            ConsoleHelper.WriteLine("Discord bot connected!");
            guild = discordClient.GetGuild(ulong.Parse(guildId));
            return Task.CompletedTask;
        }
    }
}
