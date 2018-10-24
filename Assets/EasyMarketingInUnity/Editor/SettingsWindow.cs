using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class SettingsWindow : EditorWindow {

        [MenuItem("Window/Easy Marketing in Unity/Settings", priority = 2002)]
        //[MenuItem("Window/Easy Marketing in Unity/Settings", priority = 9903)]
        public static void ShowWindow() {
            SettingsWindow window = EditorWindow.GetWindow<SettingsWindow>(false, "Settings", true);
            window.Show();
        }

    }
}
