using ATCB.Library.Helpers;
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
        private TwitchClient Client;
        private ChatCommand Context;
        private bool FromConsole;

        public ChatMessageContext ChatMessage;

        public CommandContext()
        {
            Client = null;
            Context = null;
            ChatMessage = null;
            FromConsole = false;
        }
        public CommandContext(TwitchClient client, ChatCommand context, bool fromConsole = false)
        {
            Client = client;
            Context = context;
            ChatMessage = new ChatMessageContext();
            ChatMessage.Bits = context.ChatMessage.Bits;
            ChatMessage.DisplayName = context.ChatMessage.DisplayName;
            ChatMessage.IsBroadcaster = context.ChatMessage.IsBroadcaster;
            ChatMessage.IsModerator = context.ChatMessage.IsModerator;
            ChatMessage.IsModeratorOrBroadcaster = context.ChatMessage.IsBroadcaster || context.ChatMessage.IsModerator;
            FromConsole = fromConsole;
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
                Client.SendMessage(message);
        }

        public class ChatMessageContext
        {
            public int Bits { get; set; }
            public string DisplayName { get; set; }
            public bool IsBroadcaster { get; set; }
            public bool IsModerator { get; set; }
            public bool IsModeratorOrBroadcaster { get; set; }
        }
    }
}
