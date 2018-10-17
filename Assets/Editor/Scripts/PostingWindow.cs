using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
            //PostingWindow window = EditorWindow.GetWindowWithRect<PostingWindow>(new Rect(50, 50, 300, 300),false, "Post", true);
            PostingWindow window = EditorWindow.GetWindow<PostingWindow>(false, "Post", true);
            window.Init();
            window.Show();
        }

        private bool isInitialized = false;
        //private float height;
        //private float width;
        //private float widthScale;
        //private float heightScale;

        private string[] toolbarChoice = new string[] { "General", "Specific" };
        private int toolbarSelection = 0;

        private string postingText = "";
        //private float postingHeight = 75;
        //private float postingSpace = 5;

        private Texture attachImage = null;
        private string attachFile = "";
        private int attachFileCharLim = 25;
        private string[] attachFileExtensions = new string[] { "Image Files", "png,jpeg,jpg,tif,bmp", "Gif Files", "gif", "Video Files", "avi,flv,wmv,mov,mp4", "All Files", "png,jpeg,jpg,tif,bmp,gif,avi,flv,wmv,mov,mp4" };
        //private float attachBtnSize = 20;
        //private float attachSpace = 10;
        //private float attachGap = 5;

        private string postResult = "";

        Dictionary<string, Texture> authTextures;
        //GUIContent[] authenticatorContent;

        private void Init() {
            if (isInitialized) { return; }

            //this.minSize = new Vector2(150, 150);
            //this.maxSize = new Vector2(1000, 1000);

            attachImage = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Attachment.png");
            if (attachImage == null) { Debug.Log("Could not find Attachment Image"); }

            Server.SetupGenericAuthenticators();
            Authenticator[] authenticators = Server.GetAuthenticators();
            authTextures = new Dictionary<string, Texture>();
            for (int i = 0; i < authenticators.Length; i++) {
                string name = authenticators[i].Name;
                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/" + name + ".png");

                authTextures.Add(name, texture);
            }

            //authenticatorContent = new GUIContent[authenticators.Length];
            //for (int i = 0; i < authenticators.Length; i++) {
            //    authenticatorContent[i] = new GUIContent();

            //    string name = authenticators[i].Name;
            //    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/" + name + ".png");

            //    authenticatorContent[i].tooltip = name;
            //    authenticatorContent[i].text = name;
            //    if (texture == null) {
            //        Debug.Log("Could not find Assets/Editor/Textures/" + name + ".png");
            //    } else {
            //        authenticatorContent[i].image = texture;
            //    }
            //}

            //Texture twitterImage = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Twitter.png");
            //Texture facebookImage = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Facebook.png");

            //authenticatorContent = new GUIContent[2];
            //authenticatorContent[0] = new GUIContent();
            //authenticatorContent[1] = new GUIContent();

            //authenticatorContent[0].

            //authenticatorContent[0].tooltip = "Twitter";
            //authenticatorContent[0].text = "Twitter";
            //if (twitterImage != null) {
            //    authenticatorContent[0].image = twitterImage;
            //} else {
            //    authenticatorContent[0].text = "Twitter";
            //    Debug.Log("Could not find Twitter Image");
            //}

            //authenticatorContent[1].tooltip = "Facebook";
            //authenticatorContent[1].text = "Facebook";
            //if (facebookImage != null) {
            //    authenticatorContent[1].image = facebookImage;
            //} else {
            //    authenticatorContent[1].text = "Facebook";
            //    Debug.Log("Could not find Facebook Image");
            //}

            isInitialized = true;
        }
        private void Destroy() {
            attachImage = null;


            EditorUtility.UnloadUnusedAssetsImmediate();

            isInitialized = false;
        }

        private void OnGUI() {
            //width = this.position.width;
            //height = this.position.height;
            //widthScale = width / this.minSize.x;
            //heightScale = height / this.minSize.y;
            //Debug.Log(this.minSize + " " + this.maxSize);
            //Debug.Log(width + " (" + widthScale + ") " + height + " (" + heightScale + ")");

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
                    //while (attachFile.Length > attachFileCharLim) {
                    //    int index = attachFile.IndexOf('/');
                    //    if (index > -1) {
                    //        attachFile = attachFile.Substring(index + 1);
                    //    } else { break; }
                    //}
                }

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUILayout.Label(Path.GetFileName(attachFile));

                GUILayout.EndVertical();

                GUILayout.Space(50);
                GUILayout.EndHorizontal();
            }

            // Post Buttn
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Post")) {
                    postResult = Random.Range(0, 2) == 1 ? "Post Successful!" : "Errors Detected";
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
