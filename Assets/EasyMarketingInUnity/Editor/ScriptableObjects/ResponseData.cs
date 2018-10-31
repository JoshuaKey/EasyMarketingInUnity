using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class ResponseData : ScriptableObject {
    public class Message {
        public string userId;
        public string messageId;

        public string name;
        public string message;
        public bool liked;
        public bool isReply;
    }

    public Vector2 scrollPos;

    public Message replyToMessage = null;
    public string replyText;
    public string replyFile;
    public string replyResult;

    public List<AnimBool> authBools = new List<AnimBool>();
    public Dictionary<string, List<Message>> authResponses = new Dictionary<string, List<Message>>();
    public Dictionary<string, float> authDelay = new Dictionary<string, float>();

    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }

}
