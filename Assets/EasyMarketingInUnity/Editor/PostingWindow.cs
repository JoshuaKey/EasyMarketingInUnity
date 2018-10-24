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
            window.Init();
            window.Show();
        }

        private void Init() {
            if (!Server.StartServer()) {
                EditorApplication.delayCall -= Init;
                EditorApplication.delayCall += Init;
            } else {
                EditorApplication.quitting += Shutdown;

                WindowData.Load();
                data = WindowData.postingData;
                settings = WindowData.settingData;
            }
        }
        private void Shutdown() {
            Server.Log("SHUTING DOWN UNITY");

            WindowData.Save();
            WindowData.Shutdown();

            Server.EndServer();
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
            // Multi-Post Toggle Grid
            WindowUtility.Horizontal(() => {
                WindowUtility.Horizontal(() => {
                    data.displayMultiGrid.target = EditorGUILayout.Foldout(data.displayMultiGrid.target,
                    "Current Sites");
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

                        string query = "status=" + data.postingText;
                        if(data.attachFile != "") {
                            query += "&media=" + data.attachFile;
                        }

                        for(int i = 0; i < settings.multiPosters.Count; i++) {
                            string auth = settings.multiPosters[i];

                            var res = Server.Instance.SendRequest(auth, HTTPMethod.Post, query);
                            if(res.errorCode != 0) {
                                Debug.Log(res);
                            }

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

            } 
            else {
                // Multi-Posting Info 
                WindowUtility.Horizontal(() => {
                    WindowUtility.Vertical(() => {

                        GUILayout.Label("Add a Site to the Multi-Posting List");

                    });
                });
            }
        }

        private void DisplaySinglePosting() {
            if (data.specificAuth == "") { 
                data.scrollPos = GUILayout.BeginScrollView(data.scrollPos);

                //WindowUtility.Horizontal(() => {
                //    WindowUtility.DisplayGridLayout(ref data.layout);
                //}, -1, 10);
                WindowUtility.Horizontal(() => {
                    string selection = WindowUtility.DisplayAuthGrid(new Vector2(150, 50), (int)position.width, 20);
                    if(selection != "") {
                        data.specificAuth = selection;

                        data.postResult = "";
                    }
                }, 10, 10);

                GUILayout.EndScrollView();
            } else {
                // Title / Header
                WindowUtility.Horizontal(() => {
                    if (GUILayout.Button("Back")) {
                        data.specificAuth = "";
                        data.postResult = "";
                    }

                    GUILayout.FlexibleSpace();

                    // Sort of Works,
                    WindowUtility.Horizontal(() => {
                        GUILayout.Label(data.specificAuth);
                    }, 85, 0);                   

                    GUILayout.FlexibleSpace();

                    WindowUtility.DisplayAuthenticatorMultiPostToggle(data.specificAuth, new Vector2(0, 0),
                        new GUIContent("Add to Current Sites"));
                }, 35, 35);

                WindowUtility.DisplayAuthCustomPost(data.specificAuth, data);                
            }
        }      
    }
}