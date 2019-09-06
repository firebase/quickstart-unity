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

namespace Firebase.Sample.Storage {
  using Firebase;
  using Firebase.Extensions;
  using Firebase.Storage;
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
    protected string MyStorageBucket = "gs://YOUR-FIREBASE-BUCKET/";
    private const int kMaxLogSize = 16382;
    protected static string UriFileScheme = Uri.UriSchemeFile + "://";

    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private string logText = "";
    private Vector2 scrollViewVector = Vector2.zero;
    protected bool UIEnabled = false;
    private Vector2 fileContentsScrollViewVector = Vector2.zero;
    private Vector2 metadataScrollViewVector = Vector2.zero;
    private Vector2 storageLocationScrollViewVector = Vector2.zero;
    private GUIStyle disabledButtonStyle;
    private float textAreaLineHeight;

    // Cloud Storage location to download from / upload to.
    protected string storageLocation;
    // String to upload to storageLocation or the contents of the file downloaded from
    // storageLocation.
    protected string fileContents;
    // Used to keep track of changes to fileContents for display in the UI.
    protected string previousFileContents;
    // Section of the file that can be edited by the user.
    protected string editableFileContents;
    // Metadata to change when uploading a file.
    protected string fileMetadataChangeString = "";
    // Local file to upload from / download to.
    protected string localFilename = "downloaded_file.txt";
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;
    // Hold a reference to the FirebaseStorage object so that we're not reinitializing the API on
    // each transfer.
    protected FirebaseStorage storage;
    // Currently enabled logging verbosity.
    protected Firebase.LogLevel logLevel = Firebase.LogLevel.Info;
    // Whether an operation is in progress.
    protected bool operationInProgress;
    // Cancellation token source for the current operation.
    protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    // Cached path to the persistent data directory as Application.persistentDataPath can only be used from the main
    // Unity thread.
    protected string persistentDataPath;

