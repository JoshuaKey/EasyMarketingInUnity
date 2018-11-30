using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.AnimatedValues;

namespace EasyMarketingInUnity {
    public class HelpWindow : EditorWindow {

        HelpData data;

        [MenuItem("Window/Easy Marketing in Unity/Help", priority = 2003)]
        public static void ShowWindow() {
            HelpWindow window = EditorWindow.GetWindow<HelpWindow>(false, "Help", true);
            WindowData.onInit += window.Init;
            WindowData.Init();
            window.Show();
        }

        private void Init() {
            data = WindowData.helpData;

            WindowData.onInit -= this.Init;
        }

        private void OnGUI() {
            if (data == null) {
                WindowUtility.DisplayInitializingScreen();
            } else {
                WindowUtility.Horizontal(() => {
                    WindowUtility.DisplayHelpToolbar(ref data.toolbarChoice);
                }, 30, 30);

                if (data.toolbarChoice == 0) {
                    DisplayInfo();
                } else /*if (data.toolbarChoice == 1)*/ {
                    DisplayFAQ();
                } 
                //else {             
                //    DisplayCredits();
                //}

            }
            Repaint();
        }

        private void DisplayInfo() {
            GUIStyle richCenter = new GUIStyle(GUI.skin.label);
            richCenter.alignment = TextAnchor.MiddleCenter;
            richCenter.wordWrap = true;
            richCenter.richText = true;

            GUIStyle rich = new GUIStyle(richCenter);
            rich.alignment = TextAnchor.UpperLeft;

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

            WindowUtility.Scroll(ref data.aboutScroll, () => {

                WindowUtility.Horizontal(() => {
                    GUILayout.Label("About", title);
                });
                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                // Introduction
                WindowUtility.Horizontal(() => {
                    WindowUtility.Horizontal(() => {
                        GUILayout.Label("Easy Marketing in Unity is a plugin which allows users (you) to post to multiple Social Media Sites from within the Editor.", richCenter);
                    });
                }, 10, 10);
                WindowUtility.Horizontal(() => {
                    WindowUtility.Horizontal(() => {
                        GUILayout.Label("Easy Marketing in Unity is <color=red><b>NOT</b></color> a replacement for these sites.", richCenter);
                    });
                }, 10, 10);
                GUILayout.Space(15);

                // How to Use
                WindowUtility.FadeWithFoldout(ref data.useBool, "How to Use", () => {
                    WindowUtility.Horizontal(() => {
                        string text = "\nEasy Marketing In Unity has 4 windows, <b>Post</b>, <b>Responses</b>, <b>Settings</b> and <b>Help</b>.";
                        text += "\n\n<b>Post</b> allows you to post to Social Media.";
                        text += "\n\n<b>Responses</b> allows you to see responses to your posts.";
                        text += "\n\n<b>Settings</b> allows you to customize your experience.";
                        text += "\n\n<b>Help</b> gives you access to more information.";

                        text += "\n\nTo begin, go to <b>Post</b>-><b>Single</b> and Choose a site.";
                        text += "\n\n<b>Authenticate</b> that site by clicking Authenticate and following instructions. See <b>Help</b>-><b>FAQ</b>-><b>How do I Authenticate a Site</b> for more info.";
                        text += "\n\nAdd some text and make a quick post to check that it's working.";
                        text += "\n\nGo to <b>Responses</b> to check if your previous posts are available.";
                        text += "\n\nAdd more sites or continue posting with Easy Marketing In Unity!";
                        text += "\n\nIf you have any errors, check <b>Help</b>-><b>FAQ</b>.";
                       
                        GUILayout.Label(text, rich);
                    }, 30, 10);
                });

                GUILayout.Space(30);

                // Structure
                WindowUtility.FadeWithFoldout(ref data.structureBool, "Structure", () => {
                    WindowUtility.Horizontal(() => {
                        string text = "\nEasy Marketing In Unity uses 3 components <b>Unity</b>, <b>DLL</b>, and the <b>Server</b>.";
                        text += "\n\n<b>Unity</b> allows for displaying UI and receiving User Interaction.";
                        text += "\n\nThe <b>DLL</b> acts as a medium for interacting with the Server.";
                        text += "\n\nThe <b>Server</b> allows users to perform API calls to each site.";
                        text += "\n\nIf you'd like to see the source code, Go to <b>Help</b>-><b>About</b>-><b>Contact</b>.";
                        text += "\n\nIf you'd like to know more about how each system works, Go to <b>Help</b>-><b>FAQ</b>.";

                        GUILayout.Label(text, rich);
                    }, 30, 10);
                });

                GUILayout.Space(30);

                // Contact
                WindowUtility.FadeWithFoldout(ref data.contactBool, "Contact", () => {
                    WindowUtility.Horizontal(() => {
                        string text = "\nEasy Marketing in Unity is available on Github at: <color=blue><i>https://github.com/JoshuaKey/EasyMarketingInUnity</i></color>";
                        text += "\n\nYou can also contact me personally at: <color=blue><i>KeyJoshJ98@yahoo.com</i></color>";
                        text += "\n\nTo submit a <b>Bug Report</b>, send me an email or create a new issue on Github";
                        text += "\n\n<i>Sorry the text isn't selectable. Unity can be dumb sometimes...</i>";

                        GUILayout.Label(text, rich);

                        // BBug Report Form...
                    }, 30, 10);
                });

                // Thanks
                GUILayout.Space(15);
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("<size=14>Thank you for using Easy Marketing in Unity!</size>", richCenter);
                });
                GUILayout.Space(15);

            }, true);

