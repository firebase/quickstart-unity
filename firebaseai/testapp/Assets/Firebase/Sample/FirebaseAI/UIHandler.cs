// Copyright 2025 Google Inc. All rights reserved.
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

namespace Firebase.Sample.FirebaseAI {
  using Firebase;
  using Firebase.AI;
  using Firebase.Extensions;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Text;
  using UnityEngine;

  // Handler for UI buttons on the scene.  Also performs some
  // necessary setup (initializing the firebase app, etc) on
  // startup.
  public class UIHandler : MonoBehaviour {
    private const int kMaxLogSize = 16382;

    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private string logText = "";
    private Vector2 scrollViewVector = Vector2.zero;
    protected bool UIEnabled = false;
    private float textAreaLineHeight;

    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    protected virtual void Start() {
      UIEnabled = true;
    }

    protected void InitializeFirebase() {
      FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
          DebugLog("Firebase Ready: " + FirebaseApp.DefaultInstance);
        } else {
          Debug.LogError(
            "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }

    public string ModelName = "gemini-2.0-flash";

    private int backendSelection = 0;
    private string[] backendChoices = new string[] { "Google AI Backend", "Vertex AI Backend" };
    private GenerativeModel GetModel() {
      var backend = backendSelection == 0
          ? FirebaseAI.Backend.GoogleAI()
          : FirebaseAI.Backend.VertexAI();

      return FirebaseAI.GetInstance(backend).GetGenerativeModel(ModelName);
    }

    // Send a single message to the Generative Model, without any history.
    async Task SendSingleMessage(string message) {
      DebugLog("Sending message to model: " + message);
      var response = await GetModel().GenerateContentAsync(message);
      DebugLog("Response: " + response.Text);
    }

    private Chat chatSession = null;
    void StartChatSession() {
      chatSession = GetModel().StartChat();
    }

    void CloseChatSession() {
      chatSession = null;
    }

    // Send a message to the ongoing Chat with the Generative Model, which
    // will preserve the history.
    async Task SendChatMessage(string message) {
      if (chatSession == null) {
        DebugLog("Missing Chat Session");
        return;
      }

      DebugLog("Sending chat message: " + message);
      var response = await chatSession.SendMessageAsync(message);
      DebugLog("Chat response: " + response.Text);
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s) {
      Debug.Log(s);
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

    private string textfieldString = "Hello";

    // Render the buttons and other controls.
    void GUIDisplayControls() {
      if (UIEnabled) {
        controlsScrollViewVector = GUILayout.BeginScrollView(controlsScrollViewVector);

        GUILayout.BeginVertical();

        if (chatSession == null) {
          backendSelection = GUILayout.SelectionGrid(backendSelection, backendChoices, backendChoices.Length);

          textfieldString = GUILayout.TextField(textfieldString);

          if (GUILayout.Button("Send Single Message")) {
            _ = SendSingleMessage(textfieldString);
          }

          if (GUILayout.Button("Start Chat Session")) {
            StartChatSession();
          }
        } else {
          textfieldString = GUILayout.TextField(textfieldString);

          if (GUILayout.Button("Send Chat Message")) {
            _ = SendChatMessage(textfieldString);
          }

          if (GUILayout.Button("Close Chat Session")) {
            CloseChatSession();
          }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
    }

    // Render the GUI:
    void OnGUI() {
      GUI.skin = fb_GUISkin;

      GUI.skin.textArea.fontSize = GUI.skin.textField.fontSize;
      // Reduce the text size on the desktop.
      if (UnityEngine.Application.platform != RuntimePlatform.Android &&
          UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer) {
        var fontSize = GUI.skin.textArea.fontSize / 4;
        GUI.skin.textArea.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.textField.fontSize = fontSize;
      }
      GUI.skin.textArea.stretchHeight = true;
      // Calculate the height of line of text in a text area.
      if (textAreaLineHeight == 0.0f) {
        textAreaLineHeight = GUI.skin.textArea.CalcSize(new GUIContent("Hello World")).y;
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
