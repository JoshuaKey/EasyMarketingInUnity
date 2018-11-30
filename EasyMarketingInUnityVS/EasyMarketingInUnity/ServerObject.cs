using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace EasyMarketingInUnity {
    public class ServerObject {
        public int status { get; set; }
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public string displayMessage { get; set; }
        public JToken results { get; set; } 

        public static ServerObject CreateServerResponse(HttpWebResponse response) {
            ServerObject serverObj = new ServerObject();

            string responseStr = "";
            try {
               
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        responseStr = reader.ReadToEnd();

                        //Console.WriteLine(responseStr);

                        serverObj = JsonConvert.DeserializeObject<ServerObject>(responseStr);                    
                        if(serverObj == null) {
                            throw new JsonException("Response was Empty");
                        }                        
                    }
                }

            } catch (IOException e) {
                serverObj = CreateErrorResponse(e);
            } catch (JsonException e) {
                serverObj = CreateErrorResponse(e);
            }

            return serverObj;
        }
        public static ServerObject CreateErrorResponse(Exception e = null) {
            ServerObject serverObj = new ServerObject();
            serverObj.status = 500;
            serverObj.errorCode = -1;
            serverObj.errorMessage = "Something went wrong in the DLL.\n" + (e == null ? "" : e.ToString());
            serverObj.displayMessage = "Something went wrong";
            serverObj.results = null;
            return serverObj;
        }

        public override string ToString() {
            string str;

            str = "Status: " + status + " (" + errorCode + ") " + displayMessage;
            str += "\n\tError: " + errorMessage;
            str += "\n\tResults: " + results;

            return str;
        }
    }
}

// Why does JConver Deserialize return null
// Why is response empty...