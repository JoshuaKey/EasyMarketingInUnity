using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseData : ScriptableObject {
    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }
}
