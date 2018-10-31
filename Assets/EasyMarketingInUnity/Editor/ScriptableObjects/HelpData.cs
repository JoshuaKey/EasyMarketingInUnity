using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class HelpData : ScriptableObject {

    public int toolbarChoice;

    // ABOUT
    public Vector2 aboutScroll;
    public AnimBool useBool;
    public AnimBool structureBool;
    public AnimBool contactBool;

    // FAQ
    public Vector2 faqScroll;
    public List<AnimBool> questionBools = new List<AnimBool>();

    // CREDITS
    public Vector2 creditsScroll;

    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }

}
