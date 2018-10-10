using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class EditorGUILayoutWindow : EditorWindow {

    [MenuItem("Window/Easy Marketing in Unity/Editor GUI Layout Window")]
    public static void ShowWindow() {
        EditorGUILayoutWindow window = EditorWindow.GetWindow<EditorGUILayoutWindow>(true, "Editor GUI Layout Window", true);

        window.Show();
    }

    private void Init() {
        string[] fonts = Font.GetOSInstalledFontNames();
        style1 = new GUIStyle();
        style2 = new GUIStyle();
        style1.font = Font.CreateDynamicFontFromOSFont(fonts[0], 12);
        style2.font = Font.CreateDynamicFontFromOSFont(fonts[1], 12);

        fadeBool = new AnimBool();
        fadeBool.valueChanged.AddListener(Repaint);
    }

    AnimBool fadeBool;
    GUIStyle style1 = null;
    GUIStyle style2;
    Vector2 scrollPos;
    int testInt = 3;
    bool foldout = false;
    Color dropColor = Color.red;
    private void OnGUI() {
        if (style1 == null) { Init(); }

        EditorGUILayout.LabelField("Editor GUI Layout Label Field");
        EditorGUILayout.SelectableLabel("Editor GUI Layout Selectable Label"); // Inserts a space afterwards

        EditorGUILayout.LabelField("Editor GUI Layout Scroll View");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);

        testInt = EditorGUILayout.DelayedIntField("Editor GUI Layout Delayed Int Field", testInt); // Shows the current Result, but does not return it until lost focus or press Enter...
        EditorGUILayout.DelayedFloatField("Editor GUI Layout Delayed Float Field", 3.33f);
        EditorGUILayout.DelayedDoubleField("Editor GUI Layout Delayed Double Field", 3.33);
        EditorGUILayout.DelayedTextField("Editor GUI Layout Delayed Float Field", "hi");

        testInt = EditorGUILayout.IntField("Editor GUI Layout Int Field", testInt);
        //EditorGUILayout.ObjectField()
        EditorGUILayout.LongField("Editor GUI Layout Long Field", 55);
        EditorGUILayout.FloatField("Editor GUI Layout Float Field", 3.33f);
        EditorGUILayout.DoubleField("Editor GUI Layout Double Field", 3.33);
        EditorGUILayout.TextField("Editor GUI Layout Text Field", "hi\nHello");
        EditorGUILayout.TextArea("Editor GUI Layout Text Area\nHello");
        EditorGUILayout.PasswordField("Editor GUI Layout Password Field", "Password");
        EditorGUILayout.LayerField("Editor GUI Layout Layer Field", 1);
        EditorGUILayout.TagField("Editor GUI Layout Tag Field", "Finish");
        EditorGUILayout.MaskField("Editor GUI Layout Mask Field", 1, new string[] { "Mask 1", "Mask 2", "Mask 3" });

        EditorGUILayout.Vector2Field("Editor GUI Layout Vector2 Field", Vector2.one);
        EditorGUILayout.Vector2IntField("Editor GUI Layout Vector2 Int Field", Vector2Int.one);
        EditorGUILayout.Vector3Field("Editor GUI Layout Vector3 Field", Vector3.one);
        EditorGUILayout.Vector3IntField("Editor GUI Layout Vector3 Int Field", Vector3Int.one);
        EditorGUILayout.Vector4Field("Editor GUI Layout Vector4 Field", Vector4.one);

        float min = 0, max = 10;
        EditorGUILayout.Slider("Editor GUI Layout Slider", 1, 0, 10);
        EditorGUILayout.IntSlider("Editor GUI Layout Int Slider", 5, 1, 10);
        EditorGUILayout.MinMaxSlider("Editor GUI Layout Min Max Slider", ref min, ref max, -10, 500);

        ColorSpace cSpace = ColorSpace.Uninitialized;
        EditorGUILayout.EnumFlagsField("Editor GUI Layout Enum Flags Field", cSpace);
        EditorGUILayout.EnumPopup("Editor GUI Layout Enum Popup", cSpace);

        EditorGUILayout.RectField("Editor GUI Layout Rect Field", new Rect(0, 0, 150, 150));
        EditorGUILayout.RectIntField("Editor GUI Layout Rect Int Field", new RectInt(0, 0, 150, 150));

        EditorGUILayout.BoundsField("Editor GUI Layout Bounds Field", new Bounds(Vector3.zero, Vector3.one));
        EditorGUILayout.BoundsIntField("Editor GUI Layout Bounds Int Field", new BoundsInt(Vector3Int.zero, Vector3Int.one));

        dropColor = EditorGUILayout.ColorField("Editor GUI Layout Color Field", dropColor);
        dropColor = EditorGUILayout.ColorField(new GUIContent("Editor GUI Layout Color Field (Advanced)"), dropColor, true, true, true); // Requires GuiContent...

        EditorGUILayout.CurveField("Editor GUI Layout Curve Field", new AnimationCurve(), Color.green,
            new Rect(0, 0, 50, 50));

        EditorGUILayout.HelpBox("Editor GUI Layout Help Box", MessageType.Info, true);
        EditorGUILayout.Popup("Editor GUI Layout Popup", 1, new string[] { "Normal", "Double", "Quadruple" });
        EditorGUILayout.IntPopup("Editor GUI Layout Int Popup", 1, new string[] { "Normal", "Double", "Quadruple" }, new int[] { 1, 2, 4 });

        // Allows displaying a Menu or EditorWindow for Drop down...
        if(EditorGUILayout.DropdownButton(new GUIContent("Editor GUI Layout Dropdown Button"), FocusType.Keyboard)) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("RGB/Red"), dropColor.Equals(Color.red), (x) => {
                dropColor = (Color)x;
            }, Color.red);
            menu.AddItem(new GUIContent("RGB/Blue"), dropColor.Equals(Color.blue), (x) => {
                dropColor = (Color)x;
            }, Color.blue);
            menu.AddSeparator("RGB/");
            menu.AddItem(new GUIContent("RGB/Green"), dropColor.Equals(Color.green), (x) => {
                dropColor = (Color)x;
            }, Color.green);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("White"), dropColor.Equals(Color.white), (x) => {
                dropColor = (Color)x;
            }, Color.white);
            menu.ShowAsContext();

            //GUILayoutWindow window = EditorWindow.GetWindow<GUILayoutWindow>(true, "Twitter Window", true);
            //window.ShowAsDropDown(GUILayoutUtility.GetLastRect(), Vector2.one * 500);
        }
        fadeBool.target = EditorGUILayout.Toggle("Editor GUI Layout Toggle", fadeBool.target);
        fadeBool.target = EditorGUILayout.ToggleLeft("Editor GUI Layout Toggle Left", fadeBool.target);

        EditorGUILayout.LabelField("Editor GUI Layout Knob");
        EditorGUILayout.Knob(Vector2.one, 5, 1, 10, "meter", Color.grey, Color.green, true); // Knob is apparently garbage and pointless

        if(foldout = EditorGUILayout.Foldout(foldout, "Editor GUI Layout Foldout", true)) {
            EditorGUILayout.LabelField("Hello");
        }

        EditorGUILayout.LabelField("Editor GUI Layout Get Control Rect");
        EditorGUILayout.GetControlRect(true); // For use in EditorGUI

        GameObject go = Selection.activeGameObject;
        if(go != null) {
            EditorGUILayout.LabelField("Editor GUI Layout Inspector Title Bar");
            if (foldout = EditorGUILayout.InspectorTitlebar(foldout, go, true)) {
                EditorGUILayout.LabelField("Hello from the other side!!!");
            }
        }

        EditorGUILayout.PrefixLabel("Editor GUI Layout Prefix Label Example", style1, style2);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Testing Prefix Label");
        EditorGUILayout.LabelField("Test");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Editor GUI Layout Fade Group");
        if (EditorGUILayout.BeginFadeGroup(fadeBool.faded)) {
            EditorGUILayout.LabelField("Im Hiding...");
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.LabelField("Editor GUI Layout Horizontal");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Editor GUI Layout Vertical");
        EditorGUILayout.BeginVertical();
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.EndVertical();

        fadeBool.target = EditorGUILayout.BeginToggleGroup("Editor GUI Layout Toggle Group", fadeBool.target);
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.IntField("Test", 3);
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.LabelField("Editor GUI Layout Separator");
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Editor GUI Layout Space");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("END Editor GUI Layout");

        EditorGUILayout.EndScrollView();

        //EditorGUI
        //EditorGUILayoutWindow
        //EditorGUIUtility

        // EditorStyles - Common GUIStyles
    }

    void OnInspectorUpdate() {
        this.Repaint();
    }

}
