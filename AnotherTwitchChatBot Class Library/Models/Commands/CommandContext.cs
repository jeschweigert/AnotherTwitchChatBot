using ATCB.Library.Helpers;
using ATCB.Library.Models.DiscordApp;
using ATCB.Library.Models.Settings;
using ATCB.Library.Models.Twitch;
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace ATCB.Library.Models.Commands
{
    public class CommandContext
    {
        private TwitchClient BotClient, UserClient;
        internal DiscordChatBot DiscordClient;
        private TwitchAPI TwitchApi;
        private ChatCommand Context;
        private bool FromConsole;

        public ChatMessageContext ChatMessage;
        public TwitchStreamContext TwitchStream;
        public DiscordContext Discord;
        public ApplicationSettings Settings;
        public List<string> Commands { get; private set; }

        public CommandContext(TwitchClient botClient, TwitchClient userClient, TwitchAPI twitchApi, ChatCommand context, UserType type, CommandFactory factory, ApplicationSettings settings, DiscordChatBot discord, bool fromConsole = false)
        {
            BotClient = botClient;
            UserClient = userClient;
            TwitchApi = twitchApi;
            Context = context;
            Settings = settings;
            DiscordClient = discord;
            FromConsole = fromConsole;
            
            // Provide information to the ChatMessageContext
            ChatMessage = new ChatMessageContext();
            ChatMessage.UserType = type;
            if (context != null)
            {
                ChatMessage.Bits = context.ChatMessage.Bits;
                ChatMessage.IsChatBot = fromConsole;
                ChatMessage.DisplayName = context.ChatMessage.DisplayName;
                ChatMessage.Username = context.ChatMessage.Username;
                ChatMessage.IsBroadcaster = context.ChatMessage.IsBroadcaster;
                ChatMessage.IsModerator = context.ChatMessage.IsModerator;
                ChatMessage.IsModeratorOrBroadcaster = context.ChatMessage.IsBroadcaster || context.ChatMessage.IsModerator;
                ChatMessage.IsSubscriber = context.ChatMessage.IsSubscriber;
                ChatMessage.Message = context.ChatMessage.Message;
            }

            Commands = factory.ToList(type);

            // Provide information to the TwitchStreamContext
            TwitchStream = new TwitchStreamContext();
            var channel = TwitchApi.Channels.v3.GetChannelByNameAsync(UserClient.TwitchUsername).ConfigureAwait(false).GetAwaiter().GetResult();
            TwitchStream.Game = channel.Game;
            TwitchStream.Title = channel.Status;
            TwitchStream.Username = UserClient.TwitchUsername;

            // Provide information to the DiscordContext
            Discord = new DiscordContext();
            Discord.State = discord.GetConnectionState();
        }

        public List<string> ArgumentsAsList => Context.ArgumentsAsList;
        public string ArgumentsAsString => Context.ArgumentsAsString;
        public string CommandText => Context.CommandText;

        /// <summary>
        /// Sends a message to chat through the chat bot's account.
        /// </summary>
        /// <param name="message">The chat message to be sent.</param>
        public void SendMessage(string message)
        {
            if (FromConsole)
                ConsoleHelper.WriteLine(message);
            else
                BotClient.SendMessage(message);
        }

        public void ConnectDiscord()
        {
            DiscordClient.StartAsync();
        }

        public class ChatMessageContext
        {
            public int Bits { get; set; }
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public bool IsBroadcaster { get; set; }
            public bool IsChatBot { get; set; }
            public bool IsModerator { get; set; }
            public bool IsModeratorOrBroadcaster { get; set; }
            public bool IsSubscriber { get; set; }
            public string Message { get; set; }
            public UserType UserType { get; set; }
        }

        public class TwitchStreamContext
        {
            public string Game { get; set; }
            public string Title { get; set; }
            public string Username { get; set; }
        }

        public class DiscordContext
        {
            public ConnectionState State { get; set; }
        }
    }
}
