using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using EasyMarketingInUnity;

namespace EasyMarketingInUnityConsole {
    class Program {
        static void Main(string[] args) {
            if (Server.StartServer(true)) {
                Console.WriteLine(Server.Instance.SendRequest("Twitter", Method.Authenticate));

                Console.ReadLine();

                //Console.WriteLine(Server.Instance.SendRequest("Twitter", Method.Get));

                //Console.Write("Tweet: ");
                //string message = Console.ReadLine();

                //Console.WriteLine(Server.Instance.SendRequest("Twitter", Method.Post, "status=" + message));
            }
            Server.EndServer();
        }

        // Process Window Style Show creates a separate process
        // Process Window Style Hidden combines the process

        // If It is hidden, then ShellExecute must be false, and it is combined
    }
}
