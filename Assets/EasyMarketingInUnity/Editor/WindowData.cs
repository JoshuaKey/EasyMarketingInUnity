using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public static class WindowData {
        // Constants
        public static string[] TOOLBAR_CHOICE = new string[] { "Multiple", "Single" };
        public static string[] ATTACH_FILE_EXTENSIONS = new string[] { "Image Files", "png,jpeg,jpg,tif,bmp", "Gif Files", "gif", "Video Files", "avi,flv,wmv,mov,mp4", "All Files", "png,jpeg,jpg,tif,bmp,gif,avi,flv,wmv,mov,mp4" };
        public static string[] IMPLEMENTED_AUTHENTICATORS = new string[] { "Twitter" };

        // Specific Window Data
        public static PostingData postingData { get; private set; }
        public static ResponseData responseData { get; private set; }
        public static SettingData settingData { get; private set; }
        public static HelpData helpData { get; private set; }

        // Textures
        public static Texture attachImage { get; private set; }
        public static Texture horizontalImage { get; private set; }
        public static Texture verticalImage { get; private set; }
        public static Dictionary<string, Texture> authTextures { get; private set; }

        // Other
        private static bool hasLoaded = false;

        public static void Save() {
            EditorUtility.SetDirty(postingData);
            EditorUtility.SetDirty(settingData);
            EditorUtility.SetDirty(responseData);
            EditorUtility.SetDirty(helpData);
        }
        public static void Load() {
            if (hasLoaded) { return; }
            // Specific Window Data
            {
                string path = "Assets/EasyMarketingInUnity/ScriptableObjects/";
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
                    }
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

            hasLoaded = true;
        }

        public static void Shutdown() {
            postingData = null;
            settingData = null;
            responseData = null;
            helpData = null;

            attachImage = null;
            horizontalImage = null;
            verticalImage = null;
            authTextures = null;

            // I think if I just set them to null and unload unused assets, that should work.

            Resources.UnloadUnusedAssets();

            hasLoaded = false;
        }
    }
}

// I can have this potentially manage the server as well?

// public class PostingData { Variables... }
// public PostingData postingData
// public PostingData GetPostingData() // Does this get a reference? I forget...
