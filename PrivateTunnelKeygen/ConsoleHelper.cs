using System;
using System.Collections.Generic;
using System.Linq;

namespace PrivateTunnelKeygen
{
    static class ConsoleHelper
    {
        private static readonly string[] ColorsNames = Enum.GetNames(typeof(ConsoleColor));

        private static readonly object SyncRoot = new object();

        public static void WriteLine(string message)
        {
            var colorsStack = new Stack<ConsoleColor>();

            lock (SyncRoot)
            {
                for (int i = 0; i < message.Length; ++i)
                {
                    if (message[i] == '%')
                    {
                        string colorName =
                            ColorsNames.FirstOrDefault(
                                name => string.Compare(
                                            message, i + 1,
                                            name, 0,
                                            name.Length,
                                            StringComparison.OrdinalIgnoreCase) == 0);

                        if (colorName == null)
                            Console.ForegroundColor = colorsStack.Pop();
                        else
                        {
                            i += colorName.Length;
                            colorsStack.Push(Console.ForegroundColor);
                            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colorName);
                        }
                    }
                    else
                    {
                        Console.Write(message[i]);
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
