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
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup on startup.
public class UIHandler : MonoBehaviour {

  public GUISkin fb_GUISkin;
  private Vector2 controlsScrollViewVector = Vector2.zero;
  private Vector2 scrollViewVector = Vector2.zero;
  bool UIEnabled = true;
  private string logText = "";
  const int kMaxLogSize = 16382;
  Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

  // When the app starts, check to make sure that we have
  // the required dependencies to use Firebase, and if not,
  // add them if possible.
  void Start() {
    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
      dependencyStatus = task.Result;
      if (dependencyStatus == Firebase.DependencyStatus.Available) {
        InitializeFirebase();
      } else {
        Debug.LogError(
          "Could not resolve all Firebase dependencies: " + dependencyStatus);
      }
    });
  }

  // Set the listeners for the various Invite received events.
  void InitializeFirebase() {
    DebugLog("Setting up firebase...");
    Firebase.Invites.FirebaseInvites.InviteReceived += OnInviteReceived;
    Firebase.Invites.FirebaseInvites.InviteNotReceived += OnInviteNotReceived;
    Firebase.Invites.FirebaseInvites.ErrorReceived += OnErrorReceived;
    DebugLog("Invites initialized");
  }

  // Exit if escape (or back, on mobile) is pressed.
  void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
  }

  public void OnInviteReceived(object sender,
                               Firebase.Invites.InviteReceivedEventArgs e) {
    if (e.InvitationId != "") {
      DebugLog("Invite Received: Invitation ID: " + e.InvitationId);
      Firebase.Invites.FirebaseInvites.ConvertInvitationAsync(
        e.InvitationId).ContinueWith(HandleConversionResult);
    }
    if (e.DeepLink.ToString() != "") {
      DebugLog("Invite Received: Deep Link: " + e.DeepLink);
    }
  }

  public void OnInviteNotReceived(object sender, System.EventArgs e) {
    DebugLog("No Invite or Deep Link received on start up");
  }

  public void OnErrorReceived(object sender,
                              Firebase.Invites.InviteErrorReceivedEventArgs e) {
    DebugLog("Error occurred received the invite: " +
        e.ErrorMessage);
  }

  void HandleConversionResult(Task convertTask) {
    if (convertTask.IsCanceled) {
      DebugLog("Conversion canceled.");
    } else if (convertTask.IsFaulted) {
      DebugLog("Conversion encountered an error:");
      DebugLog(convertTask.Exception.ToString());
    } else if (convertTask.IsCompleted) {
      DebugLog("Conversion completed successfully!");
      DebugLog("ConvertInvitation: Successfully converted invitation");
    }
  }

  public Task<Firebase.Invites.SendInviteResult> SendInviteAsync() {
    Firebase.Invites.Invite invite = new Firebase.Invites.Invite() {
      TitleText = "Invites Test App",
      MessageText = "Please try my app! It's awesome.",
      CallToActionText = "Download it for FREE",
      DeepLinkUrl = new System.Uri("http://google.com/abc"),
    };
    return Firebase.Invites.FirebaseInvites.SendInviteAsync(
        invite).ContinueWith<Firebase.Invites.SendInviteResult>(HandleSentInvite);
  }

  Firebase.Invites.SendInviteResult HandleSentInvite(Task<Firebase.Invites.SendInviteResult>
      sendTask) {
    if (sendTask.IsCanceled) {
      DebugLog("Invitation canceled.");
    } else if (sendTask.IsFaulted) {
      DebugLog("Invitation encountered an error:");
      DebugLog(sendTask.Exception.ToString());
    } else if (sendTask.IsCompleted) {
      DebugLog("SendInvite: " +
      (new List<string>(sendTask.Result.InvitationIds)).Count +
      " invites sent successfully.");
      foreach (string id in sendTask.Result.InvitationIds) {
        DebugLog("SendInvite: Invite code: " + id);
      }
    }
    return sendTask.Result;
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

  public void DisableUI() {
    UIEnabled = false;
  }

  public void EnableUI() {
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

      if (GUILayout.Button("Send Invite")) {
        SendInviteAsync();
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
      logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width,
        Screen.height * 0.5f);
    } else {
      // Landscape mode
      controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
      logArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f,
        Screen.height);
    }

    GUILayout.BeginArea(logArea);
    GUIDisplayLog();
    GUILayout.EndArea();

    GUILayout.BeginArea(controlArea);
    GUIDisplayControls();
    GUILayout.EndArea();
  }
}
