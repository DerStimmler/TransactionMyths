using System;
using Shared;

namespace Transaction2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Transactions.RemoveAdmin(2,1, 5, "TX2");
            Logger.Write("TX2 done");
            Console.ReadKey();
        }
    }
}