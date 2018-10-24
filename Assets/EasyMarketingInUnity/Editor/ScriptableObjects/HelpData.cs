using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpData : ScriptableObject {
    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }
}
