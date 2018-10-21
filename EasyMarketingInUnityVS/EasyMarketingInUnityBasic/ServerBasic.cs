using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace EasyMarketingInUnityBasic
{
    public class ServerBasic
    {
        public static Process process;

        public static string logFile = "server.log";

        public static bool StartServer() {
            string dir = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
            string file = "Start.bat";

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "\"" + dir + file + "\"";
            startInfo.Arguments = 3000 + " ";

            process = System.Diagnostics.Process.Start(startInfo);
            return true;
        }

        public static bool EndServer() { 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/cmd/Shutdown");
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {

                        ServerBasic.Log(reader.ReadToEnd());
                    }
                }
            }
            if (process.Responding) {
                //action = "Closing";
                ServerBasic.Log("Process is Responding");

                //Thread.Sleep(500); // Wait for Potential Close
            }
            ServerBasic.Log("Closing Main Window");
            process.CloseMainWindow();
            if (!process.HasExited) {
                ServerBasic.Log("Killing Process");
                process.Kill();
                //Thread.Sleep(500); // Wait for Potential Close
                process.WaitForExit();
            } else {
                ServerBasic.Log("Process has not exited");
            }

            int ExitCode = process.ExitCode;
            ServerBasic.Log("Exit Code: " + ExitCode);
            return ExitCode == 0;
        }

        public static string SendRequest() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/cmd/Twitter/Get");
            request.Method = "GET";

            string result = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        result = reader.ReadToEnd();
                        
                    }
                }
            }
            return result;
        }

        public static bool Log(string message = "", string type = "INFO") {
            try {
                if (message == "") { File.AppendAllText(logFile, "\n"); return true; }

                string time = DateTime.Now.ToString("h:mm:ss tt");
                string log = type + " [" + time + "] : " + message + "\n";
                File.AppendAllText(logFile, log);
            } catch (IOException e) {
                return false;
            } catch (NotSupportedException e) {
                return false;
            } catch (System.Security.SecurityException e) {
                return false;
            }
            return true;
        }

    }
}
