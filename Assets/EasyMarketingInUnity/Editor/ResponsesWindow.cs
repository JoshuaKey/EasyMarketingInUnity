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
            window.Init();
            window.Show();
        }

        private void Init() {
            WindowData.Init();
            data = WindowData.responseData;
            settings = WindowData.settingData;

            var authenticators = Server.Instance.GetAuthenticators();
            for (int i = 0; i < authenticators.Length; i++) {
                var auth = authenticators[i];

                data.authDelay[auth.Name] = 0;
                data.authResponses[auth.Name] = null;

                data.authBools.Add(new AnimBool());
            }
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
                        data.replyResult = "Hi";

                        //string query = "status=" + data.replyText;
                        //if (data.replyFile != "") {
                        //    query += "&media=" + data.replyFile;
                        //}

                        //for (int i = 0; i < settings.multiPosters.Count; i++) {
                        //    string auth = settings.multiPosters[i];

                        //    var res = Server.Instance.SendRequest(auth, HTTPMethod.Post, query);
                        //    if (settings.debugMode) {
                        //        Debug.Log(res);
                        //    }

                        //    data.replyResult += auth + ": " + res.displayMessage + "\n";
                        //}
                        //data.replyText = "";
                        //data.replyFile = "";
                    }
                });

                // Error / Success
                WindowUtility.Horizontal(() => {
                    GUILayout.Label(data.replyResult);
                });
            }, true);
        }

        private void DisplaySite(string name, ref AnimBool val) {
            WindowUtility.FadeWithFoldout(ref val, name, () => {
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
            if (!data.authDelay.ContainsKey(name) || EditorApplication.timeSinceStartup > data.authDelay[name]) {
                data.authDelay[name] = (float)EditorApplication.timeSinceStartup + settings.responseDelay / 1000f;
                data.authResponses[name] = GetResponses(name);
            }

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
            List<ResponseData.Message> messages = data.authResponses[name];

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

            WindowUtility.Horizontal(() => {
                GUIContent content;
                if (mess.liked) {
                    content = new GUIContent(WindowData.likeImage, "Unlike this (Not Implemented)");
                } else {
                    content = new GUIContent(WindowData.unlikeImage, "Like this (Not Implemented)");
                }
                if (GUILayout.Button(content, GUILayout.Width(30), GUILayout.Height(30))) {
                    mess.liked = !mess.liked;
                }

                if (GUILayout.Button("Reply (Not Implemented)", GUILayout.ExpandWidth(false), GUILayout.Height(30))) {
                    data.replyToMessage = mess;
                }
            }, 30, 10);
        }
        private void DisplayReplyMessage(ResponseData.Message mess) {
            GUIStyle text = new GUIStyle(GUI.skin.label);
            text.richText = true;
            text.wordWrap = true;

            WindowUtility.Horizontal(() => {
                GUILayout.Label("<b>" + mess.name + "</b>: " + mess.message, text);
            }, 50, 10);
             
            WindowUtility.Horizontal(() => {

                GUIContent content;
                if (mess.liked) {
                    content = new GUIContent(WindowData.likeImage, "Unlike this (Not Implemented)");
                } else {
                    content = new GUIContent(WindowData.unlikeImage, "Like this (Not Implemented)");
                }
                if (GUILayout.Button(content, GUILayout.Width(30), GUILayout.Height(30))) {
                    mess.liked = !mess.liked;
                }
                if (GUILayout.Button("Reply (Not Implemented)", GUILayout.ExpandWidth(false), GUILayout.Height(30))) {
                    data.replyToMessage = mess;
                }
            }, 50, 10);
        }

        private List<ResponseData.Message> GetResponses(string name) {
            List<ResponseData.Message> messages = null;

            var res = Server.Instance.SendRequest(name, HTTPMethod.Get);
            if (settings.debugMode) {
                Debug.Log(res);
            }

            switch (name) {
                case "Twitter":
                    messages = TwitterToMessage(res, false, true);
                    break;
            }

            return messages;
        }
        private List<ResponseData.Message> TwitterToMessage(ServerObject res, 
            bool areReply = false, bool getReplies = false) {
            List<ResponseData.Message> messages = new List<ResponseData.Message>();

            if (res.errorCode == 0) {
                if (res.results.Type == JTokenType.Array) { // Multiple Tweets
                    JArray array = JArray.Parse(res.results.ToString());

                    for (int i = 0; i < array.Count; i++) {
                        var element = array[i];

                        ResponseData.Message message = new ResponseData.Message();
                        message.userId = element["user"]["id_str"].Value<string>();
                        message.messageId = element["id_str"].Value<string>();
                        message.message = element["text"].Value<string>();
                        message.name = element["user"]["name"].Value<string>();
                        message.liked = element["favorited"].Value<bool>();
                        message.isReply = areReply;
                        messages.Add(message);

                        if (getReplies) {
                            string query = "tweet_id="+ message.messageId + "&reply=true";
                            var replyRes = Server.Instance.SendRequest("Twitter", HTTPMethod.Get, query);
                            if (settings.debugMode) {
                                Debug.Log(replyRes);
                            }
                            messages.AddRange(TwitterToMessage(replyRes, true, false));
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
                    message.isReply = areReply;
                    messages.Add(message);

                    if (getReplies) {
                        string query = "tweet_id=" + messages[0].messageId + "&reply=true";
                        var replyRes = Server.Instance.SendRequest("Twitter", HTTPMethod.Get, query);
                        if (settings.debugMode) {
                            Debug.Log(replyRes);
                        }
                        messages.AddRange(TwitterToMessage(replyRes, true, false));
                    }
                }
            } else { // Error
                ResponseData.Message message = new ResponseData.Message();
                message.message = res.errorMessage;
                message.name = res.displayMessage + " " + res.status + ":" + res.errorCode;
                message.isReply = areReply;
                messages.Add(message);
            }

            return messages;
        }
        private ResponseData.Message[] FacebookToMessage(ServerObject res) {  return null; }
    }
}
