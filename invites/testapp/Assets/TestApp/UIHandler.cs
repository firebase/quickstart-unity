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

  public Text outputText;
  Firebase.App app;
  Firebase.Invites.InvitesSender sender;
  Firebase.Invites.InvitesReceiver receiver;

  public void DebugLog(string s) {
    print(s);
    outputText.text += s + "\n";
  }

  // When the app starts, create a firebase app object,
  // Initialize the Sender and Receiver objects
  void Start() {
    DebugLog("Setting up firebase...");
    Firebase.AppOptions ops = new Firebase.AppOptions();
    DebugLog(String.Format("Created the AppOptions, with appID: {0}", ops.AppID));
    DebugLog("About to Create App");
    app = Firebase.App.Create(ops);
    DebugLog(String.Format("Created the firebase app: {0}", app.Name));

    Firebase.Invites.Initialize(app);
    DebugLog("Invites initialized");
    receiver = new Firebase.Invites.InvitesReceiver(app);
    DebugLog("InvitesReceiver created");
    receiver.Fetch().ContinueWith(HandleFetchResult);
  }

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
      Application.Quit();
    }
  }

  void HandleFetchResult(Task<Firebase.Invites.InvitesReceiver.FetchResult> fetchTask) {
    if (fetchTask.IsCanceled) {
      DebugLog("Fetch canceled.");
    } else if (fetchTask.IsFaulted) {
      DebugLog("Fetch encountered an error.");
      DebugLog(fetchTask.Exception.ToString());
    } else if (fetchTask.IsCompleted) {
      DebugLog("Fetch completed successfully!");
      Firebase.Invites.InvitesReceiver.FetchResult result = fetchTask.Result;
      if (result.InvitationId != "") {
        DebugLog("Fetch: Get invitation ID: " + result.InvitationId);
        receiver.ConvertInvitation(result.InvitationId).ContinueWith(HandleConversionResult);
      }
      if (result.DeepLink != "") {
        DebugLog("Fetch: Got deep link: " + result.DeepLink);
      }
      if (result.InvitationId == "" && result.DeepLink == "") {
        DebugLog("Fetch: No invitation ID or deep link");
      }
    }

    // Now that Fetching is complete, initialize the Sender.
    sender = new Firebase.Invites.InvitesSender(app);
    DebugLog("InvitesSender created");
  }

  void HandleConversionResult(Task<Firebase.Invites.InvitesReceiver.ConvertInvitationResult> convertTask) {
    if (convertTask.IsCanceled) {
      DebugLog("Conversion canceled.");
    } else if (convertTask.IsFaulted) {
      DebugLog("Conversion encountered an error:");
      DebugLog(convertTask.Exception.ToString());
    } else if (convertTask.IsCompleted) {
      DebugLog("Conversion completed successfully!");
      DebugLog("ConvertInvitation: Successfully converted invitation ID: " +
        convertTask.Result.InvitationId);
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
    sender.SendInvite().ContinueWith(HandleSentInvite);
  }

  void HandleSentInvite(Task<Firebase.Invites.InvitesSender.SendInviteResult> sendTask) {
    if (sendTask.IsCanceled) {
      DebugLog("Invitation canceled.");
    } else if (sendTask.IsFaulted) {
      DebugLog("Invitation encountered an error:");
      DebugLog(sendTask.Exception.ToString());
    } else if (sendTask.IsCompleted) {
      DebugLog("SendInvite: " + (new List<string>(sendTask.Result.InvitationIds)).Count +
        " invites sent successfully.");
      foreach (string id in sendTask.Result.InvitationIds) {
        DebugLog("SendInvite: Invite code: " + id);
      }
    }
  }
}
