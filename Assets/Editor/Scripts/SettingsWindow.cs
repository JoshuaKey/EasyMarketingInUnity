using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class SettingsWindow : EditorWindow {

        [MenuItem("Window/Easy Marketing in Unity/Settings", priority = 3)]
        public static void ShowWindow() {
            SettingsWindow window = EditorWindow.GetWindow<SettingsWindow>(false, "Settings", true);
            window.Show();
        }

    }
}
