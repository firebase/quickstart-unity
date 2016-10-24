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

  // When the app starts, create a firebase app object,
  // and initialize messaging.
  public void Start() {
    // Setup message event handlers before initializing Firebase.
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

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
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

    GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));

    scrollViewVector = GUILayout.BeginScrollView (scrollViewVector);
    GUILayout.Label(logText);
    GUILayout.EndScrollView();

    GUILayout.EndArea();
  }
}
