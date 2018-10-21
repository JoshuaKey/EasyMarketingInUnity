using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMarketingInUnity {
    public static class WindowData {
        public enum WindowType {
            Posting,
            Responses,
            Settings,
            Help
        }

        // Ideas for Saving Data:

        // Scriptable Objects
        // Player Prefab
        // JSON

        // Saves and Loads the entire Class. May not be nessecary if Scriptable Objects
        public static void Save() {}
        public static void Load() {}

        // Saves or Loads the window data
        public static void SaveWindow(WindowType type) { }
        public static void LoadWindow(WindowType type) { }

    }
}

// I can have this potentially manage the server as well?

// public class PostingData { Variables... }
// public PostingData postingData
// public PostingData GetPostingData() // Does this get a reference? I forget...
