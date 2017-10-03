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

using Firebase;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {
  protected string MyStorageBucket = "gs://YOUR-FIREBASE-BUCKET/";
  private const int kMaxLogSize = 16382;

  public GUISkin fb_GUISkin;
  private Vector2 controlsScrollViewVector = Vector2.zero;
  private string logText = "";
  private Vector2 scrollViewVector = Vector2.zero;
  protected bool UIEnabled = true;
  protected string firebaseStorageLocation;
  protected string fileContents;
  private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
  protected FirebaseStorage storage;

  // When the app starts, check to make sure that we have
  // the required dependencies to use Firebase, and if not,
  // add them if possible.
  protected virtual void Start() {
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
      dependencyStatus = task.Result;
      if (dependencyStatus == DependencyStatus.Available) {
        InitializeFirebase();
      } else {
        Debug.LogError(
          "Could not resolve all Firebase dependencies: " + dependencyStatus);
      }
    });
  }

  private void InitializeFirebase() {
    var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
    storage = FirebaseStorage.DefaultInstance;
    if (!String.IsNullOrEmpty(appBucket)) {
        MyStorageBucket = String.Format("gs://{0}/", appBucket);
    }
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

  protected IEnumerator UploadToFirebaseStorage() {
    StorageReference reference = FirebaseStorage.DefaultInstance
      .GetReferenceFromUrl(firebaseStorageLocation);
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
    var task = reference.PutBytesAsync(Encoding.UTF8.GetBytes(fileContents), null, null,
                                       default(System.Threading.CancellationToken), null);
#else
    var task = reference.PutBytesAsync(Encoding.UTF8.GetBytes(fileContents));
#endif
    yield return new WaitUntil(() => task.IsCompleted);
    if (task.IsFaulted) {
      DebugLog(task.Exception.ToString());
      throw task.Exception;
    } else {
      fileContents = "";
      DebugLog("Finished uploading... Download Url: " + task.Result.DownloadUrl.ToString());
      DebugLog("Press the Download button to download text from Cloud Storage");
    }
  }

  protected IEnumerator DownloadFromFirebaseStorage() {
    StorageReference reference = FirebaseStorage.DefaultInstance
      .GetReferenceFromUrl(firebaseStorageLocation);
    var task = reference.GetBytesAsync(1024 * 1024);
    yield return new WaitUntil(() => task.IsCompleted);
    if (task.IsFaulted) {
      DebugLog(task.Exception.ToString());
      throw task.Exception;
    } else {
      fileContents = Encoding.UTF8.GetString(task.Result);
      DebugLog("Finished downloading...");
      DebugLog("Contents=" + fileContents);
    }
  }

  protected IEnumerator DownloadFromFirebaseStorageWithStream() {
    StorageReference reference = FirebaseStorage.DefaultInstance
      .GetReferenceFromUrl(firebaseStorageLocation);
    // Used to buffer data from the downloaded stream.
    var memoryStream = new System.IO.MemoryStream();
    // Limit the amount of data to render in the text view.
    const int bytesToStoreInMemoryStream = 64 * 1024;

    // Download the file using a stream.
    var task = reference.GetStreamAsync((stream) => {
        var buffer = new byte[1024];
        int remaining = bytesToStoreInMemoryStream;
        int read;
        // Read data to render in the text view.
        while (remaining > 0 &&
               (read = stream.Read(buffer, 0,
                                   System.Math.Min(buffer.Length, remaining))) > 0) {
          memoryStream.Write(buffer, 0, read);
          remaining -= read;
        }
        // Read remaining data from the stream to simulate the complete download.
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
        }
      },
      // Report as the stream download progresses.
      new StorageProgress<DownloadState>(
        state => {
          DebugLog(String.Format("Progress: {0} out of {1}", state.BytesTransferred,
                    state.TotalByteCount));
        }),
      System.Threading.CancellationToken.None);

     yield return new WaitUntil(() => task.IsCompleted);
     if (task.IsFaulted) {
        DebugLog(task.Exception.ToString());
        throw task.Exception;
     } else {
        memoryStream.Position = 0;
        fileContents = (new System.IO.StreamReader(memoryStream)).ReadToEnd();
        DebugLog("Finished downloading...");
        DebugLog("Contents=" + fileContents);
     }
  }

  // Render the buttons and other controls.
  void GUIDisplayControls() {
    if (UIEnabled) {

      controlsScrollViewVector =
          GUILayout.BeginScrollView(controlsScrollViewVector);

      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      GUILayout.Label("Text:", GUILayout.Width(Screen.width * 0.20f));
      if (fileContents == null) {
        fileContents = "Sample text... (type here)";
      }
      fileContents = GUILayout.TextArea(fileContents,
        GUILayout.Height(Screen.height * 0.25f));
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      GUILayout.BeginHorizontal();
      GUILayout.Label("Storage Location:", GUILayout.Width(Screen.width * 0.20f));
      if (firebaseStorageLocation == null) {
        firebaseStorageLocation = MyStorageBucket + "File.txt";;
      }
      firebaseStorageLocation  = GUILayout.TextArea(firebaseStorageLocation,
                                    GUILayout.Height(Screen.height * 0.15f));
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      GUILayout.BeginVertical();
      if (GUILayout.Button("Upload")) {
        StartCoroutine(UploadToFirebaseStorage());
      }

      if (GUILayout.Button("Download Bytes")) {
        StartCoroutine(DownloadFromFirebaseStorage());
      }
      if (GUILayout.Button("Download Stream")) {
        StartCoroutine(DownloadFromFirebaseStorageWithStream());
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

    GUI.skin.textArea.fontSize = GUI.skin.textField.fontSize;
    // Reduce the text size on the desktop.
    if (UnityEngine.Application.platform != RuntimePlatform.Android &&
        UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer) {
      GUI.skin.textArea.fontSize /= 4;
      GUI.skin.button.fontSize = GUI.skin.textArea.fontSize;
      GUI.skin.label.fontSize = GUI.skin.textArea.fontSize;
    }
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
      controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.6f, Screen.height);
      logArea = new Rect(Screen.width * 0.6f, 0.0f, Screen.width * 0.6f, Screen.height);
    }

    GUILayout.BeginArea(logArea);
    GUIDisplayLog();
    GUILayout.EndArea();

    GUILayout.BeginArea(controlArea);
    GUIDisplayControls();
    GUILayout.EndArea();
  }
}
