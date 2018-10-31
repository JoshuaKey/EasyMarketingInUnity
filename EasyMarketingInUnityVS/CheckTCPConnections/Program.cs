using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace CheckTCPConnections {
    class Program {
        static void Main(string[] args) {
            int port = 3000;
            int id = 0;

            //IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            //IPEndPoint[] endPoints = ipProperties.GetActiveTcpListeners();
            //TcpConnectionInformation[] tcpConnections =
            //    ipProperties.GetActiveTcpConnections();

            //foreach (TcpConnectionInformation info in tcpConnections) {
            //    if(info.LocalEndPoint.Port == port) {
            //        id = inf o.
            //    }
            //    Console.WriteLine("Local: {0}:{1}\nRemote: {2}:{3}\nState: {4}\n",
            //        info.LocalEndPoint.Address, info.LocalEndPoint.Port,
            //        info.RemoteEndPoint.Address, info.RemoteEndPoint.Port,
            //        info.State.ToString());
            //}

            //Console.WriteLine("Process ID: " + id);
            //Console.ReadLine();

            foreach (ProcessPort p in ProcessPorts.ProcessPortMap.FindAll(x => x.PortNumber == port))  //extension is not needed. 
                {
                Console.WriteLine(p.ProcessName + " " + p.ProcessId);
                Console.WriteLine(p.ProcessPortDescription);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
