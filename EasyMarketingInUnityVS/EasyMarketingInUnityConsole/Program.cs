using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using EasyMarketingInUnity;
using System.Threading;
using System.IO;
using System.Net;

namespace EasyMarketingInUnityConsole {
    class Program {

        static public void Prompt(string prompt, string[] cmd) {
            Console.WriteLine(prompt);
            for (int i = 0; i < cmd.Length; i++) {
                Console.WriteLine((i + 1) + ".) " + cmd[i]);
            }
            Console.WriteLine();
        }
        static public int GetSelection(int min, int max) {
            int selection = -1;
            bool error = true;

            while (error) {
                while (!int.TryParse(Console.ReadLine(), out selection)) { }
                error = (selection < min || selection > max);
            }

            return selection;
        }
        private static HttpWebResponse GetResponse(HttpWebRequest request) {
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException e) {
                Console.WriteLine("Bad Response: \n\t" + e);
                response = (HttpWebResponse)e.Response;
            }
            return response;
        }

        static void Main(string[] args) {
            int bufSize = 1024;
            Stream inStream = Console.OpenStandardInput(bufSize);
            Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, bufSize));

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/cmd/Shutdown");
            //request.Method = "GET";
            //using (HttpWebResponse response = GetResponse(request)) {
            //    using (Stream stream = response.GetResponseStream()) {
            //        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
            //            Console.WriteLine(reader.ReadToEnd());
            //        }
            //    }
            //}

            if (EasyMarketingInUnity.Server.StartServer()) {

                while (EasyMarketingInUnity.Server.CheckServer()) {
                    ServerObject serverObj = null;
                    string query = "";

                    if (!EasyMarketingInUnity.Server.Instance.GetAuthenticator("Twitter").Authenticated) {
                        Prompt("Choose: ", new string[] { "Authenticate", "Exit" });
                        switch(GetSelection(1, 2)) {
                            case 1:
                                serverObj = EasyMarketingInUnity.Server.Instance.SendRequest("Twitter", HTTPMethod.Authenticate);
                                Console.WriteLine(serverObj);
                                break;
                            default:
                                EasyMarketingInUnity.Server.EndServer();
                                break;
                        }
                    } else {
                        Prompt("Choose: ", new string[] { "GET Twitter", "POST Twitter", "Exit" });
                        switch (GetSelection(1, 3)) {
                            case 1:
                                Console.WriteLine("Tweet ID? (Leave Blank for User's Timeline)");
                                var tweet_id = Console.ReadLine();
                                Console.WriteLine("Get Replies to tweet? (Leave Blank to get Specific Info)");
                                var reply = Console.ReadLine();

                                // 1053054196755312640
                                // 1052647238025891841
                                if (!(tweet_id == "" && reply == "")) {
                                    query = "tweet_id=" + tweet_id;
                                    query += "&reply=" + reply;
                                }

                                serverObj = EasyMarketingInUnity.Server.Instance.SendRequest("Twitter", HTTPMethod.Get, query);
                                Console.WriteLine(serverObj);
                                break;
                            case 2:
                                Console.Write("Tweet: ");
                                var status = Console.ReadLine();
                                Console.Write("Media File (Leave Blank to not upload): ");
                                var media = Console.ReadLine();
                                //Console.Write("Tweet Reply ID (Leave Blank for single tweet): ");
                                //var replyTo = Console.ReadLine();
                                //Console.Write("Multiple Tweets, Useful if the character count > 280 (Leave Blank for single tweet): ");
                                //var multiple = Console.ReadLine();

                                // C:\Users\Flameo326\Videos\video.mp4
                                // C:\Users\Flameo326\Videos\TestGif_NoAudio.gif
                                // C:\Users\Flameo326\Documents\IDEs\Unity\Capstone\Assets\Editor\Textures\Twitter.png
                                query = "status=" + status;
                                if (media != "") {
                                    query += "&media=" + media;
                                }
                                //if (replyTo != "") {
                                //    query += "&replyTo=" + replyTo;
                                //}
                                //if (multiple != "") {
                                //    query += "&multiple=" + multiple;
                                //}

                                serverObj = EasyMarketingInUnity.Server.Instance.SendRequest("Twitter", HTTPMethod.Post, query);
                                Console.WriteLine(serverObj);
                                break;
                            default:
                                EasyMarketingInUnity.Server.EndServer();
                                break;
                        }
                    }
                }
            }
            EasyMarketingInUnity.Server.EndServer();
        }
    }
}
// For Twitter Chunked Upload, Sig needs to be:
//The request URL
//The HTTP request method
//The query string parameters from the HTTP request line
//The oauth_* parameters

// Null Reference on Request and Response

    // SIMPLE MEDIA UPLOAD DOES NOT SUPPORT VIDEO BECAUSE TWITTER IS DUMB!!!!
    // LITERALLY MY IMAGE IS LARGER THAN MY VIDEO
    // WHY CAN I NOT UPLOAD A VIDEO?
    // WHY!!!!!!!!!!!!!!!!!!!!!11111


//true if the Exited event should be raised when the associated process is terminated(through either an exit or a call to Kill()); otherwise, false. The default is false. Note that the Exited event is raised even if the value of EnableRaisingEvents is false when the process exits during or before the user performs a HasExited check.

// Its almost like its sending a kill event to the process and immediately assuming its dead.