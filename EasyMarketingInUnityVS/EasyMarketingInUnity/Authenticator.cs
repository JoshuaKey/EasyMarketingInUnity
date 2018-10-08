using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyMarketingInUnity {
    public class Authenticator {

        public string Name { get; protected set; }

        // PLEASE DO NOT SET THIS VARIABLE. I AM BAD AND CAN'T FIGURE OUT HOW TO ONLY LET SERVER ACCESS THIS VARIABLE
        public bool Authenticated { get; set; }

        public string authenticateCmdURL;
        public string postCmdURL;
        public string getCmdURL;

        public Authenticator(string name) {
            Name = name.ToLower();
            Authenticated = false;
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

    }
}

