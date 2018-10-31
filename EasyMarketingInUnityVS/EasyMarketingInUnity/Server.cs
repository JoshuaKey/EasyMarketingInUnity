using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

        public delegate void OnLog(string message);
        public static OnLog onLog;

        [NonSerialized] private Process process;
        [NonSerialized] private bool onExitCalled;
        //private int previousID;

        private int port;
        private string serverURL;
        private string shutdownURL;

        private CookieContainer cookies = new CookieContainer();
        private List<Authenticator> authenticators = null;
        public static string saveFile = "server.dat";
        public static string logFile = "server.log";
        public static string directory = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
        public static string exe = "Start.bat";
        public static Server Instance = null;

        static readonly object Lock = new object();

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
        public static bool StartServer(int port = 3000, bool debug = true) {
            Server.Log("Attempting to Start Server");
            if (CheckServer()) {
                if (!EndServer()) {
                    return false;
                }
            }

            Server.Log("Starting Server");
            if (!SaveFileExists() || !LoadServer()) {
                Instance = new Server();
                Instance.port = port;
                Instance.serverURL = "http://localhost:" + port + "/";
                Instance.shutdownURL = "cmd/Shutdown";
                Instance.SetupGenericAuthenticators();
            } else {
                port = Instance.port;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + directory + exe + "\"";
            startInfo.Arguments = port + " ";
            if (!debug) {
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = directory;
            }

            // Cleanup Previous Process
            CleanupPrevousProcesses();

            // Start Process
            try {
                Instance.process = Process.Start(startInfo);
                //Instance.previousID = Instance.process.Id;
                //Console.WriteLine("Process ID: " + Instance.previousID);
            } catch (InvalidOperationException e) {
                Server.Log("Process Failed\n\t" + e);
                return false;
            } catch (FileNotFoundException e) {
                Server.Log("Process Failed\n\t" + e);
                return false;
            } catch (System.ComponentModel.Win32Exception e) {
                Server.Log("Process Failed\n\t" + e);
                return false;
            }
            Server.Log("Process Running (pID: " + Instance.process.Id + ")");


            SaveServer();

            Instance.process.EnableRaisingEvents = true;
            Instance.process.Exited += new EventHandler(OnExit);

            return true;
        }
        /// <summary>
        /// Kills the Server
        /// </summary>
        /// <returns>True if the server was shut down successfully or the server was already shutdown</returns>
        public static bool EndServer() {
            // Check for Prior Exit / Invalid State
            if (Instance == null) { return true; }
            if (Instance.process == null) {
                Instance = null;
                return true;
            }

            // Lock in case of OnExit Event
            lock (Lock) {
                Server.Log("Ending Server");
                // Save Server by default
                SaveServer(); 

                // Check for cleanup from previous Event
                if (Instance.onExitCalled) {
                    Instance.process = null;
                    Instance = null;
                    Server.Log("Process already Exited");
                    Server.Log(); // Add a Blank line
                    return true;
                }

                // Send Shutdown Request   
                if (IsResponding()) {
                    Server.Log("Sending Shutdown Request: \n\t" + Instance.serverURL + Instance.shutdownURL);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Instance.serverURL + Instance.shutdownURL);
                    request.Method = "GET";
                    using (HttpWebResponse response = GetResponse(request)) {
                        if (response != null) {
                            ServerObject serverObj = ServerObject.CreateServerResponse(response);
                            if (serverObj.status != 200) {
                                Server.Log("Server responded with:\n\t" + serverObj);
                            } else {
                                Server.Log("Shutdown Request sent Successfully!");
                            }
                        } else {
                            Server.Log("Did not recieve a response from Server");
                        }
                    }
                } 

                // Attempt to close process
                bool success = false;
                try {
                    int id = Instance.process.Id;

                    Server.Log("Closing Main Window");
                    success = Instance.process.CloseMainWindow();

                    // Check if process has exited
                    success = Instance.process.HasExited;
                    // Look for process
                    try {
                        if (Process.GetProcessById(id) != null) { success = false; }
                    } catch (ArgumentException e) { } catch (InvalidOperationException e) { }

                    Server.Log("Close " + (success ? "Successful" : "Failed"));
                } catch (InvalidOperationException e) {
                    Server.Log("Close Failed\n\t" + e);
                } catch (PlatformNotSupportedException e) {
                    Server.Log("Close Failed\n\t" + e);
                }

                // Attempts to Kill Process
                if (!success) {
                    try {
                        int id = Instance.process.Id;

                        Server.Log("Killing Process");
                        //Instance.process.Refresh();
                        Instance.process.Kill();
                        Instance.process.WaitForExit(3000);
                    
                        // Check if process has exited
                        success = Instance.process.HasExited;
                        // Look for process
                        try {
                            if (Process.GetProcessById(id) != null) { success = false; }
                        } catch(ArgumentException e) {}
                        catch(InvalidOperationException e) {}                      

                        if (success) {
                            Server.Log("Killing Success");
                            Server.Log("Exit Code: " + Instance.process.ExitCode);
                        } else {
                            Server.Log("Killing Failed");
                        }

                    } catch (InvalidOperationException e) {
                        Server.Log("Killing Failed\n\t" + e);
                    } catch (NotSupportedException e) {
                        Server.Log("Killing Failed\n\t" + e);
                    } catch (System.ComponentModel.Win32Exception e) {
                        Server.Log("Killing Failed\n\t" + e);
                    } catch(SystemException e) {
                        Server.Log("Killing Failed\n\t" + e);
                    }    
                }

                //CleanupPrevousProcesses();

                // Cleanup 
                if (Instance != null) {
                    if (Instance.process != null) {
                        Instance.process.Dispose();
                        Instance.process.Close();
                        Instance.process = null;
                    }
                    Instance = null;
                }
                Server.Log(); // Add a Blank line
                return success;
            }
        }
        
        private static void OnExit(object sender, System.EventArgs e) {
            var thread = new Thread(() => {
                lock (Lock) {
                    Server.Log("OnExit() is being Called");

                    Process sendProcess = null;
                    if (sender is Process) {
                       
                        sendProcess = (Process)sender;

                        sendProcess.Exited -= OnExit;
                        sendProcess.Dispose();
                        sendProcess.Close();

                        if (Instance != null && Instance.process == sendProcess) {
                            Server.Log("Process has Exited");
                            Instance.onExitCalled = true;
                        } else {
                            Server.Log("Different Process has exited.");
                        }

                    } else {
                        Server.Log("Sender is not a process!");
                    }
                    Server.Log(); // Add a Blank line
                }
            });
            thread.Start();           
        }
        private static void CleanupPrevousProcesses() {
            // By ID
            //if (Instance.previousID != 0) {
            //    try {
            //        var p = Process.GetProcessById(Instance.previousID);
            //        if (p != null) {
            //            Server.Log("Found Process: " + p.ProcessName + " (" + p.Id + ")\n\tAttempting to Kill");
            //            try {
            //                p.Kill();
            //            } catch (InvalidOperationException e) {
            //                Server.Log("Kill Failed: \n\t" + e);
            //            }
            //            Server.Log("Kill " + (p.HasExited ? "Successful" : "Failed"));
            //        }
            //    } catch (InvalidOperationException e) {
            //        Server.Log("Error finding Processes:\n\t" + e);
            //    } catch (ArgumentException e) {
            //        Server.Log("Error finding Processes:\n\t" + e);
            //    }
            //}
        }

        /// <summary>
        /// This method kills all processes that are listening to a specific port.
        /// THIS CAN BE DANGEROUS.
        /// Only use this method if Server Status is Invalid/Bad, The service does not work, and restarting Unity does not work.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool KillServers(int port) {
            List<int> ids = new List<int>();

            Server.Log("Attempting to kill Processes at Port: " + port);

            foreach (ProcessPort p in ProcessPorts.ProcessPortMap.FindAll(x => x.PortNumber == port)) {
                ids.Add(p.ProcessId);
                Server.Log("Found Process: " + p.ProcessName + " (" + p.ProcessId + ") @Port: " + p.PortNumber);
            }

            if(ids.Count == 0) {
                Server.Log("No Processes found");
                return true;
            }

            Server.Log("Killing Processes");
            foreach (var p in Process.GetProcesses()) {
                if (ids.Contains(p.Id)) {
                    p.Kill();
                    p.WaitForExit();
                    break;
                }
            }

            bool success = true;
            foreach (var p in Process.GetProcesses()) {
                if (ids.Contains(p.Id)) {
                    success = false;
                    Server.Log("Process: " + p.ProcessName + " (" + p.Id + ") has not been killed.");
                }
            }

            if (success) {
                Server.Log("Killing Succeeded");
            } else {
                Server.Log("Killing Failed");
            }
            return success;
        }

        /// <summary>
        /// Saves the Server at Server.savefile
        /// This includes Port, Authenticators and Cookies
        /// </summary>
        /// <returns></returns>
        public static bool SaveServer() {
            Server.Log("Saving Server");

            using (Stream stream = File.Open(saveFile, FileMode.Create)) {
                try {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    binaryFormatter.Serialize(stream, Instance);
                } catch(System.Runtime.Serialization.SerializationException e) {
                    Server.Log("Save Failed\n\t" + e);
                    return false;
                }
            }

            Server.Log("Save Successful");
            return true;
        }
        /// <summary>
        /// Loads the server at Server.saveFile
        /// Stores the server in Server.Instance
        /// </summary>
        /// <returns></returns>
        public static bool LoadServer() {
            Server.Log("Loading Server");

            using (Stream stream = File.Open(saveFile, FileMode.Open)) {
                try {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    Instance = (Server)binaryFormatter.Deserialize(stream);
                } catch (System.Runtime.Serialization.SerializationException e) {
                    Server.Log("Load Failed\n\t" + e);
                    return false;
                }
            }

            Server.Log("Load Successful");
            return true;
        }
        /// <summary>
        /// Checks if the Server.saveFile Exists
        /// </summary>
        /// <returns></returns>
        public static bool SaveFileExists() {
            return File.Exists(saveFile);
        }

        /// <summary>
        /// Logs the message to Server.logFile
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type">Log Level, i.e. ERROR, WARNING, INFO, DEBUG, etc.</param>
        /// <returns>True if it wrote to the file successfully.</returns>
        public static bool Log(string message = "", string type = "INFO") {           
            try {
                if (message == "") { File.AppendAllText(logFile, "\n"); return true; }

                string time = DateTime.UtcNow.ToString("h:mm:ss.fff tt");
                string log = type + " [" + time + "] : " + message + "\n";
                File.AppendAllText(logFile, log);

                onLog?.Invoke(log);

            } catch (IOException e) {
                return false;
            } catch (NotSupportedException e) {
                return false;
            } catch (System.Security.SecurityException e) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the server is valid. 
        /// Specifically Instance is valid and Process is Valid
        /// </summary>
        /// <returns>True if Server.Instance can safely be used</returns>
        public static bool CheckServer() {
            return Instance != null && Instance.process != null;
        }
        /// <summary>
        /// Checks if the process is functioning
        /// Specifically checks if its dunctioning has already exited and catches any errors.
        /// Relies on CheckServer
        /// </summary>
        /// <returns>True if the process is still running in the background</returns>
        public static bool IsResponding() {
            bool status = CheckServer();
            if (status) {
                try {
                    bool responding = Instance.process.Responding;
                    if (!responding) {
                        Server.Log("Server is not Responding");
                        return false;
                    }          

                    return true;
                } catch (InvalidOperationException e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                } catch (NotSupportedException e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                } catch (System.ComponentModel.Win32Exception e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                }
            }
            Server.Log("Server is invalid");
            return false;
        }
        /// <summary>
        /// Checks if the process has ended. Just because the prcoess isn't functioning doesn't mean it has ended
        /// </summary>
        /// <returns></returns>
        public static bool HasEnded() {
            bool status = CheckServer();
            if (status) {
                try {
                    bool exited = Instance.process.HasExited;
                    if (exited) {
                        Server.Log("Server has Exited");
                        return true;
                    }

                    return false;
                } catch (InvalidOperationException e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                } catch (NotSupportedException e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                } catch (System.ComponentModel.Win32Exception e) {
                    Server.Log("Server is Bad\n\t" + e);
                    return false;
                }
            }
            Server.Log("Server is invalid");
            return false;
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
            AddGenericAuthenticator("Discord");

            AddGenericAuthenticator("Reddit");
            AddGenericAuthenticator("Slack");
            AddGenericAuthenticator("Instagram");

            AddGenericAuthenticator("Youtube");
            AddGenericAuthenticator("Itch");
            AddGenericAuthenticator("VKontakte");

            AddGenericAuthenticator("Google Plus");
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
        public ServerObject SendRequest(string authName, HTTPMethod cmdMethod, string query = "", string body = "") {
            Authenticator auth = GetAuthenticator(authName);

            // Error Checking
            if (auth == null) {
                Server.Log("No Authenticator Found");
                return ServerObject.CreateErrorResponse(new ArgumentException("No Authenticator Found"));
            } 
            if (!auth.Authenticated  && cmdMethod != HTTPMethod.Authenticate) {
                Server.Log("Authenticator has not been authenticated");
                var e = new InvalidOperationException("Authenticator has not been authenticated");
                return ServerObject.CreateErrorResponse(e);
            }
            if (cmdMethod == HTTPMethod.Post && query == "" && body == "") {
                Server.Log("Can not post without Body or Query");
                var e = new ArgumentException("Can not post without Body or Query");
                return ServerObject.CreateErrorResponse(e);
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

            Server.Log("Sending Request:\n\t" + method + " " + url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;

            // Add Cookies for Session Persistence - Probably not needed...
            AddCookiesToRequest(request);

            ServerObject serverObj = null;
            using (HttpWebResponse response = GetResponse(request)) {
                if(response == null) { Server.Log("Did not recieve a response from Server");  return null; }
                AddCookiesFromResponse(response);
                serverObj = ServerObject.CreateServerResponse(response);
                Server.Log("Recieved Response:\n\t" + serverObj);
            }

            if (cmdMethod == HTTPMethod.Authenticate) {
                bool pass = auth.CheckAuthentication(serverObj);
                Server.Log(auth.Name + " Authentication " + (pass ? "Succeeded" : "Failed"));
            }

            return serverObj;
        }

        public ServerObject SendAsyncRequest(string name, HTTPMethod method) {
            return null;
        }

        /// <summary>
        /// Gets HttpWebResponse even if an error is thrown. This helps for 400 or 404 status codes.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static HttpWebResponse GetResponse(HttpWebRequest request) {
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException e) {
                Server.Log("Bad Response: \n\t" + e);
                response = (HttpWebResponse)e.Response;
            }
            return response;
        }

        private void AddCookiesFromResponse(HttpWebResponse response) {
            //PrintCookies();
            cookies.Add(response.Cookies);
        }
        private void AddCookiesToRequest(HttpWebRequest request) {
            //PrintCookies();
            request.CookieContainer = cookies;
        }
        private void PrintCookies() {
            CookieCollection CC = cookies.GetCookies(new Uri("http://localhost/"));
            Server.Log("\nCookies in http://localhost/:");
            foreach (Cookie c in CC) {
                Server.Log("\t" + c.Name + ": " + c.Value);
            }
        }
    }
}