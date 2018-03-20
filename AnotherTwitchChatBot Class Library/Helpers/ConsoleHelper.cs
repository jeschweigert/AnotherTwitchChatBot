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

        private static string Consolify(string text)
        {
            return Timestamp(HiraganaToRomaji(text));
        }

        private static void OutputLineAndReplaceConsoleText(string s, Color c)
        {
            for (int i = 0; i < charBuffer.Count; i++)
            {
                Colorful.Console.Write("\b \b");
            }
            Colorful.Console.WriteLine(s, c);
            Colorful.Console.Write(new string(charBuffer.ToArray()), Color.DarkGray);
        }

        private static void OutputLineAndReplaceConsoleTextStyled(string s, StyleSheet st)
        {
            for (int i = 0; i < charBuffer.Count; i++)
            {
                Colorful.Console.Write("\b \b");
            }
            Colorful.Console.WriteLineStyled(s, st);
            Colorful.Console.Write(new string(charBuffer.ToArray()), Color.DarkGray);
        }

        private static string Timestamp(string text)
        {
            return $"[{DateTime.Now.ToString("T")}] {text}";
        }

        public static void WriteLine(string message)
        {
            WriteLine(message, Color.DarkGray);
        }
        public static void WriteLine(string message, bool deleteInput)
        {
            Colorful.Console.WriteLine(Consolify(message), Color.DarkGray);
        }
        public static void WriteLine(string message, Color color)
        {
            OutputLineAndReplaceConsoleText(Consolify(message), color);
        }

        public static void WriteLineChat(string message, bool deleteInput = false)
        {
            if (deleteInput)
            {
                for (int i = 0; i < charBuffer.Count; i++)
                {
                    Colorful.Console.Write("\b \b");
                }

                Colorful.Console.WriteLine(Consolify(message), Color.White);
            }
            else
                OutputLineAndReplaceConsoleText(Consolify(message), Color.White);
        }

        public static void WriteLineWhisper(string message, bool deleteInput = false)
        {
            if (deleteInput)
            {
                for (int i = 0; i < charBuffer.Count; i++)
                {
                    Colorful.Console.Write("\b \b");
                }

                Colorful.Console.WriteLine(Consolify(message), Color.DarkGreen);
            }
            else
                OutputLineAndReplaceConsoleText(Consolify(message), Color.DarkGreen);
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
                        WriteLineChat($"Console: !{sentMessage}", true);

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

        private static string HiraganaToRomaji(string text)
        {
            string toReturn = "";
            foreach (char c in text)
            {
                if (c == 'わ')
                    toReturn += "wa";
                else if (c == 'ら')
                    toReturn += "ra";
                else if (c == 'や')
                    toReturn += "ya";
                else if (c == 'ま')
                    toReturn += "ma";
                else if (c == 'は')
                    toReturn += "ha";
                else if (c == 'な')
                    toReturn += "na";
                else if (c == 'た')
                    toReturn += "ta";
                else if (c == 'さ')
                    toReturn += "sa";
                else if (c == 'か')
                    toReturn += "ka";
                else if (c == 'あ')
                    toReturn += "a";
                else if (c == 'ゐ')
                    toReturn += "wi";
                else if (c == 'り')
                    toReturn += "ri";
                else if (c == 'み')
                    toReturn += "mi";
                else if (c == 'ひ')
                    toReturn += "hi";
                else if (c == 'に')
                    toReturn += "ni";
                else if (c == 'ち')
                    toReturn += "chi";
                else if (c == 'し')
                    toReturn += "shi";
                else if (c == 'き')
                    toReturn += "ki";
                else if (c == 'い')
                    toReturn += "i";
                else if (c == 'る')
                    toReturn += "ru";
                else if (c == 'ゆ')
                    toReturn += "yu";
                else if (c == 'む')
                    toReturn += "mu";
                else if (c == 'ふ')
                    toReturn += "fu";
                else if (c == 'ぬ')
                    toReturn += "nu";
                else if (c == 'つ')
                    toReturn += "tsu";
                else if (c == 'す')
                    toReturn += "su";
                else if (c == 'く')
                    toReturn += "ku";
                else if (c == 'う')
                    toReturn += "u"; // todo: add the rest lol
                else
                    toReturn += c;
            }
            return toReturn;
        }
    }
}
