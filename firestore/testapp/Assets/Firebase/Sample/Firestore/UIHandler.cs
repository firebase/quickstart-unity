// Copyright 2019 Google Inc. All rights reserved.
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

namespace Firebase.Sample.Firestore {
  using Firebase;
  using Firebase.Extensions;
  using Firebase.Firestore;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
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
    private Vector2 fieldContentsScrollViewVector = Vector2.zero;
    private GUIStyle disabledButtonStyle;
    private float textAreaLineHeight;

    // Path to the collection to query on.
    protected string collectionPath = "col1";
    // DocumentID within the collection. Set to empty to use an autoid (which
    // obviously only works for writing new documents.)
    protected string documentId = "";
    protected string fieldContents;
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    // Currently enabled logging verbosity.
    protected Firebase.LogLevel logLevel = Firebase.LogLevel.Info;
    // Whether an operation is in progress.
    protected bool operationInProgress;
    // Cancellation token source for the current operation.
    protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    // Previously completed task.
    protected Task previousTask;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start() {
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

    protected virtual void InitializeFirebase() {
      // TODO(rgowman): Enable logging here... once the plumbing is setup to
      // make this possible.
      UIEnabled = true;
      isFirebaseInitialized = true;
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

    // Wait for task completion, throwing an exception if the task fails.
    // This could be typically implemented using
    // yield return new WaitUntil(() => task.IsCompleted);
    // however, since many procedures in this sample nest coroutines and we want any task exceptions
    // to be thrown from the top level coroutine (e.g GetKnownValue) we provide this
    // CustomYieldInstruction implementation wait for a task in the context of the coroutine using
    // common setup and tear down code.
    class WaitForTaskCompletion : CustomYieldInstruction {
      Task task;
      UIHandler uiHandler;

      // Create an enumerator that waits for the specified task to complete.
      public WaitForTaskCompletion(UIHandler uiHandler, Task task) {
        uiHandler.previousTask = task;
        uiHandler.operationInProgress = true;
        this.uiHandler = uiHandler;
        this.task = task;
      }

      // Wait for the task to complete.
      public override bool keepWaiting {
        get {
          if (task.IsCompleted) {
            uiHandler.operationInProgress = false;
            uiHandler.cancellationTokenSource = new CancellationTokenSource();
            if (task.IsFaulted) {
              string s = task.Exception.ToString();
              uiHandler.DebugLog(s);
            }
            return false;
          }
          return true;
        }
      }
    }

    protected FirebaseFirestore db {
      get {
        return FirebaseFirestore.DefaultInstance;
      }
    }

    // Cancel the currently running operation.
    protected void CancelOperation() {
      if (operationInProgress && cancellationTokenSource != null) {
        DebugLog("*** Cancelling operation *** ...");
        cancellationTokenSource.Cancel();
        cancellationTokenSource = null;
      }
    }

    /**
     * Tests a *very* basic trip through the Firestore API.
     */
    protected IEnumerator GetKnownValue() {
      DocumentReference doc1 = db.Collection("col1").Document("doc1");
      var task = doc1.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DocumentSnapshot snap = task.Result;
        IDictionary<string, object> dict = snap.ToDictionary();
        if (dict.ContainsKey("field1")) {
          fieldContents = dict["field1"].ToString();
        } else {
          DebugLog("ERROR: Successfully retrieved col1/doc1, but it doesn't contain 'field1' key");
        }
      }
    }

    private static string DictToString(IDictionary<string, object> d) {
      return "{ " + d
          .Select(kv => "(" + kv.Key + ", " + kv.Value + ")")
          .Aggregate("", (current, next) => current + next + ", ")
          + "}";
    }

    private CollectionReference GetCollectionReference() {
      return db.Collection(collectionPath);
    }

    private DocumentReference GetDocumentReference() {
      if (documentId == "") {
        return GetCollectionReference().Document();
      }
      return GetCollectionReference().Document(documentId);
    }

    private IEnumerator WriteDoc(DocumentReference doc, IDictionary<string, object> data) {
      Task setTask = doc.SetAsync(data);
      yield return new WaitForTaskCompletion(this, setTask);
      if (!(setTask.IsFaulted || setTask.IsCanceled)) {
        // Update the collectionPath/documentId because:
        // 1) If the documentId field was empty, this will fill it in with the autoid. This allows
        //    you to manually test via a trivial 'click set', 'click get'.
        // 2) In the automated test, the caller might pass in an explicit docRef rather than pulling
        //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
        //    for the automated tests.)
        collectionPath = doc.Parent.Id;
        documentId = doc.Id;

        fieldContents = "Ok";
      } else {
        fieldContents = "Error";
      }
    }

    private IEnumerator UpdateDoc(DocumentReference doc, IDictionary<string, object> data) {
      Task updateTask = doc.UpdateAsync(data);
      yield return new WaitForTaskCompletion(this, updateTask);
      if (!(updateTask.IsFaulted || updateTask.IsCanceled)) {
        // Update the collectionPath/documentId because:
        // 1) In the automated test, the caller might pass in an explicit docRef rather than pulling
        //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
        //    for the automated tests.)
        collectionPath = doc.Parent.Id;
        documentId = doc.Id;

        fieldContents = "Ok";
      } else {
        fieldContents = "Error";
      }
    }

    private IEnumerator ReadDoc(DocumentReference doc) {
      Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, getTask);
      if (!(getTask.IsFaulted || getTask.IsCanceled)) {
        DocumentSnapshot snap = getTask.Result;
        // TODO(rgowman): Handle `!snap.exists()` case.
        IDictionary<string, object> resultData = snap.ToDictionary();
        fieldContents = "Ok: " + DictToString(resultData);
      } else {
        fieldContents = "Error";
      }
    }

