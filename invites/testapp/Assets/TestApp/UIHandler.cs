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
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {

  Firebase.FirebaseApp app;
  Firebase.Invites.InvitesSender sender;
  Firebase.Invites.InvitesReceiver receiver;

  public GUISkin fb_GUISkin;
  private Vector2 scrollViewVector = Vector2.zero;
  bool UIEnabled = true;
  private string logText = "";
  const int kMaxLogSize = 16382;


  // When the app starts, create a firebase app object,
  // Initialize the Sender and Receiver objects
  void Start() {
    DebugLog("Setting up firebase...");
    app = Firebase.FirebaseApp.Create();
    DebugLog(String.Format("Created the firebase app: {0}", app.Name));
    DebugLog("Invites initialized");
    sender = new Firebase.Invites.InvitesSender(app);
    DebugLog("InvitesSender created");
    receiver = new Firebase.Invites.InvitesReceiver(app);
    DebugLog("InvitesReceiver created");
    receiver.FetchAsync().ContinueWith(HandleFetchResult);
  }

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
      Application.Quit();
    }
  }

  void OnDestroy() {
    app = null;
    sender = null;
    receiver = null;
  }

  void HandleFetchResult(Task<Firebase.Invites.FetchResult> fetchTask) {
    if (fetchTask.IsCanceled) {
      DebugLog("Fetch canceled.");
    } else if (fetchTask.IsFaulted) {
      DebugLog("Fetch encountered an error.");
      DebugLog(fetchTask.Exception.ToString());
    } else if (fetchTask.IsCompleted) {
      DebugLog("Fetch completed successfully!");
      Firebase.Invites.FetchResult result = fetchTask.Result;
      if (result.InvitationID != "") {
        DebugLog("Fetch: Get invitation ID: " + result.InvitationID);
        receiver.ConvertInvitationAsync(result.InvitationID)
            .ContinueWith(HandleConversionResult);
      }
      if (result.DeepLink.ToString() != "") {
        DebugLog("Fetch: Got deep link: " + result.DeepLink);
      }
      if (result.InvitationID.ToString() == ""
        && result.DeepLink.ToString() == "") {
        DebugLog("Fetch: No invitation ID or deep link");
      }
    }
  }

  void HandleConversionResult(
      Task<Firebase.Invites.ConvertInvitationResult> convertTask) {
    if (convertTask.IsCanceled) {
      DebugLog("Conversion canceled.");
    } else if (convertTask.IsFaulted) {
      DebugLog("Conversion encountered an error:");
      DebugLog(convertTask.Exception.ToString());
    } else if (convertTask.IsCompleted) {
      DebugLog("Conversion completed successfully!");
      DebugLog("ConvertInvitation: Successfully converted invitation ID: " +
        convertTask.Result.InvitationID);
    }
  }

  public void SendInvite() {
    if (sender == null) {
      DebugLog("Please wait for Fetch to complete");
      return;
    }

    DebugLog("SendInvite: Sending an invitation...");
    sender.SetTitleText("Invites Test App");
    sender.SetMessageText("Please try my app! It's awesome.");
    sender.SetCallToActionText("Download it for FREE");
    sender.SetDeepLinkUrl("http://google.com/abc");
    sender.SendInviteAsync().ContinueWith(HandleSentInvite);
  }

  void HandleSentInvite(Task<Firebase.Invites.SendInviteResult> sendTask) {
    if (sendTask.IsCanceled) {
      DebugLog("Invitation canceled.");
    } else if (sendTask.IsFaulted) {
      DebugLog("Invitation encountered an error:");
      DebugLog(sendTask.Exception.ToString());
    } else if (sendTask.IsCompleted) {
      DebugLog("SendInvite: " + (new List<string>(sendTask.Result.InvitationIDs)).Count +
        " invites sent successfully.");
      foreach (string id in sendTask.Result.InvitationIDs) {
        DebugLog("SendInvite: Invite code: " + id);
      }
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
    scrollViewVector = GUILayout.BeginScrollView (scrollViewVector);
    GUILayout.Label(logText);
    GUILayout.EndScrollView();
  }

  // Render the buttons and other controls.
  void GUIDisplayControls(){
    if (UIEnabled || true) {
      GUILayout.BeginVertical();

      if (GUILayout.Button("Send Invite")) {
        SendInvite();
      }

      GUILayout.EndVertical();
    }
  }

  // Render the GUI:
  void OnGUI() {
    GUI.skin = fb_GUISkin;
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
