using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class ResponseData : ScriptableObject {
    public class Message {
        public string site;
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
    //public Dictionary<string, List<Message>> authResponses = new Dictionary<string, List<Message>>();
    //public Dictionary<string, float> authDelay = new Dictionary<string, float>();

    public List<KeyValuePair<string, List<Message>>> authResponses =
        new List<KeyValuePair<string, List<Message>>>();
    public List<KeyValuePair<string, float>> authDelay = new List<KeyValuePair<string, float>>();

    public static void Replace<T, U>(List<KeyValuePair<T, U>> list, T key, U value) where T : IComparable {
        var originalPair = list.Find(x => x.Key.CompareTo(key) == 0);
        int index = list.IndexOf(originalPair);

        var newPair = new KeyValuePair<T, U>(key, value);
        list[index] = newPair;
    }
    public static U Get<T, U>(List<KeyValuePair<T, U>> list, T key) where T : IComparable {
        return list.Find(x => x.Key.CompareTo(key) == 0).Value;
    }
    public static bool Contains<T, U> (List<KeyValuePair<T, U>> list, T key) where T : IComparable {
        var value = list.Find(x => x.Key.CompareTo(key) == 0);
        return value.Key.CompareTo(default(T)) != 0;
    }

    public void OnEnable() {
        hideFlags = HideFlags.NotEditable;
    }

}
