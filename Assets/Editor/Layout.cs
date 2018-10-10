using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMarketingInUnity {
    public static class Layout {

        public delegate void VoidDelegate();

        public static void GUICenter(VoidDelegate method) {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            method();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }
}