            foldout.fixedWidth = fixedWidth;
            foldout.fontSize = fontSize;
            foldout.richText = richText;
            foldout.wordWrap = wordWrap;

            //foldout.fixedWidth = 0;
            //foldout.fontSize = 11;
            //foldout.richText = false;
            //foldout.wordWrap = false; 
        }
        private void DisplayCredits() {
            // People and Software...

            GUIStyle richCenter = new GUIStyle(GUI.skin.label);
            richCenter.alignment = TextAnchor.MiddleCenter;
            richCenter.wordWrap = true;
            richCenter.richText = true;

            GUIStyle rich = new GUIStyle(richCenter);
            rich.alignment = TextAnchor.UpperLeft;

            GUIStyle title = new GUIStyle(richCenter);
            title.fontSize = 30;

            WindowUtility.Scroll(ref data.creditsScroll, () => {
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("Credits", title);
                });
                GUILayout.Space(10);
                WindowUtility.DrawLine(1);

                WindowUtility.Horizontal(() => {
                    WindowUtility.Vertical(() => {
                        GUILayout.Label("There are no credits... sort of...", richCenter);
                    });
                });
            }, true);
        }
        private void DisplayFAQ() {
            GUIStyle rich = new GUIStyle(GUI.skin.label);
            rich.wordWrap = true;
            rich.richText = true;

            GUIStyle title = new GUIStyle(rich);
            title.alignment = TextAnchor.MiddleCenter;
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

            // Simplifies making a question...
            // Note: Question can not be too long...
            int i = 0;
            System.Action<string, string> MakeQuestion = (Q, A) => {
                if (data.questionBools.Count >= i) {
                    data.questionBools.Add(new AnimBool());
                }

                AnimBool animBool = data.questionBools[i++];

                WindowUtility.FadeWithFoldout(ref animBool, "Q: " + Q, () => {
                    WindowUtility.Horizontal(() => {
                        GUILayout.Label("\n" + A, rich);
                    }, 30, 10);
                });

                GUILayout.Space(10);
                WindowUtility.DrawLine(1);
            };

            WindowUtility.Scroll(ref data.faqScroll, () => {
                WindowUtility.Horizontal(() => {
                    GUILayout.Label("Frequently Asked Questions", title);
                });
                GUILayout.Space(10);
                WindowUtility.DrawLine(1);


                MakeQuestion("How do you use this plugin?", "See <b>Help</b>-><b>About</b>-><b>How to Use</b>.");

                MakeQuestion("How does the Unity Side work?", "<b>Unity</b> is used to make the plugin visual and interactable."
                    + "\n\nThe Dll and Server are fully functional together, but lack ease of use."
                    + "\n\n<b>Unity</b> does this by using Editor Windows to format information, Scriptable Objects to save user preferences, and Plugins to actually access the DLL."
                    + "\n\n<i>In fact, you <b>could</b> use the DLL and Server to create a different application that does the same thing.</i>");

                MakeQuestion("How does the DLL Side work", "The <b>DLL</b> is written in C# and acts as a medium between Unity and the Server."
                    + "\n\nIt has static methods to Start, Stop or Check the Server. When you start the Server, you create a process that executes and runs the server locally."
                    + "\n\nTo use the <b>DLL</b>, you must send requests. It does this by sending a HttpWebRequest to the Server with the appropriate Site, Route and Query."
                    + "\n\nThe <b>DLL</b> also performs logging and saving so that you don't have to Authenticate the server each time.");

                MakeQuestion("How does the Server Side work?", "The <b>Server</b> is written in Node JS using Express, Passport, and oAuth as the three main modules."
                    + "\n\n<b>Express</b> allows Node to run as a Web Server with routes and webpages, <b>Passport</b> allows Express to perform Authentication Requests and <b>oAuth</b> allows us to make API calls to create or get posts."
                    + "\n\nIt also uses OPN for opening a Web Browser, EJS for displaying Webpages, Cookie-Session for storing sessions and cookies, and Winston for logging.");

                // Support
                {
                    string supportText = "Easy Marketing in Unity currently has plans to support:";
                    for (int y = 0; y < Server.Instance.GetAuthenticators().Length; y++) {
                        string name = Server.Instance.GetAuthenticators()[y].Name;
                        string color = WindowData.IMPLEMENTED_AUTHENTICATORS.Contains(name) ? "green" : "red";
                        supportText += "\n     -  <color=" + color + ">" + name + "</color>";
                    }
                    supportText += "\n<color=green><b>Green</b></color> means it has been Implemented.";
                    supportText += "\n<color=red><b>Red</b></color> means it has not.";
                    MakeQuestion("What sites does Easy Markeing in Unity support?", supportText);
                }

                MakeQuestion("How do I fix Errors?",
                      "1.) Check and make sure your problem hasn't already been addressed in FAQ. See <b>Help</b>-><b>FAQ</b>."
                    + "\n\n2.) If you still can't figure out the problem. Go to <b>Settings</b>-><b>Advanced Settings</b>-><b>Enable Debugging</b>. This will send Debug messages to the console whenever the DLL interacts with the Server. Look at <b>errorMessage</b> for what went wrong."
                    + "\n\n3.) If the problem still hasnt been solved. Try contacting us for help. See <b>Help</b>-><b>About</b>-><b>Contact</b>."
                    + "\n\n<size=13>Some potential alternatives:</size> "
                    + "\n\nRestart Unity."
                    + "\n\nLook at Server.Log in EasyMarketingInUnity/Misc."
                    + "\n\nDelete Server.Dat in EasyMarketingInUnity/Misc. (Requires Reauthentication)"
                    + "\n\nDelete Delete all files in EasyMarketingInUnity/ScriptableObjects. (Loses all Settings)"
                    + "\n\nDelete and Reinstall Easy Marketing in Unity. (Complete Reset)");

                MakeQuestion("How do I Authenticate a site?",
                    "1.) Go to <b>Settings</b> or <b>Post</b> and select which site to Authenticate."
                    + "\n\n2.) There should be a button labeled 'Authenticate'. Click on it. If this button is not there then the site is not implemented, or already Authenticated."
                    + "\n\n3.) A browser window will popup and redirect to the Authenticaton Page. This page lists your profile as well as all the data Easy Marketing In Unity will have access to (Read and Write)."
                    + "\n\n3.b) If your profile does not show up, you will be asked to log in."
                    + "\n\n4.) Click 'OK' if you to agree to these terms."
                    + "\n\n5.) You will redirected to a plain looking site. After this you may return to Unity.");

                MakeQuestion("How does Authentication work?", "When you authenticate a site, you are saying, 'Let this application (Easy Marketing in Unity) access my information. Each Application may have access to different information."
                    + "\n\nFor example, Easy Marketing in Unity only requires <b>Read and Write</b> access for the most part. We do not need to know anything about your identity, location, history, etc.");

                MakeQuestion("I'm getting 'Error'.", "This means that the request you sent was invalid."
                    + "\n\nMake sure to check on the rules of posting. Some sites have <b>restrictions</b>."
                    + "\n\nFor Example, Twitter has a 280 maximum character limit. <i>I</i> haven't implemented video yet either.");

                MakeQuestion("I'm getting 'Something went wrong!'.", "This means that something went wrong in the Server or the DLL. Typically an <b>Exception</b>."
                    + "\n\nSee <b>Help</b>-><b>FAQ</b>-><b>How do I fix Errors?</b>");

                MakeQuestion("I'm getting 'Please authenticate the site first'.", "This means that you sent a request before the site was Authenticated. You should <b>not</b> get this message."
                    + "\n\nIf you did, please submit a bug report detailing what happened."
                    + "\n\nTo fix this you can delete your EasyMarketingInUnity/Misc/Server.dat file. (Requires Reauthentication)");

                MakeQuestion("I'm getting a different error message.", "If this happens, see <b>Help</b>-><b>FAQ</b>-><b>How do I fix Errors?</b> and submit a bug report.");
            }, true);

            foldout.fixedWidth = fixedWidth;
            foldout.fontSize = fontSize;
            foldout.richText = richText;
            foldout.wordWrap = wordWrap;
        }

    }
}