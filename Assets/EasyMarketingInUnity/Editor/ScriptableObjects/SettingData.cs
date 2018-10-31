using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class SettingData : ScriptableObject {

    // Settings
    public List<string> multiPosters = new List<string>();
    public bool initOnStartup;
    public bool debugMode;
    public int port = 3000;
    public bool performSyncRequests;
    public bool restartOnCrash;

    public bool enableNotifications;
    public bool enableSound;
    public bool enablePopup;

    public string serverSaveFile;
    public string serverLogFile;
    public int responseDelay = 30000; // 30 Seconds
    // -------------

    // General Settings???
    public Vector2 multiScroll;
    public AnimBool advancedBool;

    public AnimBool notifyBool;


    public AnimBool siteBool;
    public string specificSite = "";

    public AnimBool restrictionsBool;

    public Vector2 singleScroll;

    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }
}
