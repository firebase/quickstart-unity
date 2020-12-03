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

namespace Firebase.Sample.Analytics {
  using Firebase;
  using Firebase.Analytics;
  using Firebase.Extensions;
  using System;
  using System.Threading.Tasks;
  using UnityEngine;

  // Handler for UI buttons on the scene.  Also performs some
  // necessary setup (initializing the firebase app, etc) on
  // startup.
  public class UIHandler : MonoBehaviour {
    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private Vector2 scrollViewVector = Vector2.zero;
    bool UIEnabled = true;
    private string logText = "";
    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool firebaseInitialized = false;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start() {
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

    // Exit if escape (or back, on mobile) is pressed.
    public virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
          Application.Quit();
      }
    }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase() {
      DebugLog("Enabling data collection.");
      FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

      DebugLog("Set user properties.");
      // Set the user's sign up method.
      FirebaseAnalytics.SetUserProperty(
        FirebaseAnalytics.UserPropertySignUpMethod,
        "Google");
      // Set the user ID.
      FirebaseAnalytics.SetUserId("uber_user_510");
      // Set default session duration values.
      FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
      firebaseInitialized = true;
    }

    // End our analytics session when the program exits.
    void OnDestroy() { }

    public void AnalyticsLogin() {
      // Log an event with no parameters.
      DebugLog("Logging a login event.");
      FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
    }

    public void AnalyticsProgress() {
      // Log an event with a float.
      DebugLog("Logging a progress event.");
      FirebaseAnalytics.LogEvent("progress", "percent", 0.4f);
    }

    public void AnalyticsScore() {
      // Log an event with an int parameter.
      DebugLog("Logging a post-score event.");
      FirebaseAnalytics.LogEvent(
        FirebaseAnalytics.EventPostScore,
        FirebaseAnalytics.ParameterScore,
        42);
    }

    public void AnalyticsGroupJoin() {
      // Log an event with a string parameter.
      DebugLog("Logging a group join event.");
      FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventJoinGroup, FirebaseAnalytics.ParameterGroupId,
        "spoon_welders");
    }

    public void AnalyticsLevelUp() {
      // Log an event with multiple parameters.
      DebugLog("Logging a level up event.");
      FirebaseAnalytics.LogEvent(
        FirebaseAnalytics.EventLevelUp,
        new Parameter(FirebaseAnalytics.ParameterLevel, 5),
        new Parameter(FirebaseAnalytics.ParameterCharacter, "mrspoon"),
        new Parameter("hit_accuracy", 3.14f));
    }

    // Reset analytics data for this app instance.
    public void ResetAnalyticsData() {
      DebugLog("Reset analytics data.");
      FirebaseAnalytics.ResetAnalyticsData();
    }

    // Get the current app instance ID.
    public Task<string> DisplayAnalyticsInstanceId() {
      return FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task => {
        if (task.IsCanceled) {
          DebugLog("App instance ID fetch was canceled.");
        } else if (task.IsFaulted) {
          DebugLog(String.Format("Encounted an error fetching app instance ID {0}",
                                  task.Exception.ToString()));
        } else if (task.IsCompleted) {
          DebugLog(String.Format("App instance ID: {0}", task.Result));
        }
        return task;
      }).Unwrap();
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

        if (GUILayout.Button("Log Login")) {
          AnalyticsLogin();
        }
        if (GUILayout.Button("Log Progress")) {
          AnalyticsProgress();
        }
        if (GUILayout.Button("Log Score")) {
          AnalyticsScore();
        }
        if (GUILayout.Button("Log Group Join")) {
          AnalyticsGroupJoin();
        }
        if (GUILayout.Button("Log Level Up")) {
          AnalyticsLevelUp();
        }
        if (GUILayout.Button("Reset Analytics Data")) {
          ResetAnalyticsData();
        }
        if (GUILayout.Button("Show Analytics Instance ID")) {
          DisplayAnalyticsInstanceId();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
    }

    // Render the GUI:
    void OnGUI() {
      GUI.skin = fb_GUISkin;
      if (dependencyStatus != DependencyStatus.Available) {
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
