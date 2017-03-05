using System;

namespace PrivateTunnelKeygen
{
    static class ConsoleHelper
    {
        public const ConsoleColor DefaultColor = ConsoleColor.Gray;

        public static void WriteLine(string message, ConsoleColor color = DefaultColor)
        {
            Write(message + Environment.NewLine, color);
        }

        public static void Write(string message, ConsoleColor color = DefaultColor)
        {
            var oldColor = Console.ForegroundColor;
            bool changeColor = color != oldColor;

            if (changeColor)
                Console.ForegroundColor = color;

            Console.Write(message);

            if (changeColor)
                Console.ForegroundColor = oldColor;
        }
    }
}
