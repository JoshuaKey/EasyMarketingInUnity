using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLLTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        print(EasyMarketingInUnity.Print());
	}

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            print(Application.persistentDataPath);
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/SomeLevel");
        }
    }

}
