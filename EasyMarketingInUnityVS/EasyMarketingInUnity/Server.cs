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
    public enum HTTPMethod {
        Authenticate,
        Post,
        Get
    }

    [Serializable]
    public class Server : IDisposable {

        [NonSerialized] private Process process;
        private int port;
        private string serverURL;
        private string shutdownURL;

        private CookieContainer cookies = new CookieContainer();
        private List<Authenticator> authenticators = null;
        public static string saveFile = "server.dat";
        public static Server Instance = null;

        private Server() { }
        ~Server() { Dispose(); }
        public void Dispose() { EndServer(); }

        /// <summary>
        /// Creates and maintains a Singleton Server obj for the Express Server.
        /// Opens up a new process that runs the server using Cmd Prompt
        /// </summary>
        /// <param name="debug">Whether or not the Process should be shown, and wether to log Data</param>
        /// <param name="port">Which port to run the Server on</param>
        /// <returns>True if creating the server was successful, or the server was already started</returns>
        public static bool StartServer(int port = 3000) {
            Console.WriteLine("Attempting to Start Server");
            if (CheckServer()) {
                EndServer();
            }
            if (Instance != null) {
                if(Instance.port == port) {
                    return true;
                }
                EndServer();             
            }

            Console.WriteLine("Starting Server");
            if (!SaveFileExists() || !LoadServer()) {
                Instance = new Server();
                Instance.port = port;
                Instance.serverURL = "http://localhost:" + port + "/";
                Instance.shutdownURL = "cmd/Shutdown";
                Instance.SetupGenericAuthenticators();
            } else {
                port = Instance.port;
            }

            string dir = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
            string file = "Start.bat";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + dir + file + "\"";
            startInfo.Arguments = port + " ";

            Console.WriteLine("Starting Process");
            Instance.process = Process.Start(startInfo);

            return true;
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
            Console.WriteLine("Ending Server");

            SaveServer();

            //if (Instance.process.Responding) {
            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Instance.serverURL + Instance.shutdownURL);
            //    request.Method = "Get";
            //    //AddCookiesToRequest(request);
            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) { }
            //}

            if (!Instance.process.HasExited) {
                Instance.process.CloseMainWindow();
            }

            bool success = true;
            Console.WriteLine("Killing Process");
            try {
                Instance.process.Kill();
            } catch (InvalidOperationException e) {
                Console.WriteLine("Killing Failed");
                Console.WriteLine(e);
                return false;
            }
            catch (NotSupportedException e) {
                Console.WriteLine("Killing Failed");
                Console.WriteLine(e);
                return false;
            } catch (System.ComponentModel.Win32Exception e) {
                Console.WriteLine("Killing Failed");
                Console.WriteLine(e);
                return false;
            }

            if (success) {
                Console.WriteLine("Killing Success");
            }

            Instance.process = null;
            Instance = null;
            return true;
        }

        /// <summary>
        /// Saves the Server at Server.savefile
        /// This includes Port, Authenticators and Cookies
        /// </summary>
        /// <returns></returns>
        public static bool SaveServer() {
            Console.WriteLine("Saving Server");

            using (Stream stream = File.Open(saveFile, FileMode.Create)) {
                try {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    binaryFormatter.Serialize(stream, Instance);
                } catch(System.Runtime.Serialization.SerializationException e) {
                    Console.WriteLine("Save Failed");
                    Console.WriteLine(e);
                    return false;
                }
            }

            Console.WriteLine("Save Successful");
            return true;
        }
        /// <summary>
        /// Loads the server at Server.saveFile
        /// Stores the server in Server.Instance
        /// </summary>
        /// <returns></returns>
        public static bool LoadServer() {
            Console.WriteLine("Loading Server");


            using (Stream stream = File.Open(saveFile, FileMode.Open)) {
                try {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    Instance = (Server)binaryFormatter.Deserialize(stream);
                } catch (System.Runtime.Serialization.SerializationException e) {
                    Console.WriteLine("Load Failed");
                    Console.WriteLine(e);
                    return false;
                }
            }

            Console.WriteLine("Load Successful");
            return true;
        }
        /// <summary>
        /// Checks if the Server.saveFile Exists
        /// </summary>
        /// <returns></returns>
        public static bool SaveFileExists() {
            return File.Exists(saveFile);
        }

        // True if Server is functioning, I think it works...
        public static bool CheckServer() {
            return Instance != null && (Instance.process.Responding || !Instance.process.HasExited);
        }

        /// <summary>
        /// Initializes the Authenticator List and Supplies it with Generic Sites
        /// </summary>
        public void SetupGenericAuthenticators() {
            if (authenticators == null) {
                authenticators = new List<Authenticator>();
            }
            AddGenericAuthenticator("Twitter");
            AddGenericAuthenticator("Facebook");
            AddGenericAuthenticator("Google Plus");
            AddGenericAuthenticator("Youtube");
            AddGenericAuthenticator("Itch");
            AddGenericAuthenticator("Discord");
            AddGenericAuthenticator("Reddit");
            AddGenericAuthenticator("VK");
            AddGenericAuthenticator("Slack");
            AddGenericAuthenticator("Instagram");
        }
        /// <summary>
        /// For Internal use only
        /// Adds a generic predetermined Authenticator like Twitter or Google
        /// </summary>
        /// <param name="name">Generic name for Authenticator and Route</param>
        private void AddGenericAuthenticator(string name) {
            Authenticator authenticator = new Authenticator(name);
            authenticator.authenticateCmdURL = "cmd/" + name + "/Authenticate";
            authenticator.getCmdURL = "cmd/" + name + "/Get";
            authenticator.postCmdURL = "cmd/" + name + "/Post";
            AddAuthenticator(authenticator);
        }

        /// <summary>
        /// Adds an Authenticator to the server.
        /// An Authenticator is a service that can be authenticated and allows for posting and getting data from.
        /// By Default, several Authenticators are already added
        /// </summary>
        /// <param name="auth"></param>
        public void AddAuthenticator(Authenticator auth) {
            Authenticator found = GetAuthenticator(auth.Name);
            if(found == null) {
                authenticators.Add(auth);
            }
        }
        /// <summary>
        /// Retireves the Authenticator
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the Authenticator with the given name, null if not found</returns>
        public Authenticator GetAuthenticator(string name) {
            name = name.ToLower();
            return authenticators.Find((auth) => {
                return auth.Name.ToLower() == name;
            });
        }
        public Authenticator[] GetAuthenticators() { return authenticators.ToArray(); }

        /// <summary>
        /// Allows an Authenticator to send a request to the server.
        /// </summary>
        /// <param name="authName">Name of the Authenticator</param>
        /// <param name="cmdMethod">Which Method or Route to use</param>
        /// <param name="query">Attached to URL, ex. q?status=...</param>
        /// <param name="body">Added in the body of the Post</param>
        /// <returns>Returns the JSON Response from the Server, which contains Error: and Response: from API</returns>
        public JObject SendRequest(string authName, HTTPMethod cmdMethod, string query = "", string body = "") {
            Authenticator auth = GetAuthenticator(authName);

            // Error Checking
            if (auth == null) {
                Console.WriteLine("No Authenticator Found");
                return null;
                //throw new ArgumentException("No Authenticator Found");
            } 
            if(!auth.Authenticated  && cmdMethod != HTTPMethod.Authenticate) {
                Console.WriteLine("Authenticator has not been authenticated");
                return null;
                //throw new InvalidOperationException("Authenticator has not been authenticated");
            }
            if (cmdMethod == HTTPMethod.Post && query == "" && body == "") {
                Console.WriteLine("Can not post without Body or Query");
                return null;
                //throw new ArgumentException("Can not post without Body or Query");
            }

            string method = "";
            string url = serverURL;

            switch (cmdMethod) {
                case HTTPMethod.Authenticate:
                    method = "GET";
                    url += auth.authenticateCmdURL;
                    break;
                case HTTPMethod.Post:
                    method = "POST";
                    url += auth.postCmdURL;
                    break;
                case HTTPMethod.Get:
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UseDefaultCredentials = true;
            request.PreAuthenticate = true;
            request.Accept = "*/*";
            request.UserAgent = "curl/7.55.1";
            request.Method = method;
            request.Credentials = CredentialCache.DefaultCredentials;

            // Add Cookies for Session Persistence - Probably not needed...
            AddCookiesToRequest(request);

            JObject responseObj = null;
            string responseStr = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                AddCookiesFromResponse(response);
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        responseStr = reader.ReadToEnd();
                        try {
                            responseObj = (JObject)JsonConvert.DeserializeObject(responseStr);
                        } catch (JsonReaderException) {
                            responseObj = new JObject(responseStr);
                        }
                    }
                }
            }

            if (cmdMethod == HTTPMethod.Authenticate) {
                auth.CheckAuthentication(responseObj);
            }
            return responseObj;
        }

        public bool SendAsyncRequest(string name, HTTPMethod method) {
            return false;
        }

        private void AddCookiesFromResponse(HttpWebResponse response) {
            PrintCookies();
            cookies.Add(response.Cookies);
        }
        private void AddCookiesToRequest(HttpWebRequest request) {
            PrintCookies();
            request.CookieContainer = cookies;
        }
        private void PrintCookies() {
            CookieCollection CC = cookies.GetCookies(new Uri("http://localhost/"));
            Console.WriteLine("\nCookies in http://localhost/:");
            foreach (Cookie c in CC) {
                Console.WriteLine("\t" + c.Name + ": " + c.Value);
            }
            Console.WriteLine();
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