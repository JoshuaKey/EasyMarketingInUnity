using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EasyMarketingInUnityBasic;
using System.Threading;

namespace EasyMarketingInUnity {
    public enum GridLayout {
        Vertical,
        Horizontal,
        Grid
    }

    public delegate void AuthenticatorDisplay(string authName, Texture authTexture);

    public class PostingWindow : EditorWindow {

        [MenuItem("Window/Easy Marketing in Unity/Post", priority = 1)]
        public static void ShowWindow() {
            PostingWindow window = EditorWindow.GetWindow<PostingWindow>(false, "Post", true);
            window.Init();
            window.Show();

        }

        // Setup
        private bool isInitialized = false;

        // Category
        private string[] toolbarChoice = new string[] { "General", "Specific" };
        private int toolbarSelection = 0;

        // Posting Text
        private string postingText = "";
        private string postResult = "";

        // Atachment
        private Texture attachImage = null;
        private string attachFile = "";
        private string[] attachFileExtensions = new string[] { "Image Files", "png,jpeg,jpg,tif,bmp", "Gif Files", "gif", "Video Files", "avi,flv,wmv,mov,mp4", "All Files", "png,jpeg,jpg,tif,bmp,gif,avi,flv,wmv,mov,mp4" };

        // Other
        Dictionary<string, Texture> authTextures;

        System.Diagnostics.Process process;
        private void Init() {
            if (isInitialized) { return; }

            Server.RESET_SERVER();
            Server.StartServer();

            //ServerBasic.StartServer();

            //string dir = "C:/Users/Flameo326/Documents/IDEs/Unity/Capstone/EasyMarketingInUnityExpress/";
            //string file = "Start.bat";

            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.FileName = "\"" + dir + file + "\"";
            //startInfo.Arguments = 3000 + " ";
            //process = System.Diagnostics.Process.Start(startInfo);


            attachImage = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Attachment.png");
            if (attachImage ==  null) { Debug.Log("Could not find Attachment Image"); }
            //Authenticator[] authenticators = Server.Instance.GetAuthenticators();
            //authTextures = new Dictionary<string, Texture>();
            //for (int i = 0; i < authenticators.Length; i++) {
            //    string name = authenticators[i].Name;
            //    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/" + name + ".png");

            //    authTextures.Add(name, texture);
            //}

            isInitialized = true;
        }
        private void OnDestroy() {
            Debug.Log("Destroy Called");

            isInitialized = false;

            attachImage = null;
            authTextures = null;

            //ServerBasic.EndServer();

            Server.EndServer();
            /*
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/cmd/Shutdown");
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {

                            Debug.Log(reader.ReadToEnd());
                        }
                    }
                }
                if (process.Responding) {
                    //action = "Closing";
                    Debug.Log("Process is Responding");

                    //Thread.Sleep(500); // Wait for Potential Close
                }
                Debug.Log("Closing Main Window");
                process.CloseMainWindow();
                if (!process.HasExited) {
                    Debug.Log("Killing Process");
                    process.Kill();
                    //Thread.Sleep(500); // Wait for Potential Close
                    process.WaitForExit();
                } else {
                    Debug.Log("Process has not exited");
                }

                int ExitCode = process.ExitCode;
                Debug.Log("Exit Code: " + ExitCode);
            }
            */
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        private void OnGUI() {
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarChoice, GUILayout.ExpandWidth(true));

            if (toolbarSelection == 0) {
                DisplayGeneralPosting();
            } else {
                DisplaySpecificPosting();
            }
        }

        private void DisplayGeneralPosting() {
            // Text Area
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);

                postingText = GUILayout.TextArea(postingText, GUILayout.Height(100));

                GUILayout.Space(30);
                GUILayout.EndHorizontal();
            }

            // Attach Button and Label
            {
                GUILayout.BeginHorizontal(GUILayout.Height(25));
                GUILayout.Space(50);

                GUIContent attachBtnContent = new GUIContent(attachImage, "Upload a Photo, Gif or Video");
                if (GUILayout.Button(attachBtnContent, GUILayout.Height(25), GUILayout.Width(25))) {
                    attachFile = EditorUtility.OpenFilePanelWithFilters("Select an Image, Gif or Video", "", attachFileExtensions);
                }

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUILayout.Label(Path.GetFileName(attachFile));

                GUILayout.EndVertical();

                GUILayout.Space(50);
                GUILayout.EndHorizontal();
            }

