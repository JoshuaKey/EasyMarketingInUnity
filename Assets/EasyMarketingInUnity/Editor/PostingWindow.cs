using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace EasyMarketingInUnity {
    public class PostingWindow : EditorWindow {

        PostingData data;
        SettingData settings;

        [MenuItem("Window/Easy Marketing in Unity/Post", priority = 2000)]
        public static void ShowWindow() {
            PostingWindow window = EditorWindow.GetWindow<PostingWindow>(false, "Post", true);
            WindowData.onInit += window.Init;
            WindowData.Init();
            window.Show();
        }

        private void Init() {
            data = WindowData.postingData;
            settings = WindowData.settingData;

            WindowData.onInit -= this.Init;
        }

        private void OnGUI() {
            if (data == null) {
                WindowUtility.DisplayInitializingScreen();
            } else {
                WindowUtility.Horizontal(() => {
                    WindowUtility.DisplayToolbar(ref data.postChoice);
                }, 30, 30);           

                if (data.postChoice == 0) {
                    // Reset Results
                    if(data.specificAuth != "") {
                        data.postResult = "";
                    }

                    DisplayMultiPosting();
                } else {
                    DisplaySinglePosting();
                }
            }
            Repaint();
        }
        private void DisplayMultiPosting() {
            WindowUtility.Scroll(ref data.multiScrollPos, () => {

                // Multi-Post Toggle Grid
                WindowUtility.Horizontal(() => {
                    WindowUtility.Horizontal(() => {
                        bool target = data.displayMultiGrid.target;
                        WindowUtility.Foldout(ref target, "Current Sites");
                        data.displayMultiGrid.target = target;
                    });
                }, 0, 30);
                WindowUtility.Fade(data.displayMultiGrid.faded, () => {
                    WindowUtility.Horizontal(() => {
                        WindowUtility.DisplayGrid(WindowUtility.GridLayout.Horizontal, (name) => {

                            GUIContent content = new GUIContent(WindowData.authTextures[name], name);
                            WindowUtility.DisplayAuthenticatorMultiPostToggle(name, new Vector2(75, 50), content);

                        });
                    }, 10, 10);
                });

                if (settings.multiPosters.Count != 0) {
                    // Text Area
                    WindowUtility.Horizontal(() => {
                        data.postingText = GUILayout.TextArea(data.postingText, GUILayout.Height(100));
                    }, 30, 30);

                    // Attachment
                    WindowUtility.Horizontal(() => {
                        WindowUtility.DisplayAttachment(ref data.attachFile);
                    }, 50, 50);

                    // Post Button
                    WindowUtility.Horizontal(() => {
                        if (GUILayout.Button("Post")) {
                            data.postResult = "";      

                            for (int i = 0; i < settings.multiPosters.Count; i++) {
                                string auth = settings.multiPosters[i];

                                string query = CreatePostQuery(auth, data.postingText, data.attachFile);

                                var res = Server.Instance.SendRequest(auth, HTTPMethod.Post, query);

                                data.postResult += auth + ": " + res.displayMessage + "\n";
                            }
                            data.postingText = "";
                            data.attachFile = "";
                        }
                    });

                    // Error / Success
                    WindowUtility.Horizontal(() => {
                        GUILayout.Label(data.postResult);
                    });

                } else {
                    // Multi-Posting Info 
                    WindowUtility.Horizontal(() => {
                        WindowUtility.Vertical(() => {

                            GUILayout.Label("Add a Site to the Multi-Posting List");

                        });
                    });
                }
            }, true);
        }
        private void DisplaySinglePosting() {
            if (data.specificAuth == "") {
                WindowUtility.Scroll(ref data.gridScrollPos, () => {
                    WindowUtility.Horizontal(() => {
                        string selection = WindowUtility.DisplayAuthGrid(new Vector2(150, 50), (int)position.width, 20);
                        if (selection != "") {
                            data.specificAuth = selection;

                            data.postResult = "";
                            data.authScrollPos = Vector2.zero;
                        }
                    }, 10, 10);
                }, true);
            } else {
                WindowUtility.Scroll(ref data.authScrollPos, () => {
                    WindowUtility.Horizontal(() => {
                        GUIStyle style = new GUIStyle(GUI.skin.label);
                        style.fontSize = 15;

                        Texture texture = WindowData.authTextures[data.specificAuth];
                        GUIContent content = new GUIContent(data.specificAuth, texture, data.specificAuth);
                        GUILayout.Label(content, style, GUILayout.Width(150), GUILayout.Height(30));

                        GUILayout.FlexibleSpace();

                        WindowUtility.Vertical(() => {
                            if (GUILayout.Button("Back")) {
                                data.specificAuth = "";
                                data.postResult = "";
                            }
                        }, 7, 0);

                    }, 35, 35);

                    DisplayAuthCustomPost(data.specificAuth);
                }, true);
            }
        }

        public void DisplayAuthCustomPost(string name) {
            if (WindowData.IMPLEMENTED_AUTHENTICATORS.Contains(name)) {
                Authenticator auth = Server.Instance.GetAuthenticator(name);
                if (!auth.Authenticated) {
                    // Authenticate Button
                    WindowUtility.Horizontal(() => {
                        WindowUtility.Vertical(() => {
                            if (GUILayout.Button("Authenticate")) {
                                Server.Instance.SendRequest(name, HTTPMethod.Authenticate);
                            }
                        });
                    });
                } else {
                    // Specific Post 
                    switch (name) {
                        case "Twitter":
                            DisplayTwitterPost();
                            break;
                        default:
                            break;
                    }
                }
            }
            // Not Implemented
            else {
                WindowUtility.Horizontal(() => {
                    WindowUtility.Vertical(() => {
                        GUILayout.Label("Not Implemented");
                    });
                });
            }
        }
        public void DisplayTwitterPost() {
            // Text Area
            WindowUtility.Horizontal(() => {
                data.postingText = GUILayout.TextArea(data.postingText, GUILayout.Height(100));
            }, 30, 30);

            // Attachment
            WindowUtility.Horizontal(() => {
                WindowUtility.DisplayAttachment(ref data.attachFile);
            }, 50);

            // Post Button
            WindowUtility.Horizontal(() => {
                if (GUILayout.Button("Post")) {
                    string query = CreatePostQuery(data.specificAuth, data.postingText, data.attachFile);

                    var res = Server.Instance.SendRequest("Twitter", HTTPMethod.Post, query);

                    data.postResult = res.displayMessage + "\n";

                    data.postingText = "";
                    data.attachFile = "";
                }
            });

            // Error / Success
            WindowUtility.Horizontal(() => {
                GUILayout.Label(data.postResult);
            });

        }

        public static string CreatePostQuery(string auth, string text, string file) {
            string query = "";

            switch (auth) {
                case "Twitter":
                    query = "status=" + text;
                    if (file != "") {
                        query += "&media=" + file;
                    }
                    if (WindowData.settingData.twitterReplyChain) {
                        query += "&multiple=true";
                    }
                    break;
            }

            return query;
        }
    }
}