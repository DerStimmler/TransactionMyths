using System;
using Shared;

namespace Transaction3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Transactions.RemoveAdmin(1, 1, 6, "TX3", 1);
            Logger.Write("TX3 done");
            Console.ReadKey();
        }
    }
}