            // Post Button
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Post")) {
                    //Debug.Log(ServerBasic.SendRequest());

                    if (Server.Instance.GetAuthenticator("Twitter").Authenticated) {
                        ServerObject obj = Server.Instance.SendRequest("Twitter", HTTPMethod.Get);
                        postResult = obj.displayMessage;
                        Debug.Log(obj);
                    } else {
                        ServerObject obj = Server.Instance.SendRequest("Twitter", HTTPMethod.Authenticate);
                        postResult = obj.displayMessage;
                        Debug.Log(obj);
                    }

                    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/cmd/Twitter/Get");
                    //request.Method = "GET";

                    //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    //    using (Stream stream = response.GetResponseStream()) {
                    //        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                    //            postResult = reader.ReadToEnd();
                    //            Debug.Log(postResult);
                    //        }
                    //    }
                    //}
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // Error / Success
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label(postResult);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private Vector2 scrollPos;
        private void DisplaySpecificPosting() {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            AuthenticatorBtnGrid(GridLayout.Horizontal, (name, text) => {
                GUILayout.Button(name, GUILayout.Height(50), GUILayout.Width(100));
            });

            GUILayout.Space(25);
            AuthenticatorBtnGrid(GridLayout.Vertical, (name, text) => {
                GUILayout.Button(name, GUILayout.Height(50), GUILayout.Width(100));
            });

            GUILayout.Space(25);
            AuthenticatorBtnGrid(GridLayout.Grid, (name, text) => {
                GUILayout.Button(name, GUILayout.Height(50), GUILayout.Width(100));
            });

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            AuthenticatorButton("Twitter", authTextures["Twitter"], () => {

            });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }


        private void AuthenticatorBtnGrid(GridLayout layout, AuthenticatorDisplay authLogic) {
            //int currContent = 0;

            var enumerator = authTextures.GetEnumerator();
            int length = authTextures.Count;
            int width = 3; //

            //GUILayout.BeginVertical();
            //while (currContent < length) {
            //    GUILayout.BeginHorizontal();
            //    for (int i = 0; i < width && currContent < length; i++) {
            //        GUILayout.Button(authenticatorContent[currContent++]);
            //    }
            //    GUILayout.EndHorizontal();  
            //}
            //GUILayout.EndVertical();

            //GUILayout.BeginVertical();
            //while (currContent < length) {
            //    if (layout != GridLayout.Vertical) { GUILayout.BeginHorizontal(); }
            //    if (layout == GridLayout.Grid) { GUILayout.FlexibleSpace(); }
            //    for (int i = 0; i < width && currContent < length; i++) {
            //        authLogic(authenticatorContent[0]);
            //        if (layout == GridLayout.Grid) { GUILayout.FlexibleSpace(); }
            //        currContent++;
            //    }
            //    if (layout != GridLayout.Vertical) { GUILayout.EndHorizontal(); }
            //}
            //GUILayout.EndVertical();

            //GUILayout.BeginVertical();
            //while (enumerator.Current != null) {
            //    if (layout != GridLayout.Vertical) { GUILayout.BeginHorizontal(); }
            //    if (layout == GridLayout.Grid) { GUILayout.FlexibleSpace(); }
            //    for (int i = 0; i < width; i++) {
            //        if (!enumerator.MoveNext()) { break; }
            //        var pair = enumerator.Current;

            //        authLogic(pair.Key, pair.Value);

            //        if (layout == GridLayout.Grid) { GUILayout.FlexibleSpace(); }
            //    }
            //    if (layout != GridLayout.Vertical) { GUILayout.EndHorizontal(); }
            //}
            //GUILayout.EndVertical();


            // Horizontal - No Xenter
            // I I I I
            // I I

            // Vertical - No Horizontal, No Center
            // I
            // I
            // I
            // I

            // Grid - All
            // I I I
            // I I I
        }

        private void AuthenticatorButton(string name, Texture texture, System.Action onPress) {
            GUILayout.Label(name);
        }

        // Text Area
        // Link Attachment
        // Post and Message
        // Grid Layout
    }
}
