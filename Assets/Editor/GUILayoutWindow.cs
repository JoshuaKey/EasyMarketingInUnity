using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUILayoutWindow : EditorWindow {

    [MenuItem("Window/Easy Marketing in Unity/GUI Layout Window")]
    public static void ShowWindow() {
        // Utility - Wether it should be floating or not
        // Title
        // Focus
        // params System.Type[] DesiredDockNextTo - Which Window to dock next tos, Only use Custom Classes
        //EditorWindow.GetWindowWithRect - Adds Rect for Size and Positioning...

        GUILayoutWindow window = EditorWindow.GetWindow<GUILayoutWindow>(true, "GUI Layout Window", true);

        window.Show(); 
        //window.ShowAsDropDown(); // Similar to Popup
        //window.ShowAuxWindow(); // Displays in the Auxiliary Window
        //window.ShowNotification(); // Shows and disappear after a while
        //window.ShowPopup(); // No Frame, Not draggable, Use PopupWindow for Proper Functionality
        //window.ShowTab(); // Shows the window. Kind of pointless.
        //window.ShowUtility(); // Displays as a floating windows
    }

    Rect windowRect = new Rect(50, 50, 200, 200);
    Vector2 scrollPos = Vector2.zero;

    private void OnGUI() {
        // GUI Layout -----------------------------------------------------------
        //GUILayout.BeginArea(new Rect(0, 0, 50, 50), "GUI Layout BeginArea / Area"); // Rect is Absolute...
        //GUILayout.Button("Test");
        //GUILayout.Button("Test");
        //GUILayout.EndArea();

        GUILayout.Space(60);

        GUILayout.Label("Label: GUI Layout Label"); // Can be given Text or Texture
        GUILayout.Button("GUI Layout Button"); // Can be given Text or Texture

        GUILayout.Label("GUI Layout ScrollView");
        scrollPos = GUILayout.BeginScrollView(scrollPos, true, true, GUILayout.MaxHeight(500));

        GUILayout.Label("GUI Layout Horizontal Scrollbar");
        GUILayout.HorizontalScrollbar(0f, 50, 0, 0);

        GUILayout.Label("GUI Layout Horizontal Slider");
        GUILayout.HorizontalSlider(0, 0, 0);

        GUILayout.Label("GUI Layout Vertical Scrollbar");
        GUILayout.VerticalScrollbar(0f, 50, 0, 0);

        GUILayout.Label("GUI Layout Vertical Slider");
        GUILayout.VerticalSlider(0, 0, 0);

        GUILayout.Label("GUI Layout BeginHorizontal / Horizontal");
        GUILayout.BeginHorizontal();
        GUILayout.Button("Test");
        GUILayout.Button("Test");
        GUILayout.EndHorizontal();

        GUILayout.Label("GUI Layout BeginVertical / Vertical");
        GUILayout.BeginVertical();
        GUILayout.Button("Test");
        GUILayout.Button("Test");
        GUILayout.EndVertical();

        GUILayout.Box("GUI Layout Box");

        GUILayout.Label("GUI Layout Password");
        GUILayout.PasswordField("Characters", '*');

        GUILayout.RepeatButton("GUI Layout Repeating button");

        GUILayout.Label("GUI Layout Selection Grid");
        GUILayout.SelectionGrid(0, new string[] { "Hello", "Bye", "This is a Test", " How Many?", "NOOO!" }, 2);

        GUILayout.TextArea("GUI Layout Text Area\nHello");
        GUILayout.TextField("GUI Layout Text Field\nHello"); // Says single-line, is obviosuly bullcrap.
        GUILayout.Toggle(true, "GUI Layout Toggle");

        GUILayout.Label("GUI Layout Toolbar");
        GUILayout.Toolbar(0, new string[] { "Hello", "Bye" });

        // Displays a 'sub' window. 
        // In a normal Script, this will act as a normal Window
        // In an Editor Window, it functions more like a popup. Also requires BeginWindows() and EndWindows()
        BeginWindows();
        windowRect = GUILayout.Window(0, windowRect, DrawGUILayoutWindow, "GUI Layout Window");
        EndWindows();

        GUILayout.EndScrollView();

        GUILayout.Label("GUI Layout Space");
        GUILayout.Space(10);

        GUILayout.Label("GUI Layout Flexible Space");
        GUILayout.FlexibleSpace();

        GUILayout.Label("GUI END");
        // --------------------------------------------------------

        //GUI
        //GUILayoutWindow
        //GUILayoutUtility

        // OTHER::::::
        // Property Drawer???
    }

    void DrawGUILayoutWindow(int windowID) {
        GUILayout.Button("Custom GUI Layout Button in Window");
        GUI.DragWindow(); // Allows the Window to be dragged
    }

}
