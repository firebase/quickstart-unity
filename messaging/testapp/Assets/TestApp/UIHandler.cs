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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public
class UIHandler : MonoBehaviour {
  public GUISkin fb_GUISkin;
  private Vector2 scrollViewVector = Vector2.zero;
  private string logText = "";
  const int kMaxLogSize = 16382;
  Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

  // When the app starts, check to make sure that we have
  // the required dependencies to use Firebase, and if not,
  // add them if possible.
  void Start() {
    dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
    if (dependencyStatus != Firebase.DependencyStatus.Available) {
      Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
        dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          // This should never happen if we're only using Firebase Analytics.
          // It does not rely on any external dependencies.
          Debug.LogError(
              "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    } else {
      InitializeFirebase();
    }
  }

  // Setup message event handlers.
  void InitializeFirebase() {
    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
    DebugLog("Firebase Messaging Initialized");
  }

  public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
    DebugLog("Received a new message");
    if (e.Message.From.Length > 0)
      DebugLog("from: " + e.Message.From);
    if (e.Message.Data.Count > 0) {
      DebugLog("data:");
      foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
               e.Message.Data) {
        DebugLog("  " + iter.Key + ": " + iter.Value);
      }
    }
  }

  public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
    DebugLog("Received Registration Token: " + token.Token);
  }

  // Exit if escape (or back, on mobile) is pressed.
  void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
  }

  // End our messaging session when the program exits.
  public void OnDestroy() {
    Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
    Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
  }

  // Output text to the debug log text field, as well as the console.
  public void DebugLog(string s) {
    print(s);
    logText += s + "\n";

    while (logText.Length > kMaxLogSize) {
      int index = logText.IndexOf("\n");
      logText = logText.Substring(index + 1);
    }

    scrollViewVector.y = int.MaxValue;
  }

  // Render the log output in a scroll view.
  void GUIDisplayLog() {
    scrollViewVector = GUILayout.BeginScrollView (scrollViewVector);
    GUILayout.Label(logText);
    GUILayout.EndScrollView();
  }

  // Render the GUI:
  void OnGUI() {
    GUI.skin = fb_GUISkin;
    if (dependencyStatus != Firebase.DependencyStatus.Available) {
      GUILayout.Label("One or more Firebase dependencies are not present.");
      GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
      return;
    }

    GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));

    scrollViewVector = GUILayout.BeginScrollView (scrollViewVector);
    GUILayout.Label(logText);
    GUILayout.EndScrollView();

    GUILayout.EndArea();
  }
}
