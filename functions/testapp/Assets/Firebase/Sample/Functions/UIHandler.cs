// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Firebase.Sample.Functions {
  using Firebase;
  using Firebase.Extensions;
  using Firebase.Functions;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  // Handler for UI buttons on the scene.  Also performs some
  // necessary setup (initializing the firebase app, etc) on
  // startup.
  public class UIHandler : MonoBehaviour {
    private const int kMaxLogSize = 16382;

    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private string logText = "";
    private Vector2 scrollViewVector = Vector2.zero;
    protected bool UIEnabled = false;
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected FirebaseFunctions functions;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start() {
      FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          Debug.LogError(
            "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }

    protected virtual void InitializeFirebase() {
      functions = FirebaseFunctions.DefaultInstance;
      UIEnabled = true;

      // To use a local emulator, uncomment this line:
      //   functions.UseFunctionsEmulator("http://localhost:5005");
      // Or from an Android emulator:
      //   functions.UseFunctionsEmulator("http://10.0.2.2:5005");
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s) {
      Debug.Log(s);
      logText += s + "\n";

      while (logText.Length > kMaxLogSize) {
        int index = logText.IndexOf("\n");
        logText = logText.Substring(index + 1);
      }

      scrollViewVector.y = int.MaxValue;
    }

    // Render the log output in a scroll view.
    void GUIDisplayLog() {
      scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
      GUI.contentColor = Color.black;
      GUILayout.Label(logText);
      GUILayout.EndScrollView();
    }

    protected void GUIDisplayTests() {
      GUILayout.BeginVertical();
      if (GUILayout.Button("addNumbers")) {
        StartCoroutine(AddNumbers(5, 7));
      }
      GUILayout.EndVertical();
    }

    protected IEnumerator AddNumbers(int firstNumber, int secondNumber) {
      var func = functions.GetHttpsCallable("addNumbers");
      var data = new Dictionary<string, object>();
      data["firstNumber"] = firstNumber;
      data["secondNumber"] = secondNumber;

      var task = func.CallAsync(data).ContinueWithOnMainThread((callTask) => {
        if (callTask.IsFaulted) {
          // The function unexpectedly failed.
          DebugLog("FAILED!");
          DebugLog(String.Format("  Error: {0}", callTask.Exception));
          return;
        }

        // The function succeeded.
        var result = (IDictionary)callTask.Result.Data;
        DebugLog(String.Format("AddNumbers: {0}", result["operationResult"]));
      });
      yield return new WaitUntil(() => task.IsCompleted);
    }

    // Render the buttons and other controls.
    void GUIDisplayControls() {
      if (UIEnabled) {
        controlsScrollViewVector =
          GUILayout.BeginScrollView(controlsScrollViewVector);

        GUIDisplayTests();

        GUILayout.EndScrollView();
      }
    }

    // Render the GUI:
    void OnGUI() {
      GUI.skin = fb_GUISkin;
      if (dependencyStatus != Firebase.DependencyStatus.Available) {
        GUILayout.Label("One or more Firebase dependencies are not present.");
        GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
        return;
      }

      GUI.skin.textArea.fontSize = GUI.skin.textField.fontSize;
      // Reduce the text size on the desktop.
      if (UnityEngine.Application.platform != RuntimePlatform.Android &&
        UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer) {
        GUI.skin.textArea.fontSize /= 4;
        GUI.skin.button.fontSize = GUI.skin.textArea.fontSize;
        GUI.skin.label.fontSize = GUI.skin.textArea.fontSize;
      }
      if (dependencyStatus != Firebase.DependencyStatus.Available) {
        GUILayout.Label("One or more Firebase dependencies are not present.");
        GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
        return;
      }
      Rect logArea, controlArea;

      if (Screen.width < Screen.height) {
        // Portrait mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width, Screen.height * 0.5f);
        logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width, Screen.height * 0.5f);
      } else {
        // Landscape mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.4f, Screen.height);
        logArea = new Rect(Screen.width * 0.4f, 0.0f, Screen.width * 0.6f, Screen.height);
      }

      GUILayout.BeginArea(logArea);
      GUIDisplayLog();
      GUILayout.EndArea();

      GUILayout.BeginArea(controlArea);
      GUIDisplayControls();
      GUILayout.EndArea();
    }
  }
}
