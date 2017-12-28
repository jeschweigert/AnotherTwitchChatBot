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

        public static void WriteLineStyled(string message, StyleSheet styleSheet, bool deleteInput)
        {
            Colorful.Console.WriteLineStyled(message, styleSheet);
        }
        public static void WriteLineStyled(string message, StyleSheet styleSheet)
        {
            OutputLineAndReplaceConsoleTextStyled(message, styleSheet);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            var key = System.Console.ReadKey();
            if (key.Key == ConsoleKey.Enter && charBuffer.Count > 0)
            {
                lock (locker)
                {
                    var sentMessage = new string(charBuffer.ToArray());
                    StyleSheet styleSheet = new StyleSheet(Color.White);
                    styleSheet.AddStyle("Console", Color.Gray);
                    WriteLineStyled($"[{DateTime.Now.ToString("T")}] Console: !{sentMessage}", styleSheet, true);

                    charBuffer.Clear();
                    OnConsoleCommand(null, new ConsoleCommandEventArgs(sentMessage));
                }
            }
            else if (key.Key == ConsoleKey.Backspace && charBuffer.Count > 0)
            {
                charBuffer.RemoveAt(charBuffer.Count - 1);
                Colorful.Console.Write(" \b");
            }
            else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || char.IsSymbol(key.KeyChar) || char.IsPunctuation(key.KeyChar))
            {
                charBuffer.Add(key.KeyChar);
            }
            return key;
        }
    }
}
