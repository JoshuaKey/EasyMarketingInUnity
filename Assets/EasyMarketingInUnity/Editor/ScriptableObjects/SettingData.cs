﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class SettingData : ScriptableObject {

    // Settings
    public List<string> multiPosters = new List<string>();
    public bool initOnStartup;
    public bool debugMode = true;
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

    // Twitter
    public bool twitterShowUserRetweets;
    public bool twitterShowReplies = true;
    public bool twitterShowUserReplies;
    public bool twitterReplyChain;

    // Discord
    public int discordDefaultChannelIndex;
    public string[] discordAllChannelNames;
    public string[] discordAllChannelIDs;

    // Reddit
    public int redditDefaultSubredditIndex;
    public string[] redditAllSubredditNames;

    // Slack
    public int slackDefaultChannelIndex;
    public string[] slackAllChannelNames;
    public string[] slackAllChannelIDs;

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
