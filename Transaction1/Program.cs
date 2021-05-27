using System;
using Shared;

namespace Transaction1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Transactions.RemoveAdmin(1, 1, 5, "TX1");
            Logger.Write("TX1 done");
            Console.ReadKey();
        }
    }
}