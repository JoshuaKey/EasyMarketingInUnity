using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EasyMarketingInUnity {
    public class ResponsesWindow : EditorWindow {

        ResponseData data;
        SettingData settings;

        [MenuItem("Window/Easy Marketing in Unity/Responses", priority = 2001)]
        public static void ShowWindow() {
            ResponsesWindow window = EditorWindow.GetWindow<ResponsesWindow>(false, "Responses", true);
            WindowData.onInit += window.Init;
            WindowData.Init();
            window.Show();
        }

        private void Init() {
            data = WindowData.responseData;
            settings = WindowData.settingData;

            var authenticators = Server.Instance.GetAuthenticators();
            for (int i = 0; i < authenticators.Length; i++) {
                var auth = authenticators[i];

                data.authDelay.Add(new KeyValuePair<string, float>(auth.Name, 0));
                data.authResponses.Add(new KeyValuePair<string, List<ResponseData.Message>>(auth.Name, null));

                data.authBools.Add(new AnimBool());
            }

            WindowData.onInit -= this.Init;
        }

        private void OnGUI() {
            if (data == null) {
                WindowUtility.DisplayInitializingScreen();
            } else {
                if (data.replyToMessage != null) {
                    DisplayReply();
                } else {
                    DisplayResponses();
                }          
            }
            Repaint();
        }

        private void DisplayResponses() {
            GUIStyle richCenter = new GUIStyle(GUI.skin.label);
            richCenter.alignment = TextAnchor.MiddleCenter;
            richCenter.wordWrap = true;
            richCenter.richText = true;

            GUIStyle rich = new GUIStyle(GUI.skin.label);
            rich.richText = true;

            GUIStyle text = new GUIStyle(GUI.skin.label);
            text.wordWrap = true;

            GUIStyle title = new GUIStyle(richCenter);
            title.fontSize = 30;

            // Header...
            var foldout = GUI.skin.GetStyle("Foldout");
            var fixedWidth = foldout.fixedHeight;
            var fontSize = foldout.fontSize;
            var richText = foldout.richText;
            var wordWrap = foldout.wordWrap;

            foldout.fixedWidth = this.position.width;
            foldout.fontSize = 14;
            foldout.richText = true;
            foldout.wordWrap = true;

            WindowUtility.Horizontal(() => {
                GUILayout.Label("Responses", title);
            });
            GUILayout.Space(10);
            WindowUtility.DrawLine(1);

            WindowUtility.Scroll(ref data.scrollPos, () => {              
                var authenticators = Server.Instance.GetAuthenticators();
                for (int i = 0; i < authenticators.Length; i++) {
                    string name = authenticators[i].Name;
                    AnimBool val = data.authBools[i];

                    DisplaySite(name, ref val);
                    GUILayout.Space(10);
                    WindowUtility.DrawLine(1);
                }           
            }, true);

            foldout.fixedWidth = fixedWidth;
            foldout.fontSize = fontSize;
            foldout.richText = richText;
            foldout.wordWrap = wordWrap;
        }
        private void DisplayReply() {
            GUIStyle title = new GUIStyle(GUI.skin.label);
            title.fontSize = 30;
            title.alignment = TextAnchor.MiddleCenter;
            title.wordWrap = true;
            title.richText = true;

            GUIStyle text = new GUIStyle(GUI.skin.label);
            text.richText = true;
            text.wordWrap = true;

            WindowUtility.Scroll(ref data.scrollPos, () => {

                // Header
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("Replying To:", title);

                    GUILayout.FlexibleSpace();

                    WindowUtility.Vertical(() => {
                        if (GUILayout.Button("Back")) {
                            data.replyToMessage = null;
                        }
                    }, 7, 0);

                }, 10, 10);
                if(data.replyToMessage == null) { return; }

                // Info
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("<b>" + data.replyToMessage.name + "</b>: " + data.replyToMessage.message, text);
                }, 30, 10);

                GUILayout.Space(20);

                // Text Area
                WindowUtility.Horizontal(() => {
                    data.replyText = GUILayout.TextArea(data.replyText, GUILayout.Height(100));
                }, 30, 30);

                // Attachment
                WindowUtility.Horizontal(() => {
                    WindowUtility.DisplayAttachment(ref data.replyFile);
                }, 50, 50);

                // Post Button
                WindowUtility.Horizontal(() => {
                    if (GUILayout.Button("Post")) {
                        data.replyResult = "";

                        string query = PostingWindow.CreatePostQuery(data.replyToMessage.site, data.replyText, data.replyFile);

                        var res = Server.Instance.SendRequest(data.replyToMessage.site, HTTPMethod.Post, query);

                        data.replyResult = res.displayMessage + "\n";
                        
                        data.replyText = "";
                        data.replyFile = "";
                    }
                });

                // Error / Success
                WindowUtility.Horizontal(() => {
                    GUILayout.Label(data.replyResult);
                });
            }, true);
        }

        private void DisplaySite(string name, ref AnimBool val) {
            AnimBool tempVal = val;
            WindowUtility.Horizontal(() => {
                bool target = tempVal.target;
                tempVal.target = WindowUtility.Foldout(ref target, name);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Settings", GUILayout.Height(25))) {
                    SettingsWindow.ShowWindow();
                    WindowData.settingData.specificSite = name;
                }

                if (GUILayout.Button(WindowData.refreshImage, GUILayout.Width(25), GUILayout.Height(25))){
                    CheckResponses(name, true);
                }
            }, 5, 10);
            val = tempVal;


            WindowUtility.Fade(val.faded, () => {
                GUILayout.Space(10);

                if (WindowData.IMPLEMENTED_AUTHENTICATORS.Contains(name)) {
                    var auth = Server.Instance.GetAuthenticator(name);
                    if (auth.Authenticated) {
                        DisplayMessages(name);
                    } 
                    else {
                        // Authenticae Button
                        WindowUtility.Horizontal(() => {
                            WindowUtility.Vertical(() => {
                                if (GUILayout.Button("Authenticate")) {
                                    var res = Server.Instance.SendRequest(name, HTTPMethod.Authenticate);
                                    if (settings.debugMode) {
                                        Debug.Log(res);
                                    }
                                    if (auth.Authenticated) {
                                        PostingWindow.OnAuthenticate(name);
                                    }
                                }
                            }, 20, 20);
                        });
                    }
                } 
                else {
                    // Not Implemented
                    WindowUtility.Horizontal(() => {
                        WindowUtility.Vertical(() => {
                            GUILayout.Label("Not Implemented");
                        }, 20, 20);
                    });
                }
            });
        }

        private void DisplayMessages(string name) {
            CheckResponses(name);

             // Random Jumbo
            {
                //if (!data.authResponses.ContainsKey(name) || data.authResponses[name] == null) {
                //    ResponseData.Message[] temp = new ResponseData.Message[4];
                //    temp[0] = new ResponseData.Message();
                //    temp[0].liked = true;
                //    temp[0].message = "Hello";
                //    temp[0].name = "Joshua Key";

                //    temp[1] = new ResponseData.Message();
                //    temp[1].liked = true;
                //    temp[1].message = "This is a potato... Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah...";
                //    temp[1].name = "Joshua Key";

                //    temp[2] = new ResponseData.Message();
                //    temp[2].liked = false;
                //    temp[2].isReply = true;
                //    temp[2].message = "Acutally I'm just a kid...";
                //    temp[2].name = "Its a Kid";

                //    temp[3] = new ResponseData.Message();
                //    temp[3].liked = false;
                //    temp[3].isReply = true;
                //    temp[3].message = " Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah...";
                //    temp[3].name = "Its a Kid";
                //    data.authResponses[name] = temp;
                //}
            }

            List<ResponseData.Message> messages = data.authResponses.Find(x => x.Key == name).Value;

            if(messages == null) {
                WindowUtility.Horizontal(() => {
                    WindowUtility.Vertical(() => {
                        GUILayout.Label("...");
                    }, 20, 20);
                });
            } else {
                for (int i = 0; i < messages.Count; i++) {
                    if (messages[i].isReply) {
                        DisplayReplyMessage(messages[i]);
                    } else {
                        DisplayUserMessage(messages[i]);
                    }
                }
            }
        }

        private void DisplayUserMessage(ResponseData.Message mess) {
            GUIStyle text = new GUIStyle(GUI.skin.label);
            text.richText = true;
            text.wordWrap = true;

            WindowUtility.Horizontal(() => {
                GUILayout.Label("<b>" + mess.name + "</b>: " + mess.message, text);
            }, 30, 10);

            //WindowUtility.Horizontal(() => {
            //    GUIContent content;
            //    if (mess.liked) {
            //        content = new GUIContent(WindowData.likeImage, "Unlike this (Not Implemented)");
            //    } else {
            //        content = new GUIContent(WindowData.unlikeImage, "Like this (Not Implemented)");
            //    }
            //    if (GUILayout.Button(content, GUILayout.Width(30), GUILayout.Height(30))) {
            //        mess.liked = !mess.liked;
            //    }

            //    if (GUILayout.Button("Reply (Not Implemented)", GUILayout.ExpandWidth(false), GUILayout.Height(30))) {
            //        data.replyToMessage = mess;
            //    }
            //}, 30, 10);
        }
        private void DisplayReplyMessage(ResponseData.Message mess) {
            GUIStyle text = new GUIStyle(GUI.skin.label);
            text.richText = true;
            text.wordWrap = true;

            WindowUtility.Horizontal(() => {
                GUILayout.Label("<b>" + mess.name + "</b>: " + mess.message, text);
            }, 50, 10);
             
            //WindowUtility.Horizontal(() => {

            //    GUIContent content;
            //    if (mess.liked) {
            //        content = new GUIContent(WindowData.likeImage, "Unlike this (Not Implemented)");
            //    } else {
            //        content = new GUIContent(WindowData.unlikeImage, "Like this (Not Implemented)");
            //    }
            //    if (GUILayout.Button(content, GUILayout.Width(30), GUILayout.Height(30))) {
            //        mess.liked = !mess.liked;
            //    }
            //    if (GUILayout.Button("Reply (Not Implemented)", GUILayout.ExpandWidth(false), GUILayout.Height(30))) {
            //        data.replyToMessage = mess;
            //    }
            //}, 50, 10);
        }


        private void CheckResponses(string name, bool force = false) {
            if (force || !ResponseData.Contains(data.authDelay, name) ||
                    EditorApplication.timeSinceStartup > ResponseData.Get(data.authDelay, name)) {

                float nextTime = (float)EditorApplication.timeSinceStartup + settings.responseDelay / 1000f;
                ResponseData.Replace(data.authDelay, name, nextTime);

                ResponseData.Replace(data.authResponses, name, GetResponses(name));
            }
        }
        private List<ResponseData.Message> GetResponses(string name) {
            List<ResponseData.Message> messages = null;

            ServerObject res;
            string query = "";

            switch (name) {
                case "Twitter":
                    res = Server.Instance.SendRequest(name, HTTPMethod.Get);
                    messages = TwitterToMessage(res, null, settings.twitterShowReplies);
                    break;
                case "Discord":
                    query = "channel=" + settings.discordAllChannelIDs[settings.discordDefaultChannelIndex];
                    res = Server.Instance.SendRequest(name, HTTPMethod.Get, query);
                    messages = DiscordToMessage(res);
                    break;
                case "Reddit":
                    res = Server.Instance.SendRequest(name, HTTPMethod.Get);
                    messages = RedditToMessage(res, null, false);
                    break;
                case "Slack":
                    query = "channel=" + settings.slackAllChannelIDs[settings.slackDefaultChannelIndex];
                    res = Server.Instance.SendRequest(name, HTTPMethod.Get, query);
                    messages = SlackToMessage(res);
                    break;
            }

            return messages;
        }
        private List<ResponseData.Message> TwitterToMessage(ServerObject res, 
            ResponseData.Message prevMessage = null, bool getReplies = false) {
            List<ResponseData.Message> messages = new List<ResponseData.Message>();

            if (res.errorCode == 0) {
                if (res.results.Type == JTokenType.Array) { // Multiple Tweets
                    JArray array = JArray.Parse(res.results.ToString());

                    for (int i = 0; i < array.Count; i++) {
                        var element = array[i];

                        // Check If reply was towards itself...
                        if (!settings.twitterShowUserReplies && prevMessage != null) {
                            string userID = prevMessage.userId;
                            string currentUserID = element["user"]["id_str"].Value<string>();
                            if (userID == currentUserID){
                                continue;
                            }
                        }

                        // Check
                        if (!settings.twitterShowUserRetweets) {
                            JToken retweetToken = element["retweeted_status"];
                            if (retweetToken != null) {
                                continue;
                            }                 
                        }

                        ResponseData.Message message = new ResponseData.Message();
                        message.userId = element["user"]["id_str"].Value<string>();
                        message.messageId = element["id_str"].Value<string>();
                        message.message = element["text"].Value<string>();
                        message.name = element["user"]["name"].Value<string>();
                        message.liked = element["favorited"].Value<bool>();
                        message.isReply = prevMessage != null;
                        message.site = "Twitter";
                        messages.Add(message);

                        if (getReplies) {
                            string query = "tweet_id="+ message.messageId + "&reply=true";
                            var replyRes = Server.Instance.SendRequest("Twitter", HTTPMethod.Get, query);

                            messages.AddRange(TwitterToMessage(replyRes, message, false));
                        }
                    }
                } else { // Single Tweet
                    JToken token = res.results;

                    ResponseData.Message message = new ResponseData.Message();
                    message.userId = token["user"]["id_str"].Value<string>();
                    message.messageId = token["id_str"].Value<string>();
                    message.message = token["text"].Value<string>();
                    message.name = token["user"]["name"].Value<string>();
                    message.liked = token["favorited"].Value<bool>();
                    message.isReply = prevMessage != null;
                    message.site = "Twitter";
                    messages.Add(message);

                    if (getReplies) {
                        string query = "tweet_id=" + messages[0].messageId + "&reply=true";
                        var replyRes = Server.Instance.SendRequest("Twitter", HTTPMethod.Get, query);
                        if (settings.debugMode) {
                            Debug.Log(replyRes);
                        }
                        messages.AddRange(TwitterToMessage(replyRes, message, false));
                    }
                }
            } else { // Error
                ResponseData.Message message = new ResponseData.Message();
                message.message = res.errorMessage;
                message.name = res.displayMessage + " " + res.status + ":" + res.errorCode;
                message.isReply = prevMessage != null;
                message.site = "Twitter";
                messages.Add(message);
            }

            return messages;
        }
        private List<ResponseData.Message> DiscordToMessage(ServerObject res) {
            List<ResponseData.Message> messages = new List<ResponseData.Message>();

            if (res.errorCode == 0) {
                if (res.results.Type == JTokenType.Array) { // Multiple Tweets
                    JArray array = JArray.Parse(res.results.ToString());

                    var messArray = array.Where((x) => {
                        return x["type"].Value<int>() == 0;
                    });

                    for (int i = 0; i < messArray.Count(); i++) {
                        JToken token = messArray.ElementAt(i);

                        ResponseData.Message message = new ResponseData.Message();
                        message.userId = token["author"]["id"].Value<string>();
                        message.messageId = token["id"].Value<string>();
                        message.message = token["content"].Value<string>();
                        message.name = token["author"]["username"].Value<string>();
                        if (token["reactions"] != null && token["reactions"].HasValues) {
                            var reactArray = JArray.Parse(token["reactions"].ToString());
                            message.liked = reactArray.Where(x => x["me"].Value<bool>()).Count() != 0;
                        } else {
                            message.liked = false;
                        }
                        message.site = "Discord";
                        messages.Add(message);

                    }
                } else { // Single Tweet
                    JToken token = res.results;

                    ResponseData.Message message = new ResponseData.Message();
                    message.userId = token["author"]["id"].Value<string>();
                    message.messageId = token["id"].Value<string>();
                    message.message = token["content"].Value<string>();
                    message.name = token["author"]["username"].Value<string>();
                    if (token["reactions"].HasValues) {
                        var reactArray = JArray.Parse(token["reactions"].ToString());
                        message.liked = reactArray.Where(x => x["me"].Value<bool>()).Count() != 0;
                    } else {
                        message.liked = false;
                    }
                    message.site = "Discord";
                    messages.Add(message);

                }
            } else { // Error
                ResponseData.Message message = new ResponseData.Message();
                message.message = res.errorMessage;
                message.name = res.displayMessage + " " + res.status + ":" + res.errorCode;
                message.site = "Discord";
                messages.Add(message);
            }

            return messages;
        }

        private List<ResponseData.Message> RedditToMessage(ServerObject res, 
            ResponseData.Message prevMessage = null, bool getReplies = false) {
            List<ResponseData.Message> messages = new List<ResponseData.Message>();

            if (res.errorCode == 0) {
                if (res.results["data"]["children"].Type == JTokenType.Array) { // Multiple Tweets
                    JArray array = JArray.Parse(res.results["data"]["children"].ToString());

                    for (int i = 0; i < array.Count(); i++) {
                        JToken token = array.ElementAt(i);

                        ResponseData.Message message = new ResponseData.Message();
                        message.userId = token["data"]["author_fullname"].Value<string>();
                        message.messageId = token["data"]["name"].Value<string>();
                        message.message = token["data"]["title"].Value<string>() + ": " + token["data"]["selftext"].Value<string>();
                        message.name = token["data"]["author"].Value<string>() + " (" + token["data"]["subreddit"].Value<string>() + ")";
                        message.liked = token["data"]["likes"].Value<bool>();
                        message.site = "Reddit";
                        messages.Add(message);

                        if (getReplies) {
                            string query = "subreddit=" + token["data"]["subreddit"] + "&article=" + token["data"]["id"];
                            var replyRes = Server.Instance.SendRequest("Reddit", HTTPMethod.Get, query);

                            messages.AddRange(RedditToMessage(replyRes, message, false));
                        }

                    }
                } else { // Single Tweet
                    JToken token = res.results;

                    ResponseData.Message message = new ResponseData.Message();
                    message.userId = token["data"]["author_fullname"].Value<string>();
                    message.messageId = token["data"]["name"].Value<string>();
                    message.message = token["data"]["selftext"].Value<string>();
                    message.name = token["data"]["author"].Value<string>() + " (" + token["data"]["subreddit"].Value<string>() + ")";
                    message.liked = token["data"]["likes"].Value<bool>();
                    message.site = "Reddit";
                    messages.Add(message);

                }
            } else { // Error
                ResponseData.Message message = new ResponseData.Message();
                message.message = res.errorMessage;
                message.name = res.displayMessage + " " + res.status + ":" + res.errorCode;
                message.site = "Reddit";
                messages.Add(message);
            }

            return messages;
        }

        private List<ResponseData.Message> SlackToMessage(ServerObject res) {
            List<ResponseData.Message> messages = new List<ResponseData.Message>();

            if (res.errorCode == 0) {
                if (res.results["messages"].Type == JTokenType.Array) { // Multiple Tweets
                    JArray array = JArray.Parse(res.results["messages"].ToString());

                    for (int i = 0; i < array.Count(); i++) {
                        JToken token = array.ElementAt(i);

                        ResponseData.Message message = new ResponseData.Message();
                        message.userId = token["user"].Value<string>();
                        message.messageId = token["ts"].Value<string>();
                        message.message = token["text"].Value<string>();
                        message.name = "???";
                        //message.liked = token["data"]["likes"].Value<bool>();
                        message.site = "Slack";
                        messages.Add(message);

                    }
                } else { // Single Tweet
                    JToken token = res.results;

                    ResponseData.Message message = new ResponseData.Message();
                    message.userId = token["user"].Value<string>();
                    message.messageId = token["ts"].Value<string>();
                    message.message = token["text"].Value<string>();
                    message.name = "???";
                    //message.liked = token["data"]["likes"].Value<bool>();
                    message.site = "Slack";
                    messages.Add(message);
                    messages.Add(message);

                }
            } else { // Error
                ResponseData.Message message = new ResponseData.Message();
                message.message = res.errorMessage;
                message.name = res.displayMessage + " " + res.status + ":" + res.errorCode;
                message.site = "Slack";
                messages.Add(message);
            }

            return messages;
        }

        public static string CreateReplyQuery(string auth, string text, string file, ResponseData.Message message) {
            string query = "";

            switch (auth) {
                case "Twitter":
                    query = PostingWindow.CreatePostQuery(auth, text, file);
                    query += "&replyTo=" + message.messageId;
                    break;
            }

            return query;
        }
    }
}
