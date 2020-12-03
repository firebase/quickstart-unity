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

namespace Firebase.Sample.Messaging {
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
    private string logText = "";
    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;
    private string topic = "TestTopic";
    private bool UIEnabled = true;

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation) {
      bool complete = false;
      if (task.IsCanceled) {
        DebugLog(operation + " canceled.");
      } else if (task.IsFaulted) {
        DebugLog(operation + " encounted an error.");
        foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
          string errorCode = "";
          Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
          if (firebaseEx != null) {
            errorCode = String.Format("Error.{0}: ",
              ((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString());
          }
          DebugLog(errorCode + exception.ToString());
        }
      } else if (task.IsCompleted) {
        DebugLog(operation + " completed");
        complete = true;
      }
      return complete;
    }


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

    // Setup message event handlers.
    void InitializeFirebase() {
      Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
      Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
      Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(task => {
        LogTaskCompletion(task, "SubscribeAsync");
      });
      DebugLog("Firebase Messaging Initialized");

      // This will display the prompt to request permission to receive
      // notifications if the prompt has not already been displayed before. (If
      // the user already responded to the prompt, thier decision is cached by
      // the OS and can be changed in the OS settings).
      Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
        task => {
          LogTaskCompletion(task, "RequestPermissionAsync");
        }
      );
      isFirebaseInitialized = true;
    }

    public virtual void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
      DebugLog("Received a new message");
      var notification = e.Message.Notification;
      if (notification != null) {
        DebugLog("title: " + notification.Title);
        DebugLog("body: " + notification.Body);
        var android = notification.Android;
        if (android != null) {
            DebugLog("android channel_id: " + android.ChannelId);
        }
      }
      if (e.Message.From.Length > 0)
        DebugLog("from: " + e.Message.From);
      if (e.Message.Link != null) {
        DebugLog("link: " + e.Message.Link.ToString());
      }
      if (e.Message.Data.Count > 0) {
        DebugLog("data:");
        foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                 e.Message.Data) {
          DebugLog("  " + iter.Key + ": " + iter.Value);
        }
      }
    }

    public virtual void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
      DebugLog("Received Registration Token: " + token.Token);
    }

    public void ToggleTokenOnInit() {
      bool newValue = !Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;
      Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
      DebugLog("Set TokenRegistrationOnInitEnabled to " + newValue);
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
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

        GUILayout.BeginHorizontal();
        GUILayout.Label("Topic:", GUILayout.Width(Screen.width * 0.20f));
        topic = GUILayout.TextField(topic);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Subscribe")) {
          Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(
            task => {
              LogTaskCompletion(task, "SubscribeAsync");
            }
          );
          DebugLog("Subscribed to " + topic);
        }
        if (GUILayout.Button("Unsubscribe")) {
          Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(topic).ContinueWithOnMainThread(
            task => {
              LogTaskCompletion(task, "UnsubscribeAsync");
            }
          );
          DebugLog("Unsubscribed from " + topic);
        }
        if (GUILayout.Button("Toggle Token On Init")) {
          ToggleTokenOnInit();
        }
        if (GUILayout.Button("GetToken")) {
          String token = "";
          Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(
            task => {
              token = task.Result;
              LogTaskCompletion(task, "GetTokenAsync");
            }
          );
          DebugLog("GetTokenAsync " + token);
        }

        if (GUILayout.Button("DeleteToken")) {
          Firebase.Messaging.FirebaseMessaging.DeleteTokenAsync().ContinueWithOnMainThread(
            task => {
              LogTaskCompletion(task, "DeleteTokenAsync");
            }
          );
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

      Rect logArea;
      Rect controlArea;

      if (Screen.width < Screen.height) {
        // Portrait mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width, Screen.height * 0.5f);
        logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width, Screen.height * 0.5f);
      } else {
        // Landscape mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
        logArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f, Screen.height);
      }

      GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));

      GUILayout.BeginArea(logArea);
      GUIDisplayLog();
      GUILayout.EndArea();

      GUILayout.BeginArea(controlArea);
      GUIDisplayControls();
      GUILayout.EndArea();

      GUILayout.EndArea();
    }
  }
}
