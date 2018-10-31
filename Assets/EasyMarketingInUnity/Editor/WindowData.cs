using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    [InitializeOnLoad]
    public static class WindowData {
        // Constants
        public static string[] TOOLBAR_CHOICE = new string[] { "Multiple", "Single" };
        public static string[] HELP_TOOLBAR_CHOICE = new string[] { "About", "FAQ", "Credits" };
        public static string[] ATTACH_FILE_EXTENSIONS = new string[] { "Image Files", "png,jpeg,jpg,tif,bmp", "Gif Files", "gif", "Video Files", "avi,flv,wmv,mov,mp4", "All Files", "png,jpeg,jpg,tif,bmp,gif,avi,flv,wmv,mov,mp4" };
        public static string[] IMPLEMENTED_AUTHENTICATORS = new string[] { "Twitter" };

        // Specific Window Data
        public static PostingData postingData { get; private set; }
        public static ResponseData responseData { get; private set; }
        public static SettingData settingData { get; private set; }
        public static HelpData helpData { get; private set; }

        // Textures
        public static Texture attachImage { get; private set; }
        public static Texture likeImage { get; private set; }
        public static Texture unlikeImage { get; private set; }
        public static Texture horizontalImage { get; private set; }
        public static Texture verticalImage { get; private set; }
        public static Dictionary<string, Texture> authTextures { get; private set; }

        // Other
        public static bool hasInit { get; private set; }

        static WindowData() {
            if (LoadSettings()) {
                Init();
            }
        }

        public static void Save() {
            EditorUtility.SetDirty(postingData);
            EditorUtility.SetDirty(settingData);
            EditorUtility.SetDirty(responseData);
            EditorUtility.SetDirty(helpData);
        }
        public static void Load() {
            //if (hasInit) { return; }

            // Specific Window Data
            {
                string path = "Assets/EasyMarketingInUnity/Save/";

                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                if (postingData == null) {
                    postingData = AssetDatabase.LoadAssetAtPath<PostingData>(path + "PostingData.asset");
                    if (postingData == null) {
                        postingData = ScriptableObject.CreateInstance<PostingData>();
                        AssetDatabase.CreateAsset(postingData, path + "PostingData.asset");
                    }
                }
                if (responseData == null) {
                    responseData = AssetDatabase.LoadAssetAtPath<ResponseData>(path + "ResponseData.asset");
                    if (responseData == null) {
                        responseData = ScriptableObject.CreateInstance<ResponseData>();
                        AssetDatabase.CreateAsset(responseData, path + "ResponseData.asset");
                    }
                }
                if (settingData == null) {
                    settingData = AssetDatabase.LoadAssetAtPath<SettingData>(path + "SettingData.asset");
                    if (settingData == null) {
                        settingData = ScriptableObject.CreateInstance<SettingData>();
                        AssetDatabase.CreateAsset(settingData, path + "SettingData.asset");

                        settingData.serverLogFile = "Assets\\EasyMarketingInUnity\\Log";
                        settingData.serverSaveFile = "Assets\\EasyMarketingInUnity\\Log";
                    }
                    Server.logFile = settingData.serverLogFile + "\\" + GetSortableDate() + ".log";
                    Server.saveFile = settingData.serverSaveFile + "\\server.dat";

                    Debug.Log(Server.logFile);
                    Debug.Log(Server.saveFile);
                }
                if (helpData == null) {
                    helpData = AssetDatabase.LoadAssetAtPath<HelpData>(path + "HelpData.asset");
                    if (helpData == null) {
                        helpData = ScriptableObject.CreateInstance<HelpData>();
                        AssetDatabase.CreateAsset(helpData, path + "HelpData.asset");
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // Texture
            {
                string path = "Assets/EasyMarketingInUnity/Textures/";

                if (attachImage == null) {
                    attachImage = AssetDatabase.LoadAssetAtPath<Texture>(path + "Attachment.png");
                    if (attachImage == null) { Debug.Log("Could not find Attachment Image"); }
                }
                if (likeImage == null) {
                    likeImage = AssetDatabase.LoadAssetAtPath<Texture>(path + "Like.png");
                    if (likeImage == null) { Debug.Log("Could not find Like Image"); }
                }
                if (unlikeImage == null) {
                    unlikeImage = AssetDatabase.LoadAssetAtPath<Texture>(path + "Unlike.png");
                    if (unlikeImage == null) { Debug.Log("Could not find Unlike Image"); }
                }
                if (verticalImage == null) {
                    verticalImage = AssetDatabase.LoadAssetAtPath<Texture>(path + "Vertical.png");
                    if (verticalImage == null) { Debug.Log("Could not find Verical Image"); }
                }
                if (horizontalImage == null) {
                    horizontalImage = AssetDatabase.LoadAssetAtPath<Texture>(path + "Horizontal.png");
                    if (horizontalImage == null) { Debug.Log("Could not find Horizontal Image"); }
                }

                if (authTextures == null) {
                    Authenticator[] authenticators = Server.Instance.GetAuthenticators();
                    authTextures = new Dictionary<string, Texture>();
                    for (int i = 0; i < authenticators.Length; i++) {
                        string name = authenticators[i].Name;
                        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path + name + ".png");
                        if (texture == null) { Debug.Log("Could not find " + name + " Image"); }

                        authTextures.Add(name, texture);
                    }
                }
            }
        }
        private static bool LoadSettings() {
            // return true if we should initOnStartup

            if (settingData == null) {
                string path = "Assets/EasyMarketingInUnity/Save/";

                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                settingData = AssetDatabase.LoadAssetAtPath<SettingData>(path + "SettingData.asset");
                if (settingData == null) {
                    settingData = ScriptableObject.CreateInstance<SettingData>();
                    AssetDatabase.CreateAsset(settingData, path + "SettingData.asset");

                    settingData.serverLogFile = "Assets\\EasyMarketingInUnity\\Save";
                    settingData.serverSaveFile = "Assets\\EasyMarketingInUnity\\Save";
                }
            }
     
            Server.logFile = settingData.serverLogFile + "\\" + GetSortableDate() + ".log";
            Server.saveFile = settingData.serverSaveFile + "\\server.dat";
            if (settingData.debugMode) {
                Server.onLog += Log;
            }

            return settingData.initOnStartup;        
        }

        public static void Init() {
            if (Server.CheckServer()) { return; }

            LoadSettings();

            Server.directory = Application.dataPath + "\\EasyMarketingInUnity\\Plugins\\";
            Server.exe = "easymarketinginunityexpress-win.exe";

            if (!Server.StartServer(settingData.port, settingData.debugMode)) {
                EditorApplication.delayCall -= Init;
                EditorApplication.delayCall += Init;
            } else {
                EditorApplication.quitting += Shutdown;

                WindowData.Load();
            }
        }
        public static void Restart() {
            Server.directory = Application.dataPath + "\\EasyMarketingInUnity\\Plugins\\";
            Server.exe = "easymarketinginunityexpress-win.exe";

            Server.EndServer();
            Server.StartServer(settingData.port, settingData.debugMode);
        }
        public static void Shutdown() {
            EditorApplication.quitting -= Shutdown;

            WindowData.Save();

            // Resources 
            {
                postingData = null;
                settingData = null;
                responseData = null;
                helpData = null;
                authTextures = null;

                attachImage = null;
                likeImage = null;
                unlikeImage = null;
                horizontalImage = null;
                verticalImage = null;

                // I think if I just set them to null and unload unused assets, that should work.
                Resources.UnloadUnusedAssets();
            }

            Server.Log("SHUTING DOWN UNITY");
            Server.EndServer();
        }

        public static void Log(string message) {
            Debug.Log(message);
        }

        public static string GetSortableDate() {
            string temp = System.DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

            return temp.Substring(0, temp.IndexOf('T'));
        }
    }
}
