using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using ATCB.Library.Models.Twitch;

namespace ATCB.Library.Models.Misc
{
    public class ChatTimer
    {
        private readonly string directory = $"{AppDomain.CurrentDomain.BaseDirectory}chattimers";
        private List<string> messages;
        private int index = 0;
        private Timer timer;
        private TwitchChatBot bot;

        public ChatTimer(TwitchChatBot chatBot, int interval)
        {
            bot = chatBot;
            messages = new List<string>();
            foreach (var item in Directory.GetFiles(directory).Where(x => x.Split('.').Last() == "txt"))
            {
                messages.Add(File.ReadAllText(item));
            }
            if (messages.Count() > 1)
                timer = new Timer(new TimerCallback(OnElapsed), null, 0, interval);
        }
        public int Count() => messages.Count();

        private void OnElapsed(object state)
        {
            if (bot.IsLive)
            {
                bot.SendMessage(messages[index]);

                if (index + 1 > messages.Count - 1)
                    index = 0;
                else
                    index++;
            }
        }

    }
}
