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

        public bool CheckAuthentication(ServerObject serverObj) {
            return Authenticated = (serverObj.errorCode == 0);
        }
    }
}

