using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyMarketingInUnity {
    public class ResponsesWindow : EditorWindow {

        [MenuItem("Window/Easy Marketing in Unity/Responses", priority = 2001)]
        public static void ShowWindow() {
            ResponsesWindow window = EditorWindow.GetWindow<ResponsesWindow>(false, "Responses", true);
            window.Show();
        }
    }
}
