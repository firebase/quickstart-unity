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

using Firebase;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {
  private const string MyStorageBucket = "gs://YOUR-FIREBASE-BUCKET/";
  private const int kMaxLogSize = 16382;

  public GUISkin fb_GUISkin;
  private Vector2 controlsScrollViewVector = Vector2.zero;
  private string logText = "";
  private Vector2 scrollViewVector = Vector2.zero;
  private bool UIEnabled = true;
  private string firebaseStorageLocation;
  private string fileContents;
  private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

  // When the app starts, check to make sure that we have
  // the required dependencies to use Firebase, and if not,
  // add them if possible.
  void Start() {
    dependencyStatus = FirebaseApp.CheckDependencies();
    if (dependencyStatus != DependencyStatus.Available) {
      FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
        dependencyStatus = FirebaseApp.CheckDependencies();
        if (dependencyStatus != DependencyStatus.Available) {
          // This should never happen if we're only using Firebase Analytics.
          // It does not rely on any external dependencies.
          Debug.LogError(
              "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }
  }

  // Exit if escape (or back, on mobile) is pressed.
  void Update() {
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
    GUILayout.Label(logText);
    GUILayout.EndScrollView();
  }

  IEnumerator UploadToFirebaseStorage() {
    StorageReference reference = FirebaseStorage.DefaultInstance
      .GetReferenceFromUrl(firebaseStorageLocation);
    var task = reference.PutBytesAsync(Encoding.UTF8.GetBytes(fileContents));
    yield return new WaitUntil(() => task.IsCompleted);
    if (task.IsFaulted) {
      DebugLog(task.Exception.ToString());
    } else {
      fileContents = "";
      DebugLog("Finished uploading... Download Url: " + task.Result.DownloadUrl.ToString());
      DebugLog("Press the Download button to download text from Firebase Storage");
    }
  }

  IEnumerator DownloadFromFirebaseStorage() {
    StorageReference reference = FirebaseStorage.DefaultInstance
      .GetReferenceFromUrl(firebaseStorageLocation);
    var task = reference.GetBytesAsync(1024 * 1024);
    yield return new WaitUntil(() => task.IsCompleted);
    if (task.IsFaulted) {
      DebugLog(task.Exception.ToString());
    } else {
      fileContents = Encoding.UTF8.GetString(task.Result);
      DebugLog("Finished downloading...");
      DebugLog("Contents=" + fileContents);
    }
  }

  // Render the buttons and other controls.
  void GUIDisplayControls() {
    if (UIEnabled) {

      controlsScrollViewVector =
          GUILayout.BeginScrollView(controlsScrollViewVector);

      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      GUILayout.Label("Text:", GUILayout.Width(Screen.width * 0.20f));
      if (fileContents == null) {
        fileContents = "Sample text... (type here)";
      }
      fileContents = GUILayout.TextArea(fileContents,
        GUILayout.Height(Screen.height * 0.25f));
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      GUILayout.BeginHorizontal();
      GUILayout.Label("Storage Location:", GUILayout.Width(Screen.width * 0.20f));
      if (firebaseStorageLocation == null) {
        firebaseStorageLocation = MyStorageBucket + "File.txt";;
      }
      firebaseStorageLocation  = GUILayout.TextArea(firebaseStorageLocation,
                                    GUILayout.Height(Screen.height * 0.15f));
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Upload")) {
        StartCoroutine(UploadToFirebaseStorage());
      }

      if (GUILayout.Button("Download")) {
        StartCoroutine(DownloadFromFirebaseStorage());
      }
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();
      GUILayout.EndScrollView();
    }
  }

  // Render the GUI:
  void OnGUI() {
    GUI.skin = fb_GUISkin;
    GUI.skin.textArea.fontSize = GUI.skin.textField.fontSize;
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
      controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.6f, Screen.height);
      logArea = new Rect(Screen.width * 0.6f, 0.0f, Screen.width * 0.6f, Screen.height);
    }

    GUILayout.BeginArea(logArea);
    GUIDisplayLog();
    GUILayout.EndArea();

    GUILayout.BeginArea(controlArea);
    GUIDisplayControls();
    GUILayout.EndArea();
  }
}
