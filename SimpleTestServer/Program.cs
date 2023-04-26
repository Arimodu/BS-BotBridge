using BSBBCore.Network.Servers;
using System;

namespace SimpleTestServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServerWrapper serverWrapper = new ServerWrapper((log) => Console.WriteLine(log), 3);

            serverWrapper.Start();

            while (Console.ReadLine() != "stop") { }

            serverWrapper.Stop();

            Console.ReadLine();
        }
    }
}
