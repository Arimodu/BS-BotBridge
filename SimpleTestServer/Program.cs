using BSBBLib;
using System;

namespace SimpleTestServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server((log) => Console.WriteLine(log));

            while (Console.ReadLine() != "stop") { }

            server.Stop();

            Console.ReadLine();
        }
    }
}
