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
using System.Threading.Tasks;
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
  // initialize remote config, and set the default values.
  public void Start() {
    DebugLog("Setting up firebase...");
    Firebase.AppOptions ops = new Firebase.AppOptions();
    DebugLog("appID: " + ops.AppID);
    app = Firebase.App.Create(ops);
    DebugLog("Created the firebase app: " + app.Name);

    Firebase.RemoteConfig.Initialize(app);
    System.Collections.Generic.Dictionary<string, string> defaults =
      new System.Collections.Generic.Dictionary<string, string>();

    // These are the values that are used if we haven't fetched data from the
    // server
    // yet, or if we ask for values that the server doesn't have:
    defaults.Add("config_test_string", "default local string");
    defaults.Add("config_test_int", "1");
    defaults.Add("config_test_float", "1.0");
    defaults.Add("config_test_bool", "False");

    Firebase.RemoteConfig.SetDefaults(defaults);
    DebugLog("RemoteConfig configured and ready!");
  }

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
      Application.Quit();
    }
  }

  // End our remote config session when the program exits.
  public void OnDestroy() {
    Firebase.RemoteConfig.Terminate();
    app.Destroy();
  }

  // Display the currently loaded data.  If fetch has been called, this will be
  // the data fetched from the server.  Otherwise, it will be the defaults.
  // Note:  Firebase will cache this between sessions, so even if you haven't
  // called fetch yet, if it was called on a previous run of the program, you
  //  will still have data from the last time it was run.
  public void DisplayData() {
    ClearDebugLog();
    DebugLog("Current Data:");
    DebugLog("config_test_string: " +
             Firebase.RemoteConfig.GetString("config_test_string"));
    DebugLog("config_test_int: " + Firebase.RemoteConfig.GetLong("config_test_int"));
    DebugLog("config_test_float: " + Firebase.RemoteConfig.GetDouble("config_test_float"));
    DebugLog("config_test_bool: " + Firebase.RemoteConfig.GetBoolean("config_test_bool"));
  }

  public void DisplayAllKeys() {
    ClearDebugLog ();
    DebugLog("Current Keys:");
    System.Collections.Generic.IEnumerable<string> keys =
        Firebase.RemoteConfig.GetKeys();
    foreach (string key in keys) {
      DebugLog("    " + key);
    }
    DebugLog("GetKeysByPrefix(\"config_test_s\"):");
    keys = Firebase.RemoteConfig.GetKeysByPrefix("config_test_s");
    foreach (string key in keys) {
      DebugLog("    " + key);
    }
  }

  // Start a fetch request.
  public void FetchData() {
    ClearDebugLog();
    DebugLog("Fetching data...");
    System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.Fetch();
    fetchTask.ContinueWith(FetchComplete);
  }

  void FetchComplete(Task fetchTask) {
    if (fetchTask.IsCanceled) {
      DebugLog("Fetch canceled.");
    } else if (fetchTask.IsFaulted) {
      DebugLog("Fetch encountered an error.");
    } else if (fetchTask.IsCompleted) {
      DebugLog("Fetch completed successfully!");
    }

    switch (Firebase.RemoteConfig.GetInfo().LastFetchStatus) {
    case Firebase.RemoteConfig.LastFetchStatus.Success:
      Firebase.RemoteConfig.ActivateFetched();
      DebugLog("Remote data loaded and ready.");
      break;
    case Firebase.RemoteConfig.LastFetchStatus.Failure:
      switch (Firebase.RemoteConfig.GetInfo().LastFetchFailureReason) {
      case Firebase.RemoteConfig.FetchFailureReason.Error:
        DebugLog("Fetch failed for unknown reason");
        break;
      case Firebase.RemoteConfig.FetchFailureReason.Throttled:
        DebugLog("Fetch throttled until " + Firebase.RemoteConfig.GetInfo().ThrottledEndTime);
        break;
      }
      break;
    case Firebase.RemoteConfig.LastFetchStatus.Pending:
      DebugLog("Latest Fetch call still pending.");
      break;
    }
  }
}