    // Previously completed task.
    protected Task previousTask;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start() {
      persistentDataPath = Application.persistentDataPath;
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
      var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
      storage = FirebaseStorage.DefaultInstance;
      if (!String.IsNullOrEmpty(appBucket)) {
        MyStorageBucket = String.Format("gs://{0}/", appBucket);
      }
      storage.LogLevel = logLevel;
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

    // Retrieve a storage reference from the user specified path.
    protected StorageReference GetStorageReference() {
      // If this is an absolute path including a bucket create a storage instance.
      if (storageLocation.StartsWith("gs://") ||
          storageLocation.StartsWith("http://") ||
          storageLocation.StartsWith("https://")) {
        var storageUri = new Uri(storageLocation);
        var firebaseStorage = FirebaseStorage.GetInstance(
          String.Format("{0}://{1}", storageUri.Scheme, storageUri.Host));
        return firebaseStorage.GetReferenceFromUrl(storageLocation);
      }
      // When using relative paths use the default storage instance which uses the bucket supplied
      // on creation of FirebaseApp.
      return FirebaseStorage.DefaultInstance.GetReference(storageLocation);
    }

    // Wait for task completion, throwing an exception if the task fails.
    // This could be typically implemented using
    // yield return new WaitUntil(() => task.IsCompleted);
    // however, since many procedures in this sample nest coroutines and we want any task exceptions
    // to be thrown from the top level coroutine (e.g UploadBytes) we provide this
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
              uiHandler.DisplayStorageException(task.Exception);
            }
            return false;
          }
          return true;
        }
      }
    }

    // Get the local filename as a URI relative to the persistent data path if the path isn't
    // already a file URI.
    protected virtual string PathToPersistentDataPathUriString(string filename) {
      if (filename.StartsWith(UriFileScheme)) {
        return filename;
      }
      return String.Format("{0}{1}/{2}", UriFileScheme, persistentDataPath,
                           filename);
    }

    // Cancel the currently running operation.
    protected void CancelOperation() {
      if (operationInProgress && cancellationTokenSource != null) {
        DebugLog("*** Cancelling operation *** ...");
        cancellationTokenSource.Cancel();
        cancellationTokenSource = null;
      }
    }

    // Display a storage exception.
    protected void DisplayStorageException(Exception exception) {
      var storageException = exception as StorageException;
      if (storageException != null) {
        DebugLog(String.Format("Error Code: {0}", storageException.ErrorCode));
        DebugLog(String.Format("HTTP Result Code: {0}", storageException.HttpResultCode));
        DebugLog(String.Format("Recoverable: {0}", storageException.IsRecoverableException));
        DebugLog(storageException.ToString());
      } else {
        DebugLog(exception.ToString());
      }
    }

    // Display the result of an upload operation.
    protected void DisplayUploadComplete(Task<StorageMetadata> task) {
      if (!(task.IsFaulted || task.IsCanceled)) {
        fileContents = "";
        fileMetadataChangeString = "";
        DebugLog("Finished uploading");
        DebugLog(MetadataToString(task.Result, false));
        DebugLog("Press the Download button to download text from Cloud Storage\n");
      }
    }

    // Write upload state to the log.
    protected virtual void DisplayUploadState(UploadState uploadState) {
      if (operationInProgress) {
        DebugLog(String.Format("Uploading {0}: {1} out of {2}", uploadState.Reference.Name,
                               uploadState.BytesTransferred, uploadState.TotalByteCount));
      }
    }

    // Upload file text to Cloud Storage using a byte array.
    protected IEnumerator UploadBytes() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Uploading to {0} ...", storageReference.Path));
      var task = storageReference.PutBytesAsync(
        Encoding.UTF8.GetBytes(fileContents), StringToMetadataChange(fileMetadataChangeString),
        new StorageProgress<UploadState>(DisplayUploadState),
        cancellationTokenSource.Token, null);
      yield return new WaitForTaskCompletion(this, task);
      DisplayUploadComplete(task);
    }

    // Upload file to Cloud Storage using a stream.
    protected IEnumerator UploadStream() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Uploading to {0} using stream...", storageReference.Path));
      var task = storageReference.PutStreamAsync(
        new MemoryStream(System.Text.Encoding.ASCII.GetBytes(fileContents)),
        StringToMetadataChange(fileMetadataChangeString),
        new StorageProgress<UploadState>(DisplayUploadState),
        cancellationTokenSource.Token, null);
      yield return new WaitForTaskCompletion(this, task);
      DisplayUploadComplete(task);
    }

    // Upload a file from the local filesystem to Cloud Storage.
    protected IEnumerator UploadFromFile() {
      var localFilenameUriString = PathToPersistentDataPathUriString(localFilename);
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Uploading '{0}' to '{1}'...", localFilenameUriString,
                             storageReference.Path));
      var task = storageReference.PutFileAsync(
        localFilenameUriString, StringToMetadataChange(fileMetadataChangeString),
        new StorageProgress<UploadState>(DisplayUploadState),
        cancellationTokenSource.Token, null);
      yield return new WaitForTaskCompletion(this, task);
      DisplayUploadComplete(task);
    }

    // Update the metadata on the file in Cloud Storage.
    protected IEnumerator UpdateMetadata() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Updating metadata of {0} ...", storageReference.Path));
      var task = storageReference.UpdateMetadataAsync(StringToMetadataChange(
        fileMetadataChangeString));
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DebugLog("Updated metadata");
        DebugLog(MetadataToString(task.Result, false) + "\n");
      }
    }

    // Write download state to the log.
    protected virtual void DisplayDownloadState(DownloadState downloadState) {
      if (operationInProgress) {
        DebugLog(String.Format("Downloading {0}: {1} out of {2}", downloadState.Reference.Name,
                               downloadState.BytesTransferred, downloadState.TotalByteCount));
      }
    }

    // Download from Cloud Storage into a byte array.
    protected IEnumerator DownloadBytes() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Downloading {0} ...", storageReference.Path));
      var task = storageReference.GetBytesAsync(
        0, new StorageProgress<DownloadState>(DisplayDownloadState),
        cancellationTokenSource.Token);
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DebugLog("Finished downloading bytes");
        fileContents = System.Text.Encoding.Default.GetString(task.Result);
        DebugLog(String.Format("File Size {0} bytes\n", fileContents.Length));
      }
    }

    // Download from Cloud Storage using a stream.
    protected IEnumerator DownloadStream() {
      // Download the file using a stream.
      fileContents = "";
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Downloading {0} with stream ...", storageReference.Path));
      var task = storageReference.GetStreamAsync((stream) => {
        var buffer = new byte[1024];
        int read;
        // Read data to render in the text view.
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
          fileContents += System.Text.Encoding.Default.GetString(buffer, 0, read);
        }
      },
        new StorageProgress<DownloadState>(DisplayDownloadState),
        cancellationTokenSource.Token);

      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DebugLog("Finished downloading stream\n");
      }
    }

    // Get a local filesystem path from a file:// URI.
    protected string FileUriStringToPath(string fileUriString) {
      return Uri.UnescapeDataString((new Uri(fileUriString)).PathAndQuery);
    }

    // Download from Cloud Storage to a local file.
    protected IEnumerator DownloadToFile() {
      var storageReference = GetStorageReference();
      var localFilenameUriString = PathToPersistentDataPathUriString(localFilename);
      DebugLog(String.Format("Downloading {0} to {1}...", storageReference.Path,
                             localFilenameUriString));
      var task = storageReference.GetFileAsync(
        localFilenameUriString,
        new StorageProgress<DownloadState>(DisplayDownloadState),
        cancellationTokenSource.Token);
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        var filename = FileUriStringToPath(localFilenameUriString);
        DebugLog(String.Format("Finished downloading file {0} ({1})", localFilenameUriString,
                               filename));
        DebugLog(String.Format("File Size {0} bytes\n", (new FileInfo(filename)).Length));
        fileContents = File.ReadAllText(filename);
      }
    }

    // Delete a remote file.
    protected IEnumerator Delete() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Deleting {0}...", storageReference.Path));
      var task = storageReference.DeleteAsync();
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DebugLog(String.Format("{0} deleted", storageReference.Path));
      }
    }

    // Download and display Metadata for the storage reference.
    protected IEnumerator GetMetadata() {
      var storageReference = GetStorageReference();
      DebugLog(String.Format("Bucket: {0}", storageReference.Bucket));
      DebugLog(String.Format("Path: {0}", storageReference.Path));
      DebugLog(String.Format("Name: {0}", storageReference.Name));
      DebugLog(String.Format("Parent Path: {0}", storageReference.Parent != null ?
                                                     storageReference.Parent.Path : "(root)"));
      DebugLog(String.Format("Root Path: {0}", storageReference.Root.Path));
      DebugLog(String.Format("App: {0}", storageReference.Storage.App.Name));
      var task = storageReference.GetMetadataAsync();
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled))
        DebugLog(MetadataToString(task.Result, false) + "\n");
    }

    // Display the download URL for a storage reference.
    protected IEnumerator ShowDownloadUrl() {
      var task = GetStorageReference().GetDownloadUrlAsync();
      yield return new WaitForTaskCompletion(this, task);
      if (!(task.IsFaulted || task.IsCanceled)) {
        DebugLog(String.Format("DownloadUrl={0}", task.Result));
      }
    }

    // Convert a string in the form:
    //   key1=value1
    //   ...
    //   keyN=valueN
    //
    // to a MetadataChange object.
    //
    // If an empty string is provided this method returns null.
    MetadataChange StringToMetadataChange(string metadataString) {
      var metadataChange = new MetadataChange();
      var customMetadata = new Dictionary<string, string>();
      bool hasMetadata = false;
      foreach (var metadataStringLine in metadataString.Split(new char[] { '\n' })) {
        if (metadataStringLine.Trim() == "")
          continue;
        var keyValue = metadataStringLine.Split(new char[] { '=' });
        if (keyValue.Length != 2) {
          DebugLog(String.Format("Ignoring malformed metadata line '{0}' tokens={2}",
                                 metadataStringLine, keyValue.Length));
          continue;
        }
        hasMetadata = true;
        var key = keyValue[0];
        var value = keyValue[1];
        if (key == "CacheControl") {
          metadataChange.CacheControl = value;
        } else if (key == "ContentDisposition") {
          metadataChange.ContentDisposition = value;
        } else if (key == "ContentEncoding") {
          metadataChange.ContentEncoding = value;
        } else if (key == "ContentLanguage") {
          metadataChange.ContentLanguage = value;
        } else if (key == "ContentType") {
          metadataChange.ContentType = value;
        } else {
          customMetadata[key] = value;
        }
      }
      if (customMetadata.Count > 0)
        metadataChange.CustomMetadata = customMetadata;
      return hasMetadata ? metadataChange : null;
    }

    // Convert a Metadata object to a string.
    protected string MetadataToString(StorageMetadata metadata, bool onlyMutableFields) {
      var fieldsAndValues = new Dictionary<string, object> {
        {"ContentType", metadata.ContentType},
        {"CacheControl", metadata.CacheControl},
        {"ContentDisposition", metadata.ContentDisposition},
        {"ContentEncoding", metadata.ContentEncoding},
        {"ContentLanguage", metadata.ContentLanguage}
      };
      if (!onlyMutableFields) {
        foreach (var kv in new Dictionary<string, object> {
                            {"Reference", metadata.Reference != null ?
                                              metadata.Reference.Path : null},
                            {"Path", metadata.Path},
                            {"Name", metadata.Name},
                            {"Bucket", metadata.Bucket},
                            {"Generation", metadata.Generation},
                            {"MetadataGeneration", metadata.MetadataGeneration},
                            {"CreationTimeMillis", metadata.CreationTimeMillis},
                            {"UpdatedTimeMillis", metadata.UpdatedTimeMillis},
                            {"SizeBytes", metadata.SizeBytes},
                            {"Md5Hash", metadata.Md5Hash}
                         }) {
          fieldsAndValues[kv.Key] = kv.Value;
        }
      }
      foreach (var key in metadata.CustomMetadataKeys) {
        fieldsAndValues[key] = metadata.GetCustomMetadata(key);
      }
      var fieldAndValueStrings = new List<string>();
      foreach (var kv in fieldsAndValues) {
        fieldAndValueStrings.Add(String.Format("{0}={1}", kv.Key, kv.Value));
      }
      return String.Join("\n", fieldAndValueStrings.ToArray());
    }

    // Render a field that only accepts an integer.
    int GUIDisplayIntegerField(int currentValue) {
      int value;
      if (Int32.TryParse(GUILayout.TextField(currentValue.ToString()), out value)) {
        return value;
      }
      return currentValue;
    }

    // Display a field that edits a timespan in milliseconds.
    TimeSpan GUIDisplayTimespanMsField(TimeSpan timeSpan) {
      return TimeSpan.FromMilliseconds(
        (double)GUIDisplayIntegerField((int)timeSpan.TotalMilliseconds));
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
        GUILayout.Label("Text:");
        if (fileContents == null)
          fileContents = "Sample text... (type here)";

        // Track changes in the downloaded file.
        if (fileContents != previousFileContents) {
          previousFileContents = fileContents;
          // TextArea can only display a subset of the file contents so limit what is displayed.
          editableFileContents = fileContents;
          if (editableFileContents.Length > kMaxLogSize) {
            editableFileContents = editableFileContents.Substring(0, kMaxLogSize);
          }
        }

        fileContentsScrollViewVector = GUILayout.BeginScrollView(
          fileContentsScrollViewVector, GUILayout.MinHeight(textAreaLineHeight * 6));
        var newEditableFileContents = GUILayout.TextArea(editableFileContents);
        GUILayout.EndScrollView();

        // Only update the file contents if it was edited, allowing the start of the file to be
        // modified.
        if (newEditableFileContents != editableFileContents) {
          fileContents = newEditableFileContents +
              (fileContents.Length > kMaxLogSize ? fileContents.Substring(kMaxLogSize) : "");
        }

        GUILayout.Space(10);
        GUILayout.Label("MetadataChange: (Key=Value)");
        metadataScrollViewVector = GUILayout.BeginScrollView(
          metadataScrollViewVector, GUILayout.MinHeight(textAreaLineHeight * 3));
        fileMetadataChangeString = GUILayout.TextArea(fileMetadataChangeString);
        GUILayout.EndScrollView();
        GUILayout.Space(10);

        GUILayout.Label("Local File Path:");
        localFilename = GUILayout.TextField(localFilename);

        GUILayout.Label("Storage Location:");
        if (String.IsNullOrEmpty(storageLocation)) {
          storageLocation = MyStorageBucket + "File.txt";
        }
        storageLocationScrollViewVector = GUILayout.BeginScrollView(
          storageLocationScrollViewVector, GUILayout.MinHeight(textAreaLineHeight * 2));
        storageLocation = GUILayout.TextArea(storageLocation);
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        if (Button("Upload", !operationInProgress)) {
          StartCoroutine(UploadBytes());
        }
        if (Button("Upload Stream", !operationInProgress)) {
          StartCoroutine(UploadStream());
        }
        if (Button("Upload from File", !operationInProgress)) {
          StartCoroutine(UploadFromFile());
        }
        if (Button("Update Metadata", !operationInProgress)) {
          StartCoroutine(UpdateMetadata());
        }
        GUILayout.Space(5);
        if (Button("Download Bytes", !operationInProgress)) {
          StartCoroutine(DownloadBytes());
        }
        if (Button("Download Stream", !operationInProgress)) {
          StartCoroutine(DownloadStream());
        }
        if (Button("Download to File", !operationInProgress)) {
          StartCoroutine(DownloadToFile());
        }
        if (Button("Get Metadata", !operationInProgress)) {
          StartCoroutine(GetMetadata());
        }
        if (Button("Get Download URL", !operationInProgress)) {
          StartCoroutine(ShowDownloadUrl());
        }
        GUILayout.Space(5);
        if (Button("Delete", !operationInProgress)) {
          StartCoroutine(Delete());
        }

        GUILayout.Space(10);

        GUILayout.Label("Retry Configuration");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Download Retry Time (ms):");
        if (storage != null) {
          storage.MaxDownloadRetryTime = GUIDisplayTimespanMsField(storage.MaxDownloadRetryTime);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Operation Retry Time (ms):");
        if (storage != null) {
          storage.MaxOperationRetryTime = GUIDisplayTimespanMsField(storage.MaxOperationRetryTime);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Upload Retry Time (ms):");
        if (storage != null) {
          storage.MaxUploadRetryTime = GUIDisplayTimespanMsField(storage.MaxUploadRetryTime);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
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
