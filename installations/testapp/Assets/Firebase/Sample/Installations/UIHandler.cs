// Copyright 2020 Google Inc. All rights reserved.
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

namespace Firebase.Sample.Installations {
  using Firebase;
  using Firebase.Extensions;
  using Firebase.Installations;
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
    protected FirebaseInstallations installations = null;

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
      DebugLog("Get Installations.");
      installations = FirebaseInstallations.DefaultInstance;
      firebaseInitialized = true;
    }

    void OnDestroy() {
      installations = null;
    }

    // Get / create and log the Installations.
    protected Task GetIdAsync() {
      DisableUI();
      return installations.GetIdAsync().ContinueWithOnMainThread(task => {
        EnableUI();
        if (LogTaskCompletion(task, "GetIdAsync")) {
          DebugLog(String.Format("Installations {0}", task.Result));
        }
      });
    }

    // Delete the Installations.
    protected Task DeleteAsync() {
      DisableUI();
      return installations.DeleteAsync().ContinueWithOnMainThread(task => {
        EnableUI();
        LogTaskCompletion(task, "DeleteAsync");
      });
    }

    // Get a token from the current Installations.
    protected Task GetTokenAsync() {
      DisableUI();
      return installations.GetTokenAsync(true).ContinueWithOnMainThread(task => {
        EnableUI();
        if (LogTaskCompletion(task, "GetTokenAsync")) {
          DebugLog(String.Format("Token {0}", task.Result));
        }
      });
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    bool LogTaskCompletion(Task task, string operation) {
      bool complete = false;
      if (task.IsCanceled) {
        DebugLog(operation + " canceled.");
      } else if (task.IsFaulted) {
        DebugLog(operation + " encounted an error.");
        foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
          string errorCode = "";
          FirebaseException firebaseException = exception as FirebaseException;
          if (firebaseException != null) {
            errorCode = String.Format("Error code={0}: ",
                firebaseException.ErrorCode.ToString(),
                firebaseException.Message);
          }
          DebugLog(errorCode + exception.ToString());
        }
      } else if (task.IsCompleted) {
        DebugLog(operation + " completed");
        complete = true;
      }
      return complete;
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

        if (GUILayout.Button("Get ID")) {
          GetIdAsync();
        }
        if (GUILayout.Button("Delete")) {
          DeleteAsync();
        }
        if (GUILayout.Button("Get Token")) {
          GetTokenAsync();
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
