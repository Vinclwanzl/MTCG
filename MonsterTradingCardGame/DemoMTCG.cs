using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class DemoMTCG
    {
        static void Main()
        {
            // Create and start the server
            MTCGServer server = new MTCGServer();
            server.StartServerAsync("127.0.0.1", 10001);

            Console.WriteLine("Server running. Press any key to stop...");
            Console.ReadKey();

            // Stop the server

        }
    }
}