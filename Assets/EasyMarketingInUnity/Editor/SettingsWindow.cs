using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class SettingsWindow : EditorWindow {

        SettingData data;

        [MenuItem("Window/Easy Marketing in Unity/Settings", priority = 2002)]
        public static void ShowWindow() {
            SettingsWindow window = EditorWindow.GetWindow<SettingsWindow>(false, "Settings", true);
            WindowData.onInit += window.Init;
            WindowData.Init();
            window.Show();
        }

        private void Init() {
            data = WindowData.settingData;

            WindowData.onInit -= this.Init;
        }

        private void OnGUI() {
            if (data == null) {
                WindowUtility.DisplayInitializingScreen();
            } else {
                if (data.specificSite == "") {
                    DisplayGeneral();
                } else {  
                    DisplaySite();
                }             
            }
            Repaint();
        }

        private void DisplayGeneral() {
            GUIStyle richCenter = new GUIStyle(GUI.skin.label);
            richCenter.alignment = TextAnchor.MiddleCenter;
            richCenter.wordWrap = true;
            richCenter.richText = true;

            GUIStyle rich = new GUIStyle(GUI.skin.label);
            rich.richText = true;

            GUIStyle title = new GUIStyle(richCenter);
            title.fontSize = 30;

            WindowUtility.Scroll(ref data.multiScroll, () => {
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("Settings", title);
                });

                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                WindowUtility.FadeWithFoldout(ref data.siteBool, "Sites", () => {
                    GUILayout.Space(10);

                    WindowUtility.Horizontal(() => {
                        string selection = WindowUtility.DisplayAuthGrid(new Vector2(150, 50), (int)position.width, 20);
                        if (selection != "") {
                            data.specificSite = selection;

                        }
                    }, 10, 10);
                });

                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                WindowUtility.FadeWithFoldout(ref data.notifyBool, "Notifications", () => {
                    GUILayout.Space(10);

                    WindowUtility.Horizontal(() => {
                        WindowUtility.Vertical(() => {
                            data.enableNotifications = EditorGUILayout.BeginToggleGroup("Enable Notifications", data.enableNotifications);

                            data.enableSound = GUILayout.Toggle(data.enableSound, "Enable Sound");
                            data.enablePopup = GUILayout.Toggle(data.enablePopup, "Enable Popup");

                            EditorGUILayout.EndToggleGroup();
                        }, 0, 0);

                    }, 30, 10);
                });

                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                WindowUtility.FadeWithFoldout(ref data.advancedBool, "Advanced Settings", () => {
                    GUILayout.Space(10);

                    WindowUtility.Horizontal(() => {
                        if (data.debugMode) {
                            string valid = Server.CheckServer() ? "Valid" : "Invalid";
                            string good = Server.IsResponding() && !Server.HasEnded() ? "Good" : "Bad";
                            GUILayout.Label("<b>Server Status: " + valid + "/" + good + "</b>", rich);
                        } else {
                            string valid = Server.CheckServer() ? "Good" : "Bad";                  
                            GUILayout.Label("<b>Server Status: " + valid + "</b>", rich);
                        }
                        
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        data.initOnStartup = GUILayout.Toggle(data.initOnStartup, "Initialize Server on Startup");
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        bool debug = GUILayout.Toggle(data.debugMode, "Enable Debug Mode");
                        if (debug != data.debugMode) {
                            if (debug) {
                                Server.onLog += WindowData.Log;
                            } else {
                                Server.onLog -= WindowData.Log;
                            }                            

                            data.debugMode = debug;
                        }
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        data.performSyncRequests = GUILayout.Toggle(data.performSyncRequests, "Perform Synchronous Requests");
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        data.restartOnCrash = GUILayout.Toggle(data.restartOnCrash, "Restart Server on Crash");
                    }, 30, 10);

                    WindowUtility.Horizontal(() => {
                        GUILayout.Label("Save File Location");
                    }, 30);
                    WindowUtility.Horizontal(() => {
                        string temp = EditorGUILayout.DelayedTextField(data.serverSaveFile, GUILayout.ExpandWidth(true));
                        // Validation
                        if (temp != data.serverSaveFile) {
                            try {
                                //string directory = System.IO.Path.GetDirectoryName(temp);
                                if (System.IO.Directory.Exists(temp)) {
                                    data.serverSaveFile = temp;
                                    Server.saveFile = temp + "\\server.dat";
                                } else {
                                    ShowNotification(new GUIContent("That is not a valid Path"));
                                }
                            } catch (System.ArgumentException) {
                                ShowNotification(new GUIContent("That is not a valid Path"));
                            } catch (System.IO.PathTooLongException) {
                                ShowNotification(new GUIContent("That is not a valid Path"));
                            }
                        }
                    }, 50, 30);

                    WindowUtility.Horizontal(() => {
                        GUILayout.Label("Log File Location");
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        string temp = EditorGUILayout.DelayedTextField(data.serverLogFile, GUILayout.ExpandWidth(true));
                        // Validation
                        if (temp != data.serverLogFile) {
                            try {
                                //string directory = System.IO.Path.GetDirectoryName(temp);
                                if (System.IO.Directory.Exists(temp)) {
                                    data.serverLogFile = temp;
                                    Server.logFile = temp + "\\" + WindowData.GetSortableDate() + ".log";
                                } else {
                                    ShowNotification(new GUIContent("That is not a valid Path"));
                                }
                            } catch (System.ArgumentException) {
                                ShowNotification(new GUIContent("That is not a valid Path"));
                            } catch (System.IO.PathTooLongException) {
                                ShowNotification(new GUIContent("That is not a valid Path"));
                            }
                        }
                    }, 50, 30);

                    WindowUtility.Horizontal(() => {
                        var content = new GUIContent("Response Delay (MS)", "How long to wait before checking if someone has responded to you.");
                        GUILayout.Label(content, rich);
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        int delay = EditorGUILayout.DelayedIntField(data.responseDelay, GUILayout.ExpandWidth(true));
                        if(delay != data.responseDelay) {
                            bool ok = EditorUtility.DisplayDialog("Response Delay", "Currently Easy Marketing In Unity is unoptimized. I reccomend having it at 30000 (30 seconds) so it doesn't slow down Unity.", "Change", "Cancel");
                            if (ok) {
                                data.responseDelay = delay;
                            }
                        }
                       
                    }, 50, 30);

                    WindowUtility.Horizontal(() => {
                        var content = new GUIContent("Port", "Which port should the Server listen to?");
                        GUILayout.Label(content, rich);
                    }, 30, 10);
                    WindowUtility.Horizontal(() => {
                        int port = EditorGUILayout.DelayedIntField(data.port, GUILayout.ExpandWidth(true));
                        if(port != data.port) {
                            bool ok = EditorUtility.DisplayDialog("Changing Ports", "This requires a restart of the server.", "Restart", "Cancel");
                            if (ok) {
                                data.port = port;
                                WindowData.Restart();
                            }
                        }
                    }, 50, 30);

                    WindowUtility.Horizontal(() => {
                        if (GUILayout.Button("Restart Server", GUILayout.ExpandWidth(false))) {
                            WindowData.Restart();
                        }
                    }, 30, 10);

                    WindowUtility.Horizontal(() => {
                        if (GUILayout.Button("Kill Previous Servers", GUILayout.ExpandWidth(false))) {
                            bool ok = EditorUtility.DisplayDialog("Killing Process", 
                                "WARNING: This will attempt to kill all processes listening at port " + data.port + ".\nThis method should only be used as a last resort.\nIn fact, I would recommend restarting your computer instead as a safer alternative.",
                            "Kill", "Cancel");
                            if (ok) {
                                string success = Server.KillServers(data.port) ? "Successs" : "Failed";
                                ShowNotification(new GUIContent(success));
                                WindowData.Restart();
                            }                        
                        }
                    }, 30, 10);
                });

                GUILayout.Space(10);
                WindowUtility.DrawLine(1);
                
            }, true);
        }
        private void DisplaySite() {
            GUIStyle richCenter = new GUIStyle(GUI.skin.label);
            richCenter.alignment = TextAnchor.MiddleCenter;
            richCenter.wordWrap = true;
            richCenter.richText = true;

            GUIStyle rich = new GUIStyle(GUI.skin.label);
            rich.richText = true;

            GUIStyle title = new GUIStyle(richCenter);
            title.fontSize = 30;

            WindowUtility.Scroll(ref data.singleScroll, () => {
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("Settings", title);
                });
                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                // Header
                WindowUtility.Horizontal(() => {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.fontSize = 15;

                    string name = data.specificSite;
                    GUIContent content = new GUIContent(name, WindowData.authTextures[name], name);
                    GUILayout.Label(content, style, GUILayout.Width(150), GUILayout.Height(30));

                    GUILayout.FlexibleSpace();

                    WindowUtility.Vertical(() => {
                        if (GUILayout.Button("Back")) {
                            data.specificSite = "";
                        }
                    }, 7, 0);

                }, 35, 35);

                switch (data.specificSite) {
                    case "Twitter":
                        DisplayTwitterSettings();
                        break;
                    case "Discord":
                        DisplayFacebookSettings();
                        break;
                    case "Facebook":
                        DisplayFacebookSettings();
                        break;
                    case "Youtube":
                        DisplayYoutubeSettings();
                        break;
                    case "Itch":
                        DisplayItchSettings();
                        break;
                    case "Reddit":
                        DisplayRedditSettings();
                        break;
                    case "Slack":
                        DisplaySlackSettings();
                        break;
                    case "Instagram":
                        DisplayInstagramSettings();
                        break;
                    case "GooglePlus":
                        DisplayGooglePlusSettings();
                        break;
                    case "VKontakte":
                        DisplayVkontakteSettings();
                        break;
                }
            });
        }

        private void DisplayTwitterSettings() {
            WindowUtility.FadeWithFoldout(ref data.restrictionsBool, "Restrictions", () => {
                WindowUtility.Horizontal(() => {
                    GUILayout.Box("Twitter has a maximum of 280 characters.");
                }, 20, 20);

                WindowUtility.Horizontal(() => {
                    GUILayout.Box("Images...");
                }, 20, 20);

                WindowUtility.Horizontal(() => {
                    GUILayout.Box("Videos...");
                }, 20, 20);
            });

            WindowUtility.Horizontal(() => {
                data.twitterShowReplies = GUILayout.Toggle(data.twitterShowReplies, "Show Replies");
            }, 30, 10);

            WindowUtility.Horizontal(() => {
                data.twitterShowUserReplies = GUILayout.Toggle(data.twitterShowUserReplies, "Show User Replies");
            }, 30, 10);

            WindowUtility.Horizontal(() => {
                data.twitterShowUserRetweets = GUILayout.Toggle(data.twitterShowUserRetweets, "Show User Retweets");
            }, 30, 10);

            WindowUtility.Horizontal(() => {
                GUIContent content = new GUIContent("Create Reply chains", "Reply Chains are multiple tweets that reply to the previous Tweet to create a long message.");
                data.twitterReplyChain = GUILayout.Toggle(data.twitterReplyChain, content);
            }, 30, 10);
        }
        private void DisplayDiscordSettings() {

        }
        private void DisplayFacebookSettings() {

        }

        private void DisplayYoutubeSettings() {

        }
        private void DisplayRedditSettings() {

        }
        private void DisplayInstagramSettings() {

        }

        private void DisplayItchSettings() {

        }
        private void DisplaySlackSettings() {

        }
        private void DisplayVkontakteSettings() {

        }
        private void DisplayGooglePlusSettings() {

        }

    }
}
