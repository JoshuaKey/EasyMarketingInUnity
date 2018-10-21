using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class TwitterWindow : EditorWindow {

        const string openTwitterWindowHotKey = "&5";
        //"Window/Easy Marketing in Unity/Twitter Window " + 

        [MenuItem("Window/Easy Marketing in Unity/Twitter Window " + openTwitterWindowHotKey)]
        public static void ShowWindow() {
            TwitterWindow window = EditorWindow.GetWindow<TwitterWindow>(false, "Twitter Window", true);
            window.Show();
        }

        // SERVER ----------------------------------------------
        private bool m_prevServerStatus = false;
        private bool m_currServerStatus = false;
        private void OnGUI() {
            //Layout.GUICenter(() => {
            //    if(GUILayout.Button("Reset Everything")) {
            //        try{
            //            Server.EndServer();
            //        } catch (System.Net.WebException e) {
            //            Debug.Log(e);
            //        }
            //        m_prevServerStatus = false;
            //        m_currServerStatus = false;
            //        m_tweet = "";
            //        m_response = "";
            //    }
            //});

            //m_currServerStatus = Server.CheckServer();

            //Layout.GUICenter(() => {
            //    GUILayout.Label("Current Server Status: " + (m_currServerStatus ? "Good" : "Bad"));
            //});
            //if (m_currServerStatus) {
            //    TwitterGUI();
            //} else {
            //    if (m_prevServerStatus) {
            //        Server.EndServer();
            //        m_tweet = "";
            //        m_response = "";
            //    }
            //    ServerGUI();
            //}

            //m_prevServerStatus = m_currServerStatus;
        }

        void ServerGUI() {
            //GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //Layout.GUICenter(() => {
            //    if (GUILayout.Button("Start Server")) {
            //        Server.StartServer();
            //    }
            //});
            //GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();
        }

        // TWITTER -------------------------------------------
        private string m_tweet = "";
        private string m_response = "";
        void TwitterGUI() {
            //    Authenticator twitter = Server.Instance.GetAuthenticator("Twitter");
            //    string name = "Twitter";
            //    bool authenticated = twitter.Authenticated;

            //    Layout.GUICenter(() => { GUILayout.Label(name); });
            //    if (authenticated) {
            //        Layout.GUICenter(() => {
            //            m_tweet = GUILayout.TextField(m_tweet);
            //            if (GUILayout.Button("Post Tweet")) {
            //                string query = "status=" + m_tweet;
            //                m_tweet = "";

            //                JObject response = Server.Instance.SendRequest(name, HTTPMethod.Post, query);
            //                JToken errorToken = response.GetValue("error");
            //                JToken resultsToken = response.GetValue("results");
            //                JToken textToken = resultsToken == null ? null : resultsToken.SelectToken("text");

            //                if (textToken != null) {
            //                    m_response = "Successfully Tweeted: " + textToken.CreateReader().ReadAsString();
            //                } else {
            //                    m_response = "Could not get last Tweet\n" + errorToken.CreateReader().ReadAsString();
            //                }
            //            }
            //        });

            //        Layout.GUICenter(() => {
            //            if (GUILayout.Button("Get Last Tweet")) {
            //                JObject response = Server.Instance.SendRequest(name, HTTPMethod.Get);
            //                JToken errorToken = response.GetValue("error");
            //                JToken resultsToken = response.GetValue("results");
            //                JToken textToken = resultsToken == null ? null : resultsToken.SelectToken("text");

            //                if (textToken != null) {
            //                    m_response = "Tweet: " + textToken.CreateReader().ReadAsString();
            //                }  else {
            //                    m_response = "Could not get last Tweet\n" + response;
            //                }
            //            }
            //        });
            //    } else {
            //        Layout.GUICenter(() => {
            //            if (GUILayout.Button("Authenticate")) {
            //                JObject response = Server.Instance.SendRequest(name, HTTPMethod.Authenticate);
            //                JToken errorToken = response.GetValue("error");

            //                if (twitter.Authenticated) {
            //                    m_response = "Twitter Authentication Successful";
            //                } else {
            //                    m_response = "Twitter Authentication Failed\n" + errorToken.CreateReader().ReadAsString();
            //                }
            //            }
            //        });
            //    }

            //    Layout.GUICenter(() => { GUILayout.Label(m_response); });

            //    Layout.GUICenter(() => {
            //        if (GUILayout.Button("End Server")) {
            //            Server.EndServer();
            //        }
            //    });
        }
    }

    // Tracking Key Presses

    //[InitializeOnLoad]
    //public static class EditorHotkeysTracker {
    //    static EditorHotkeysTracker() {
    //        SceneView.onSceneGUIDelegate += view =>
    //        {
    //            var e = Event.current;
    //            if (e != null && e.keyCode != KeyCode.None)
    //                Debug.Log("Key pressed in editor: " + e.keyCode);
    //        };
    //    }
    //}
}

