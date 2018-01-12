using Colorful;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace ATCB.Library.Helpers
{
    public static class ConsoleHelper
    {
        private static List<char> charBuffer = new List<char>();
        private static object locker = new object();

        public static EventHandler OnConsoleCommand;

        private static void OutputLineAndReplaceConsoleText(string s, Color c)
        {
            for (int i = 0; i < charBuffer.Count; i++)
            {
                Colorful.Console.Write("\b \b");
            }
            Colorful.Console.WriteLine(s, c);
            Colorful.Console.Write(new string(charBuffer.ToArray()), Color.LightGray);
        }

        private static void OutputLineAndReplaceConsoleTextStyled(string s, StyleSheet st)
        {
            for (int i = 0; i < charBuffer.Count; i++)
            {
                Colorful.Console.Write("\b \b");
            }
            Colorful.Console.WriteLineStyled(s, st);
            Colorful.Console.Write(new string(charBuffer.ToArray()), Color.LightGray);
        }

        public static void WriteLine(string message)
        {
            WriteLine(message, Color.LightGray);
        }
        public static void WriteLine(string message, bool deleteInput)
        {
            Colorful.Console.WriteLine(message, Color.LightGray);
        }
        public static void WriteLine(string message, Color color)
        {
            OutputLineAndReplaceConsoleText(message, color);
        }

        public static void WriteLineChat(string message, bool deleteInput = false)
        {
            if (deleteInput)
            {
                for (int i = 0; i < charBuffer.Count; i++)
                {
                    Colorful.Console.Write("\b \b");
                }

                Colorful.Console.WriteLine(message, Color.White);
            }
            else
                OutputLineAndReplaceConsoleText(message, Color.White);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            var key = System.Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                if (charBuffer.Count > 0)
                {
                    lock (locker)
                    {
                        var sentMessage = new string(charBuffer.ToArray());
                        WriteLineChat($"[{DateTime.Now.ToString("T")}] Console: !{sentMessage}", true);

                        charBuffer.Clear();
                        Task.Run(() => { OnConsoleCommand(null, new ConsoleCommandEventArgs(sentMessage)); });
                    }
                }
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && charBuffer.Count > 0)
                {
                    charBuffer.RemoveAt(charBuffer.Count - 1);
                    System.Console.Write("\b \b");
                }
                else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || char.IsSymbol(key.KeyChar) || char.IsPunctuation(key.KeyChar))
                {
                    charBuffer.Add(key.KeyChar);
                    System.Console.Write(key.KeyChar);
                }
            }
            return key;
        }
    }
}
