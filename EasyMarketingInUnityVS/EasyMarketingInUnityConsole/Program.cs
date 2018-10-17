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

        static void Main(string[] args) {

            if (Server.StartServer()) {
                while (Server.Instance != null && Server.CheckServer()) {
                    JObject JSON = null;
                    string query = "";

                    if (!Server.Instance.GetAuthenticator("Twitter").Authenticated) {
                        Prompt("Choose: ", new string[] { "Authenticate", "Exit" });
                        switch(GetSelection(1, 2)) {
                            case 1:
                                JSON = Server.Instance.SendRequest("Twitter", HTTPMethod.Authenticate);
                                Console.WriteLine(JSON);
                                break;
                            case 2:
                                Server.EndServer();
                                break;
                        }
                    } else {
                        Prompt("Choose: ", new string[] { "GET Twitter", "POST Twitter", "Post Image Twitter", "Exit" });
                        switch (GetSelection(1, 4)) {
                            case 1:
                                JSON = Server.Instance.SendRequest("Twitter", HTTPMethod.Get);
                                Console.WriteLine(JSON);
                                break;
                            case 2:
                                Console.Write("Tweet: ");
                                query = "status=" + Console.ReadLine();

                                JSON = Server.Instance.SendRequest("Twitter", HTTPMethod.Post, query);
                                Console.WriteLine(JSON);
                                break;
                            case 3:
                                Console.Write("Tweet: ");
                                var status = Console.ReadLine();
                                Console.Write("Media File: ");
                                var media = Console.ReadLine();
                                // C:\Users\Flameo326\Documents\IDEs\Unity\Capstone\Assets\Editor\Textures\Twitter.png

                                query = "status=" + status + "&media=" + media;

                                JSON = Server.Instance.SendRequest("Twitter", HTTPMethod.Post, query);
                                Console.WriteLine(JSON);
                                break;
                            case 4:
                                Server.EndServer();
                                break;
                        }
                    }
                }
            }
            Server.EndServer();
        }
    }
}
// For Twitter Chunked Upload, Sig needs to be:
//The request URL
//The HTTP request method
//The query string parameters from the HTTP request line
//The oauth_* parameters

// SO I AM sending the Session ID, but for some reason, Express cant find the user based off that???

// Peername
// Idle Start
//225.	        [Symbol(asyncId)]: 23,	225.	        [Symbol(asyncId)]: 265,
//226.	        [Symbol(triggerId)]: 22 },	226.	        [Symbol(triggerId)]: 264 },

//432.	  headers:	                        432.	  headers:
//433.	   { accept: '*/*',	                433.	   { host: 'localhost:3000',
//434.	     'user-agent': 'curl/7.55.1',	434.	     'user-agent': 'curl/7.55.1',
//435.	     host: 'localhost:3000',	    435.	     accept: '*/*' },
//436.	     connection: 'Keep-Alive' },		
//437.	  rawHeaders:	                    436.	  rawHeaders:
//438.	   [ 'Accept',                      437.	   [ 'Host',
//439.	     '*/*',                         438.	     'localhost:3000',
//440.	     'User-Agent',                  439.	     'User-Agent',
//441.	     'curl/7.55.1',                 440.	     'curl/7.55.1',
//442.	     'Host',                        441.	     'Accept',
//443.	     'localhost:3000',              442.	     '*/*' ],
//444.	     'Connection',		
//445.	     'Keep-Alive' ],
//

// If I can get Authorize to recieve the Response from the Callback method, aka the cookie, then I can set saveUnitialized=false and save space on sessions...