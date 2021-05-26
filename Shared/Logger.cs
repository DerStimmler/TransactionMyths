using System;

namespace Shared
{
    public static class Logger
    {
        public static void Write(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff tt}> {message}");
        }
    }
}