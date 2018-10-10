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
        private string serverURL;
        private string shutdownURL;
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
        public static bool StartServer(int port = 3000) {
            if (!CheckServer()) {
                EndServer();
            }
            if (Instance != null) {
                if(Instance.port == port) {
                    return true;
                }
                EndServer();             
            }

            string dir = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
            string file = "Start.bat";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + dir + file + "\"";
            startInfo.Arguments = port + " ";

            Instance = new Server();
            Instance.port = port;
            Instance.serverURL = "http://localhost:" + port + "/";
            Instance.shutdownURL = "cmd/Shutdown";
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
            if (Instance.process == null) {
                Instance = null;
                return true;
            }

            if (Instance.process.Responding) {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Instance.serverURL + Instance.shutdownURL);
                request.Method = "Get";
                Authenticator.AddCookiesToRequest(request);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) { }
            }

            if (!Instance.process.HasExited) {
                Instance.process.CloseMainWindow();
            }
            try {
                Instance.process.Kill();
            } catch (InvalidOperationException e) {}
            catch (NotSupportedException e) {}
            catch (System.ComponentModel.Win32Exception e) {}     

            Instance.process = null;

            Instance = null;

            return true;
        }

        // True if Server is functioning, I think it works...
        public static bool CheckServer() {
            return Instance != null && (Instance.process.Responding || !Instance.process.HasExited);
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
        public Authenticator GetAuthenticator(string name) {
            name = name.ToLower();
            return authenticators.Find((auth) => {
                return auth.Name == name;
            });
        }
        public Authenticator[] GetAuthenticators() { return authenticators.ToArray(); }

        public JObject SendRequest(string authName, Method cmdMethod, string query = "", string body = "") {
            authName = authName.ToLower();
            Authenticator auth = authenticators.Find((authenticator) => authenticator.Name == authName);

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
                default:
                    method = "GET";
                    break;
            }

            if (query != "") {
                url = url + "?" + query;
            }
            Console.WriteLine(CredentialCache.DefaultCredentials);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UseDefaultCredentials = true;
            request.PreAuthenticate = true;
            request.Accept = "*/*";
            request.UserAgent = "curl/7.55.1";
            request.Method = method;
            request.Credentials = CredentialCache.DefaultCredentials;

            //request.GetRequestStream
            //request.ContentLength
            //request.ContentType
            //request.MediaType
            //request.Timeout
            //request.SendChunked

            // Add Cookies for Session Persistence
            Authenticator.AddCookiesToRequest(request);

            foreach (string key in request.Headers.AllKeys) {
                Console.WriteLine(key + ": " + request.Headers[key]);
            }

            JObject responseObj = null;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                Authenticator.AddCookiesFromResponse(response);
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        //response.ContentType

                        try {
                            responseObj = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());
                        } catch (JsonReaderException e) {
                            //Console.WriteLine(e);
                        }
                    }
                }
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