    // Button that can be optionally disabled.
    bool Button(string buttonText, bool enabled) {
      if (disabledButtonStyle == null) {
        disabledButtonStyle = new GUIStyle(fb_GUISkin.button);
        disabledButtonStyle.normal.textColor = Color.grey;
        disabledButtonStyle.active = disabledButtonStyle.normal;
      }
      var style = enabled ? fb_GUISkin.button : disabledButtonStyle;
      return GUILayout.Button(buttonText, style) && enabled;
    }

    // Render the buttons and other controls.
    void GUIDisplayControls() {
      if (UIEnabled) {
        controlsScrollViewVector = GUILayout.BeginScrollView(controlsScrollViewVector);

        GUILayout.BeginVertical();

        GUILayout.Label("CollectionPath:");
        collectionPath = GUILayout.TextField(collectionPath);

        GUILayout.Label("DocumentId (set to empty for autoid):");
        documentId = GUILayout.TextField(documentId);

        GUILayout.Label("Text:");
        if (fieldContents == null) {
          // TODO(rgowman): Provide instructions on how to set document contents here.
          fieldContents = "Sample text... (type here)";
        }
        fieldContents = GUILayout.TextField(fieldContents);

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        if (Button("GetKnownValue", !operationInProgress)) {
          StartCoroutine(GetKnownValue());
        }

        if (Button("WriteDoc", !operationInProgress)) {
          // TODO(rgowman): allow these values to be set by the user via the UI.
          var data = new Dictionary<string, object>{
            {"f1", "v1"},
            {"f2", 2},
            {"f3", true},
            // TODO(rgowman): Add other types here too.
          };
          StartCoroutine(WriteDoc(GetDocumentReference(), data));
        }

        if (Button("UpdateDoc", !operationInProgress)) {
          // TODO(rgowman): allow these values to be set by the user via the UI.
          var data = new Dictionary<string, object>{
            {"f1", "v1b"},
            {"f4", "v4"},
            // TODO(rgowman): Add other types here too.
          };
          StartCoroutine(UpdateDoc(GetDocumentReference(), data));
        }

        if (Button("ReadDoc", !operationInProgress)) {
          StartCoroutine(ReadDoc(GetDocumentReference()));
        }

        GUILayout.EndVertical();

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

      // TODO(rgowman): Fix sizing on desktop. Possibly using something like the following.
      // Sizing in unity is a little weird; fonts that look ok on desktop
      // become really small on mobile, so they need to be adjusted. But we
      // don't support desktop just yet, so we'll skip this step for now.
      /*
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
      */

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
      if (Button("Cancel Operation", operationInProgress)) {
        CancelOperation();
      }
      GUILayout.EndArea();

      GUILayout.BeginArea(controlArea);
      GUIDisplayControls();
      GUILayout.EndArea();
    }
  }
}
