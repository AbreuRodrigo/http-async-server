using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    public static class ConsoleHelper
    {
        private static ConsoleColor lastColor;

        public static void Write(string text)
        {
            Console.Write(text);
        }

        public static void Write(string text, ConsoleColor color)
        {
            ChangeForegroundColor(color);
            Console.Write(text);
            ResetForegroundColor();
        }

        public static void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            ChangeForegroundColor(color);
            Console.WriteLine(text);
            ResetForegroundColor();
        }

        private static void ChangeForegroundColor(ConsoleColor color)
        {
            lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        private static void ResetForegroundColor()
        {
            Console.ForegroundColor = lastColor;
        }
    }
}
