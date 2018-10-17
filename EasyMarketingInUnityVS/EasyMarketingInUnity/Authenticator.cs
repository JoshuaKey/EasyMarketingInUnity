using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyMarketingInUnity {
    [Serializable]
    public class Authenticator {
        public string Name { get; protected set; }
        public bool Authenticated { get; protected set; }

        public string authenticateCmdURL;
        public string postCmdURL;
        public string getCmdURL;

        public Authenticator(string name) {
            Name = name;
            Authenticated = false;
        }

        public bool CheckAuthentication(JObject obj) {
            // Search for Error Token
            var errorToken = obj["error"];

            // ErrorToken is null -> No Errors
            // ErrorToken: null -> No Errors
            Authenticated = errorToken == null || (errorToken.ToString() == "" || errorToken.ToString() == "null");
            return Authenticated;
        }
    }
}

