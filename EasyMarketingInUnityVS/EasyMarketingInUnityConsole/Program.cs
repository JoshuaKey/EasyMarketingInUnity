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

            Dictionary<string, string[]> getParams = new Dictionary<string, string[]>();
            Dictionary<string, string[]> postParams = new Dictionary<string, string[]>();

            getParams.Add("Twitter", new string[] { "tweet_id", "reply" });
            postParams.Add("Twitter", new string[] { "status", "media", "like", "replyTo", "multiple" });
            //1057735038425432064

            //Testing Reply Chains 
            //C:\Users\Flameo326\Pictures\Flameo.jpg

            postParams.Add("Facebook", new string[] { "message", "like", "pageID"  });
            getParams.Add("Facebook", new string[] { "profileID", "accountIndex",  });
            // Test PAge - 704927669881541
            // 1909633502435072_920965411301891
            //fields=likes.summary(false)


            postParams.Add("Discord", new string[] { "channel", "message", "file", "like"});
            getParams.Add("Discord", new string[] { "channel", "message", "like" });
            // 507287942356795392 - Announcements
            // 507287492777607170 - General
            // 507287908881924128 - UI - UX
            // 507292807728332800 - Announce Message
            // Easy Emojis ID - 509167432280309760

            //[{"name": "TestHook", "channel_id": "507287492777607170", "token": "Ev9cvQGhZYXOZyDrk_WhehtNukhE-WdzM05GvVbWX9bM3Q4RwI5C1dyibK9yWCvxLfRU", "avatar": null, "guild_id": "507287492777607168", "id": "510519700174929922", "user": {"username": "Flameo326", "discriminator": "0904", "id": "205531180852969473", "avatar": "b5a8375119369c6fc878709746933c9b"}}

            postParams.Add("Reddit", new string[] { "objID",  "title", "message", "flair", "isComment", "vote", });
            getParams.Add("Reddit", new string[] { "subscribed", "subreddit", "article" });
            // Subreddits data.display_name or data.title
            // Subreddits data.id data.url kind
            // gamedesign
            // t3_8pbi01 -> Likes

            // Getting User posts  = Good
            // Getting Comments = Good
            // Liking a Post = Good
            // Post a Thread = Good
            // Post a Comment = Good
            // Post a flair = ???
            // Audio or Image = ???

            postParams.Add("Slack", new string[] { "channel", "message", "file", "timestamp" });
            getParams.Add("Slack", new string[] { "channel" });
            // Random Channel = C6MH8AVKM
            // Maple Message = 1532732100.000114
            // file = C:\Users\Flameo326\Pictures\Flameo.jpg

            //postParams.Add("Youtube", new string[] { });
            //getParams.Add("Youtube", new string[] { });

            //postParams.Add("Itch", new string[] { });
            //getParams.Add("Itch", new string[] { });

            postParams.Add("Vkontakte", new string[] { "message", "postID", "like" });
            getParams.Add("Vkontakte", new string[] { "postID", "type"});

            Server.StartServer(3000, true);

            while (Server.CheckServer()) {

                string auth = ChooseAuth();
                if (auth == "") {
                    break;
                }

                while (auth != "") {

                    HTTPMethod method;
                    if (ChooseCmd(auth, out method)) {

                        string[] queryParams = null;
                        switch (method) {
                            case HTTPMethod.Authenticate:
                                break;
                            case HTTPMethod.Post:
                                queryParams = postParams[auth];
                                break;
                            case HTTPMethod.Get:
                                queryParams = getParams[auth];
                                break;
                        }

                        string query = ChooseParameters(queryParams);
                        var serverObj = Server.Instance.SendRequest(auth, method, query);
                        Console.WriteLine(serverObj);

                    } else {
                        auth = "";
                    }

                }
            }

            Server.EndServer();

            
        }

        static string ChooseAuth() {
            var authNames = Server.Instance.GetAuthenticators().Select(x => x.Name).ToList();
            authNames.Add("Quit");
            Prompt("Choose a Site", authNames.ToArray());

            int selection = GetSelection(0, authNames.Count()) - 1;

            if (selection == authNames.Count - 1) {
                return "";
            }

            return authNames.ElementAt(selection);
        }

        static bool ChooseCmd(string authName, out HTTPMethod meth) {

            Authenticator auth = Server.Instance.GetAuthenticator(authName);
            string[] cmds;
            if (auth.Authenticated) {
                cmds = new string[] { "Get", "Post", "Back" };
            } else {
                cmds = new string[] { "Authenticate", "Back" };
            }
           
            Prompt("Choose a Command", cmds);
            int selection = GetSelection(1, cmds.Length) - 1;

            if(selection == cmds.Length - 1) {
                meth = HTTPMethod.Authenticate;
                return false;
            } 

            if (auth.Authenticated) {
                meth = selection == 0 ? HTTPMethod.Get : HTTPMethod.Post;
            } else {
                meth = HTTPMethod.Authenticate;
            }

            return true;
        }

        static string ChooseParameters(string[] queryParam) {
            if(queryParam == null) { return ""; }

            string query = "";

            for(int i = 0; i < queryParam.Length; i++) {
                Console.Write(queryParam[i] + " (Empty for Null): ");
                string input = Console.ReadLine();

                if(input != "") {
                    if (query != "") { query += "&"; }
                    query += queryParam[i] + "=" + input;
                }
            }

            return query;
        }

    }
}

// For Twitter Chunked Upload, Sig needs to be:
//The request URL
//The HTTP request method
//The query string parameters from the HTTP request line
//The oauth_* parameters