using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyMarketingInUnity {
    public class Authenticator {

        public string Name { get; protected set; }
        public bool Authenticated { get; protected set; }

        public string authenticateCmdURL;
        public string postCmdURL;
        public string getCmdURL;

        private static CookieContainer cookies = new CookieContainer();

        public Authenticator(string name) {
            Name = name.ToLower();
            Authenticated = false;

            //cookies = new CookieContainer();
            //Cookie c = new Cookie();
            //c.Domain = "localhost";
            //c.HttpOnly = true;
            //c.Path = "/";
            //c.Name = "connect.sid";
            //c.Value = "s%3Aafdba8cd-29ce-418a-9486-72b352451641.hWA7InHtcZB%2BZo%2FzzNg90tD7%2FGdIG7RvE6PY4D7g2Is";
            //cookies.Add(c);
        }

        public bool CheckAuthentication(JObject obj) {
            // Search for Error Token
            JToken errorToken = obj.GetValue("error");
            // Check if the token has a value or children
            if (errorToken == null || !errorToken.HasValues || errorToken.CreateReader().ReadAsString() != "") {
                Authenticated = true;
            }
            return Authenticated;
        }
        public static void AddCookiesFromResponse(HttpWebResponse response) {
            cookies.Add(response.Cookies);
            //foreach (Cookie c in response.Cookies) {
            //    Console.WriteLine(c);
            //    Console.WriteLine(c.Domain);
            //    Console.WriteLine(c.Name);
            //    Console.WriteLine(c.Path);
            //    Console.WriteLine(c.Port);
            //    Console.WriteLine(c.Secure);
            //    Console.WriteLine(c.Value);
            //    Console.WriteLine(c.Comment);
            //    Console.WriteLine(c.CommentUri);
            //    Console.WriteLine(c.Expired);
            //}
        }
        public static void AddCookiesToRequest(HttpWebRequest request) {
            //var CC = cookies.GetCookies(request.Address);
            //foreach (Cookie c in CC) {
            //    Console.WriteLine(c);
            //}

            request.CookieContainer = cookies;
        }
    }
}

