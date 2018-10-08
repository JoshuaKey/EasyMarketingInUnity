using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyMarketingInUnity {
    public enum Method {
        Authenticate,
        Post,
        Get
    }

    public class Server : IDisposable{

        private Process process;
        private int port;
        private bool debug;
        private string serverURL;
        private List<Authenticator> authenticators = new List<Authenticator>();

        public static Server Instance = null;
        private Server() { }
        ~Server() { Dispose(); }

        /// <summary>
        /// Creates and maintains a Singleton Server obj for the Express Server.
        /// Opens up a new process that runs the server using Cmd Prompt
        /// </summary>
        /// <param name="debug">Whether or not the Process should be shown, and wether to log Data</param>
        /// <param name="port">Which port to run the Server on</param>
        /// <returns>True if creating the server was successful, or the server was already started</returns>
        public static bool StartServer(bool debug = false, int port = 3000) {
            if (Instance != null) {
                if(Instance.port == port && Instance.debug == debug) {
                    return true;
                }
                EndServer();             
            }

            string dir = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
            string file = "Start.bat";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + dir + file + "\"";
            //startInfo.CreateNoWindow = true;
            ////startInfo.WindowStyle = debug ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            //startInfo.UseShellExecute = false;
            ////startInfo.WindowStyle = ProcessWindowStyle.
            startInfo.Arguments = port + " " + debug;

            Instance = new Server();
            Instance.port = port;
            Instance.debug = debug;
            Instance.serverURL = "http://localhost:" + port + "/";
            Instance.process = Process.Start(startInfo);
            

            Authenticator twitter = new Authenticator("Twitter");
            twitter.authenticateCmdURL = "cmd/Twitter/Authenticate";
            twitter.getCmdURL = "cmd/Twitter/Get";
            twitter.postCmdURL = "cmd/Twitter/Post";
            Instance.AddAuthenticator(twitter);

            return true;
        }

        public void Dispose() {
            EndServer();
        }
        /// <summary>
        /// Kills the Server
        /// </summary>
        /// <returns>True if the server was shut down successfully or the server was already shutdown</returns>
        public static bool EndServer() {
            if (Instance == null) { return true; }
            if (Instance.process == null) { return true; }

            if (!Instance.process.HasExited) {
                Instance.process.CloseMainWindow();
            }
            Instance.process.Kill();

            Instance.process = null;

            Instance = null;

            return true;
        }

        // True if Server is functioning, I think it works...
        public bool CheckServer() {
            return process.Responding || !process.HasExited;
        }

        /// <summary>
        /// Adds an Authenticator to the server.
        /// An Authenticator is a service that can be authenticated and allows for posting and getting data from.
        /// By Default, several Authenticators are already added
        /// </summary>
        /// <param name="auth"></param>
        public void AddAuthenticator(Authenticator auth) {
            Authenticator found = authenticators.Find((auth2) => auth2.Name == auth.Name);
            if(found == null) {
                authenticators.Add(auth);
            }
        }
        // You really shouldn't need to call this method.
        public Authenticator GetAuthenticator(string name) {
            return authenticators.Find((auth) => {
                return auth.Name == name;
            });
        }

        public JObject SendRequest(string name, Method cmdMethod, string query = "", string body = "") {
            name = name.ToLower();
            Authenticator auth = authenticators.Find((authenticator) => authenticator.Name == name);

            // Throw Error?
            // Error Checking
            if (auth == null) {
                throw new ArgumentException("No Authenticator Found");
            } 
            if(!auth.Authenticated  && cmdMethod != Method.Authenticate) {
                throw new InvalidOperationException("Authenticator has not been authenticated");
            }
            if (cmdMethod == Method.Post && query == "" && body == "") {
                throw new ArgumentException("Can not post without Body or Query");
            }

            string method = "";
            string url = serverURL;

            switch (cmdMethod) {
                case Method.Authenticate:
                    method = "GET";
                    url += auth.authenticateCmdURL;
                    break;
                case Method.Post:
                    method = "POST";
                    url += auth.postCmdURL;
                    
                    break;
                case Method.Get:
                    method = "GET";
                    url += auth.getCmdURL;
                    break;
            }

            if (query != "") {
                url = url + "?" + query;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            //request.GetRequestStream
            //request.ContentLength
            //request.ContentType
            //request.MediaType
            //request.Timeout
            //request.SendChunked
            

            JObject responseObj = null;
            using (WebResponse response = request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        responseObj = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());
                    }
                }
                response.Close();
            }

            if (cmdMethod == Method.Authenticate) {
                auth.CheckAuthentication(responseObj);
            }

            return responseObj;
        }
        public bool SendAsyncRequest(string name, Method method) {
            return false;
        }
    }
}

// So...
// How about, you create a server by calling StartServer. This will run the batch file for the Express Server
// Then you can add Authenticators, or sites that use oAuth and can Authenticate, Post and Get Content from them.
// After they've been added, you can call SendRequest with the Method and recieve the result
// At the End, you can call EndServer, Dispose, or wait for Garbage Collection to remove the Server.

// Maybe I should make everything static....
// How to Appropriately stop a Process
// How to Check Server... Is it correct?

// Should Routes be get, or post, or all?