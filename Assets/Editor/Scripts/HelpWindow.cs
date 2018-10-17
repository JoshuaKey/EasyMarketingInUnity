using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class HelpWindow : EditorWindow {

        [MenuItem("Window/Easy Marketing in Unity/Help", priority = 4)]
        public static void ShowWindow() {
            HelpWindow window = EditorWindow.GetWindow<HelpWindow>(false, "Help", true);
            window.Show();
        }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}