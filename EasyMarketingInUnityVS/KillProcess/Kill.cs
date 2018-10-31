using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace KillProcess {
    class Kill {
        static void Main(string[] args) {
            int id = 9208;

            foreach(var p in Process.GetProcesses()) {
                if(p.Id == id) {
                    p.Kill();
                    p.WaitForExit();
                    break;
                }
            }

            bool success = true;
            foreach (var p in Process.GetProcesses()) {
                if (p.Id == id) {
                    success = false;
                    Console.WriteLine("Failed");
                    break;
                }
            }

            if (success) {
                Console.WriteLine("Success");
            }

        }
    }
}
