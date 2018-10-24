using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public static class WindowUtility {
        public enum GridLayout {
            Vertical,
            Horizontal
        }
        public delegate void AuthDelegate(string authName);

        public static GridLayout ToggleLayout(ref GridLayout layout) {
            return layout = (layout == GridLayout.Vertical ? GridLayout.Horizontal : GridLayout.Vertical);
        }

        /// <summary>
        /// Displays the content Vertically and performs 'action' between TopSpace and BotSpace
        /// </summary>
        /// <param name="action">Delegate to display custom Graphics</param>
        /// <param name="topSpace">Space on Top Side of 'action', negative for Flexible</param>
        /// <param name="botSpace">Space on Bottom Side of 'action', negative for Flexible</param>
        public static void Vertical(System.Action action, int topSpace = -1, int botSpace = -1) {
            GUILayout.BeginVertical();
            if (topSpace < 0) { GUILayout.FlexibleSpace(); } else { GUILayout.Space(topSpace); }

            action();

            if (botSpace < 0) { GUILayout.FlexibleSpace(); } else { GUILayout.Space(botSpace); }
            GUILayout.EndVertical();
        }
        /// <summary>
        /// Displays the content Vertically and performs 'action' between LeftSpace and RightSpace
        /// </summary>
        /// <param name="action">Delegate to display custom Graphics</param>
        /// <param name="leftSpace">Space on Left Side of 'action', negative for Flexible</param>
        /// <param name="rightSpace">Space on Right Side of 'action', negative for Flexible</param>
        public static void Horizontal(System.Action action, int leftSpace = -1, int rightSpace = -1) {
            GUILayout.BeginHorizontal();
            if (leftSpace < 0) { GUILayout.FlexibleSpace(); } else { GUILayout.Space(leftSpace); }

            action();

            if (rightSpace < 0) { GUILayout.FlexibleSpace(); } else { GUILayout.Space(rightSpace); }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// Displays the content in a Fade Group according to delegate 'action'
        /// </summary>
        /// <param name="fade">Float value to transition smoothly, Recommend AnimBool</param>
        /// <param name="action">Delegate to display custom Graphics</param>
        public static void Fade(float fade, System.Action action) {
            if (EditorGUILayout.BeginFadeGroup(fade)) {
                action();
            }
            EditorGUILayout.EndFadeGroup();
        }

        public static void DisplayGrid(GridLayout layout, AuthDelegate action) {
            var enumerator = WindowData.authTextures.GetEnumerator();
            int length = WindowData.authTextures.Count;
            int width = 5; //

            GUILayout.BeginVertical();
            for (int i = 0; i < length;) {
                if (layout == GridLayout.Horizontal) { GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace(); }

                for (int y = 0; y < width && i < length && enumerator.MoveNext(); y++, i++) {
                    action(enumerator.Current.Key);

                    if (layout == GridLayout.Horizontal) { GUILayout.FlexibleSpace(); }
                }
                if (layout == GridLayout.Horizontal) { GUILayout.EndHorizontal(); }
            }
            GUILayout.EndVertical();
        }

        public static int DisplayToolbar(ref int choice, bool expand = true) {
            choice = GUILayout.Toolbar(choice, WindowData.TOOLBAR_CHOICE, GUILayout.ExpandWidth(expand));
            return choice;
        }
        public static string DisplayAttachment(ref string file) {
            string tempFileForLambdaCapture = file;
            //int leftOff = (file == "" ? 95 : 50);
            //Horizontal(() => {
                //Horizontal(() => {
                    GUIContent content = new GUIContent(WindowData.attachImage, "Upload a Photo, Gif or Video");
                    if (GUILayout.Button(content, GUILayout.Height(25), GUILayout.Width(25))) {
                        tempFileForLambdaCapture = EditorUtility.OpenFilePanelWithFilters("Select an Image, Gif or Video", "", WindowData.ATTACH_FILE_EXTENSIONS);
                    }

                    GUILayout.Space(10);

                    GUILayout.BeginVertical();
                    GUILayout.Space(10);

                    GUILayout.Label(Path.GetFileName(tempFileForLambdaCapture));

                    GUILayout.EndVertical();

                //}, leftOff, 0);
            //});

            return file = tempFileForLambdaCapture; 
        }
        public static GridLayout DisplayGridLayout(ref GridLayout layout) {
            GUIStyle style = GUI.skin.button;
            style.imagePosition = ImagePosition.ImageOnly;
            Texture texture = (layout == GridLayout.Horizontal ? WindowData.horizontalImage : WindowData.verticalImage);
            GUIContent content = new GUIContent(layout.ToString(), texture, "Change Grid Alignment (" + layout.ToString() + ")");
            if (GUILayout.Button(content, style, GUILayout.Width(50), GUILayout.Height(50))) {
                WindowUtility.ToggleLayout(ref layout);
            }
            return layout;
        }
        public static void DisplayAuthenticatorMultiPostToggle(string name, Vector2 size, GUIContent content = null) {
            if(content == null) {
                content = new GUIContent(name, name);
            }

            GUILayoutOption width = null;
            if(size.x != 0) {
                width = GUILayout.Width(size.x);
            }
            GUILayoutOption height = null;
            if (size.y != 0) {
                height = GUILayout.Height(size.y);
            }

            bool prev = WindowData.settingData.multiPosters.Contains(name);
            bool curr;
            if(height != null) {
                if (width != null) {
                    curr = GUILayout.Toggle(prev, content, width, height);
                } else {
                    curr = GUILayout.Toggle(prev, content, height);
                }
            } else if(width != null) {
                curr = GUILayout.Toggle(prev, content, width);
            } else {
                curr = GUILayout.Toggle(prev, content);
            }          

            if (prev != curr) {
                if (curr) {
                    Authenticator auth = Server.Instance.GetAuthenticator(name);
                    if (auth.Authenticated) {
                        WindowData.settingData.multiPosters.Add(name);
                    } else if(WindowData.IMPLEMENTED_AUTHENTICATORS.Contains(name)) {
                        bool authenticate = EditorUtility.DisplayDialog(name, "Please Authenticate " + name + " first",
                            "Authenticate", "Cancel");
                        if (authenticate) {
                            Server.Instance.SendRequest(name, HTTPMethod.Authenticate);
                            if (auth.Authenticated) {
                                WindowData.settingData.multiPosters.Add(name);
                            }
                        }
                    } else {
                        EditorUtility.DisplayDialog(name, "Not Implemented",
                            "Ok");
                    }
                } else {
                    WindowData.settingData.multiPosters.Remove(name);
                }
            }
        }
        public static string DisplayAuthGrid(Vector2 btnSize, int windowSizeX, int offset = 0) {
            string selection = "";

            var enumerator = WindowData.authTextures.GetEnumerator();
            int length = WindowData.authTextures.Count;

            int maxAmo = 0; // How many per Row?
            int currAmo = 0;
            int sum = offset;

            for(int i = 0; i < length; i++) {
                sum += (int)btnSize.x + 5;
                if (sum >= windowSizeX) {
                    sum = offset;
                    currAmo = 0;
                } else {
                    currAmo++;
                    if (currAmo >= maxAmo) { maxAmo = currAmo; }
                }
            }
            maxAmo = Mathf.Max(maxAmo, 1);


            GUILayout.BeginVertical(); 
            for (int i = 0; i < length;) {
                GUILayout.BeginHorizontal(); 

                GUILayout.FlexibleSpace();
                for (int y = 0; y < maxAmo && i < length && enumerator.MoveNext(); y++, i++) {
                    string name = enumerator.Current.Key;

                    GUIStyle style = GUI.skin.button;
                    style.alignment = TextAnchor.MiddleLeft;
                    style.imagePosition = ImagePosition.ImageLeft;

                    GUIContent content = new GUIContent(name, WindowData.authTextures[name], name);
                    if (GUILayout.Button(content, style, GUILayout.Width(btnSize.x), GUILayout.Height(btnSize.y))) {
                        selection = name;
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal(); 
            }
            GUILayout.EndVertical(); 


            // VERTICAL and HORIZONTAL
            //if (layout == GridLayout.Horizontal) { GUILayout.BeginVertical(); }
            //else { GUILayout.BeginHorizontal(); }
            //for (int i = 0; i < length;) {
            //    if (layout == GridLayout.Horizontal) { GUILayout.BeginHorizontal(); }
            //    else { GUILayout.BeginVertical(); }

            //    for (int y = 0; y < maxAmo && i < length && enumerator.MoveNext(); y++, i++) {
            //        string name = enumerator.Current.Key;

            //        GUIStyle style = GUI.skin.button;
            //        style.alignment = TextAnchor.MiddleLeft;
            //        style.imagePosition = ImagePosition.ImageLeft;

            //        GUIContent content = new GUIContent(name, WindowData.authTextures[name], name);
            //        if (GUILayout.Button(content, style, GUILayout.Width(btnSize.x), GUILayout.Height(btnSize.y))) {
            //            selection = name;
            //        }

            //        GUILayout.FlexibleSpace();
            //    }
            //    if (layout == GridLayout.Horizontal) { GUILayout.EndHorizontal(); }
            //    else { GUILayout.EndVertical(); }
            //}
            //if (layout == GridLayout.Horizontal) { GUILayout.EndVertical(); }
            //else { GUILayout.EndHorizontal(); }

            return selection;
        }
        public static void DisplayAuthCustomPost(string name, PostingData data) {
            if (WindowData.IMPLEMENTED_AUTHENTICATORS.Contains(name)) {
                Authenticator auth = Server.Instance.GetAuthenticator(name);
                if (!auth.Authenticated) {
                    // Authenticate Button
                    Horizontal(() => {
                        Vertical(() => {
                            if (GUILayout.Button("Authenticate")) {
                                Server.Instance.SendRequest(name, HTTPMethod.Authenticate);
                            }
                        });
                    });
                } else {
                    // Specific Post 
                    switch (name) {
                        case "Twitter":
                            DisplayTwitterPost(data);
                            break;
                        default:
                            break;
                    }
                }
            }
            // Not Implemented
            else {
                Horizontal(() => {
                    Vertical(() => {
                        GUILayout.Label("Not Implemented");
                    });
                });
            }
        }
        public static void DisplayTwitterPost(PostingData data) {
            // Text Area
            Horizontal(() => {
                data.postingText = GUILayout.TextArea(data.postingText, GUILayout.Height(100));
            }, 30, 30);

            // Attachment
            Horizontal(() => {
                DisplayAttachment(ref data.attachFile);
            }, 50);

            // Post Button
            Horizontal(() => {
                if (GUILayout.Button("Post")) {
                    string query = "status=" + data.postingText;
                    if (data.attachFile != "") {
                        query = "&media=" + data.attachFile;
                    }

                    var res = Server.Instance.SendRequest("Twitter", HTTPMethod.Post, query);
                    if (res.errorCode != 0) {
                        Debug.Log(res);
                    }

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

        private static double lastCall = 0;
        private static string response = "";
        public static void DisplayInitializingScreen() {
            Horizontal(() => {
                Vertical(() => {
                    if(GetEpochTime() >= lastCall + .3) {
                        lastCall = GetEpochTime();
                        response += ".";
                        if(response.Length > 3) {
                            response = "";
                        }
                    }
                    GUILayout.Label("Initializing" + response);
                });
            });
        }
        public static double GetEpochTime() {
            return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}

// I would like for things to be dynamic, like a website... BBut Unity doesn't have great support for that
// Therefore I will, IN THE FUTURE, add methods for that kind of stuff... I think...


// Make DisplayGrid more Generic??? 
// Vertical: 
// Calculate Width based on Y
// Horizontal -> Vertical
// X X
// X X
// X

// Horizontal:
// Calculate Width based on X
// Horizontal ->
// X X X
// X X

// Grid
// X X X
//  X X


// Window Specific stuff could seta global variable beore doing any GUI stuff.