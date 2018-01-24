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

        public ChatMessageContext ChatMessage;

        public CommandContext()
        {
            Client = null;
            Context = null;
            ChatMessage = null;
        }
        public CommandContext(TwitchClient client, ChatCommand context)
        {
            Client = client;
            Context = context;
            ChatMessage = new ChatMessageContext();
            ChatMessage.DisplayName = context.ChatMessage.DisplayName;
            ChatMessage.IsBroadcaster = context.ChatMessage.IsBroadcaster;
            ChatMessage.IsModerator = context.ChatMessage.IsModerator;
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
            Client.SendMessage(message);
        }

        public class ChatMessageContext
        {
            public string DisplayName { get; set; }
            public bool IsModerator { get; set; }
            public bool IsBroadcaster { get; set; }
        }
    }
}
