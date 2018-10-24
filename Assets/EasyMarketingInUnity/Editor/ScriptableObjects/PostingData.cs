using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EasyMarketingInUnity;
using UnityEditor.AnimatedValues;

public class PostingData : ScriptableObject {

    public int postChoice = 0;
    public string postingText = "";
    public string postResult = "";
    public string attachFile = "";

    public AnimBool displayMultiGrid;

    public WindowUtility.GridLayout layout;

    public string specificAuth = "";
    public Vector2 scrollPos;


    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
        
    }
}

