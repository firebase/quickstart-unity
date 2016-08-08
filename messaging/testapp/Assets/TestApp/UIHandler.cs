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
  public Text outputText;
  private Firebase.App app;

  // Write text to the Unity and onscreen logs.
  private void DebugLog(string s) {
    print(s);
    outputText.text += s + "\n";
  }

  // Clears the onscreen log.
  private void ClearDebugLog() { outputText.text = ""; }

  // When the app starts, create a firebase app object,
  // and initialize messaging.
  public void Start() {
    DebugLog("Setting up firebase...");
    Firebase.AppOptions ops = new Firebase.AppOptions();
    DebugLog("appID: " + ops.AppID);
    app = Firebase.App.Create(ops);
    DebugLog("Created the firebase app: " + app.Name);

    Firebase.Messaging.Initialize (app, new MessageListener(DebugLog));
    DebugLog("Firebase Messaging initialized!");
  }

  // End our messaging session when the program exits.
  public void OnDestroy() {
    DebugLog("On Destroy");
    Firebase.Messaging.Terminate();
    app.Destroy();
  }
}

internal class MessageListener : Firebase.Messaging.Listener {
  private Action<string> DebugLogFunc;

  public MessageListener(Action<string> debugLogFunc) {
    DebugLogFunc = debugLogFunc;
  }

  private void DebugLog(string text) {
    if (DebugLogFunc != null) {
        DebugLogFunc(text);
      }
  }

  public override void OnMessage (Firebase.Messaging.Message message) {
    DebugLog("Received a new message");
    if (message.from.Length > 0)
      DebugLog("from: " + message.from);
    if (message.data.Count > 0) {
      DebugLog("data:");
      foreach (System.Collections.Generic.KeyValuePair<string, string> iter in message.data) {
        DebugLog("  " + iter.Key + ": " + iter.Value);
      }
    }
  }

  public override void OnTokenReceived (string token) {
    DebugLog("Received Registration Token: " + token);
  }
}
