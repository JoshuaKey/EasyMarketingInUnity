using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingData : ScriptableObject {

    public List<string> multiPosters = new List<string>();

    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }
}
