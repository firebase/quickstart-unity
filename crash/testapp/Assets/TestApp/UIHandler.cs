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

using Firebase;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {

  public Text outputText;
  FirebaseApp app;

  public void DebugLog(string s) {
    print(s);
    outputText.text += s + "\n";
  }

  // When the app starts, create a firebase app object,
  // set the user property and id, and enable analytics.
  void Start() {
    DebugLog("Setting up firebase...");
    app = FirebaseApp.Create();
    DebugLog(String.Format("Created the firebase app: {0}", app.Name));
  }

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
      Application.Quit();
    }
  }

  void OnDestroy() {
    app = null;
  }

  public void CrashLog() {
    // Log a crash log using Crash.log
    DebugLog("Logging a crash message.");
    Firebase.Crash.FirebaseCrash.Log("This message logged via Crash.Log()");
  }

  public void CrashLogcat() {
    // Log a crash log using Crash.logcat
    DebugLog("Logging a crash message with logcat out.");
    Firebase.Crash.FirebaseCrash.Logcat(LogLevel.Debug, "Firebase", "This message logged via Crash.Logcat()");
  }

  public void CrashReport() {
    // Report the logged crash events to Firebase
    DebugLog("Calling Crash.Report()");
    Firebase.Crash.FirebaseCrash.Report("Unity Crash");
  }

  public void CreateUncaughtException() {
    // Trigger an uncaught exception, for testing Firebase Crash
    DebugLog("Triggering an uncaught exception.");
    throw new Exception("Test exception for verifying Firebase Crash");
  }
}
