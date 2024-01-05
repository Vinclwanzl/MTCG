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
            MTCGServer server = new MTCGServer();
            server.StartServer("127.0.0.1", 10001, "127.0.0.1", 5432);
            Console.WriteLine("Press any key to stop the Server...");
            Console.ReadKey();
        }
    }
}