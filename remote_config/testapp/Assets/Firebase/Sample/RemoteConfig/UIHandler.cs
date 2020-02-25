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

namespace Firebase.Sample.RemoteConfig {
  using Firebase.Extensions;
  using System;
  using System.Threading.Tasks;
  using UnityEngine;

  // Handler for UI buttons on the scene.  Also performs some
  // necessary setup (initializing the firebase app, etc) on
  // startup.
  public
  class UIHandler : MonoBehaviour {
    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private Vector2 scrollViewVector = Vector2.zero;
    bool UIEnabled = true;
    private string logText = "";
    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start() {
      Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          Debug.LogError(
            "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }

    // Initialize remote config, and set the default values.
    void InitializeFirebase() {
      // [START set_defaults]
      System.Collections.Generic.Dictionary<string, object> defaults =
        new System.Collections.Generic.Dictionary<string, object>();

      // These are the values that are used if we haven't fetched data from the
      // server
      // yet, or if we ask for values that the server doesn't have:
      defaults.Add("config_test_string", "default local string");
      defaults.Add("config_test_int", 1);
      defaults.Add("config_test_float", 1.0);
      defaults.Add("config_test_bool", false);

      Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(defaults);
      // [END set_defaults]
      DebugLog("RemoteConfig configured and ready!");
      isFirebaseInitialized = true;
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }


    // Display the currently loaded data.  If fetch has been called, this will be
    // the data fetched from the server.  Otherwise, it will be the defaults.
    // Note:  Firebase will cache this between sessions, so even if you haven't
    // called fetch yet, if it was called on a previous run of the program, you
    //  will still have data from the last time it was run.
    public void DisplayData() {
      DebugLog("Current Data:");
      DebugLog("config_test_string: " +
               Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("config_test_string").StringValue);
      DebugLog("config_test_int: " +
               Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("config_test_int").LongValue);
      DebugLog("config_test_float: " +
               Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("config_test_float").DoubleValue);
      DebugLog("config_test_bool: " +
               Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("config_test_bool").BooleanValue);
    }

    public void DisplayAllKeys() {
      DebugLog("Current Keys:");
      System.Collections.Generic.IEnumerable<string> keys =
          Firebase.RemoteConfig.FirebaseRemoteConfig.Keys;
      foreach (string key in keys) {
        DebugLog("    " + key);
      }
      DebugLog("GetKeysByPrefix(\"config_test_s\"):");
      keys = Firebase.RemoteConfig.FirebaseRemoteConfig.GetKeysByPrefix("config_test_s");
      foreach (string key in keys) {
        DebugLog("    " + key);
      }
    }

    // [START fetch_async]
    // Start a fetch request.
    // FetchAsync only fetches new data if the current data is older than the provided
    // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    // By default the timespan is 12 hours, and for production apps, this is a good
    // number. For this example though, it's set to a timespan of zero, so that
    // changes in the console will always show up immediately.
    public Task FetchDataAsync() {
      DebugLog("Fetching data...");
      System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(
          TimeSpan.Zero);
      return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    //[END fetch_async]

    void FetchComplete(Task fetchTask) {
      if (fetchTask.IsCanceled) {
        DebugLog("Fetch canceled.");
      } else if (fetchTask.IsFaulted) {
        DebugLog("Fetch encountered an error.");
      } else if (fetchTask.IsCompleted) {
        DebugLog("Fetch completed successfully!");
      }

      var info = Firebase.RemoteConfig.FirebaseRemoteConfig.Info;
      switch (info.LastFetchStatus) {
        case Firebase.RemoteConfig.LastFetchStatus.Success:
          Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();
          DebugLog(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                 info.FetchTime));
          break;
        case Firebase.RemoteConfig.LastFetchStatus.Failure:
          switch (info.LastFetchFailureReason) {
            case Firebase.RemoteConfig.FetchFailureReason.Error:
              DebugLog("Fetch failed for unknown reason");
              break;
            case Firebase.RemoteConfig.FetchFailureReason.Throttled:
              DebugLog("Fetch throttled until " + info.ThrottledEndTime);
              break;
          }
          break;
        case Firebase.RemoteConfig.LastFetchStatus.Pending:
          DebugLog("Latest Fetch call still pending.");
          break;
      }
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

    void DisableUI() {
      UIEnabled = false;
    }

    void EnableUI() {
      UIEnabled = true;
    }

    // Render the log output in a scroll view.
    void GUIDisplayLog() {
      scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
      GUILayout.Label(logText);
      GUILayout.EndScrollView();
    }

    // Render the buttons and other controls.
    void GUIDisplayControls() {
      if (UIEnabled) {
        controlsScrollViewVector =
            GUILayout.BeginScrollView(controlsScrollViewVector);
        GUILayout.BeginVertical();
        if (GUILayout.Button("Display Current Data")) {
          DisplayData();
        }
        if (GUILayout.Button("Display All Keys")) {
          DisplayAllKeys();
        }
        if (GUILayout.Button("Fetch Remote Data")) {
          FetchDataAsync();
        }
        GUILayout.EndVertical();
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
      Rect logArea, controlArea;

      if (Screen.width < Screen.height) {
        // Portrait mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width, Screen.height * 0.5f);
        logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width, Screen.height * 0.5f);
      } else {
        // Landscape mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
        logArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f, Screen.height);
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
