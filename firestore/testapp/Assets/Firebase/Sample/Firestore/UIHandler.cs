// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
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

    private const string DefaultDatabase = "(default)";

    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private string logText = "";
    private Vector2 scrollViewVector = Vector2.zero;
    protected bool UIEnabled = false;
    private GUIStyle disabledButtonStyle;

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

    private enum Backend {
      Production = 0,
      Emulator = 1
      // Nightly and stage can be added later.
    }

    private Backend TargetBackend = GetTargetBackend();
    // For local testing purpose, manually modify the port number if Firestore
    // emulator is running at a different port.
    private string EmulatorPort = "8080";

    private static Backend GetTargetBackend() {
      // Set Firestore emulator as default backend.
      Backend targetBackend = Backend.Emulator;

      // Use production backend if not running on Unity editor or standalone platform (Mac OS X,
      // Windows or Linux).
      #if !UNITY_EDITOR && !UNITY_STANDALONE
        targetBackend = Backend.Production;
      // Set custom script `RUN_AGAINST_PRODUCTION` to run tests against the productions. Refer to
      // unity documents for guidance:https://docs.unity3d.com/Manual/CustomScriptingSymbols.html
      #elif RUN_AGAINST_PRODUCTION
        targetBackend = Backend.Production;
      #endif 

      return targetBackend;
    }
    
    /**
     * Compares two objects for deep equality.
     */
    private bool ObjectDeepEquals(object left, object right) {
      if (left == right) {
        return true;
      } else if (left == null) {
        return right == null;
      } else if (left is IEnumerable && right is IEnumerable) {
        if (left is IDictionary && right is IDictionary) {
          return DictionaryDeepEquals(left as IDictionary, right as IDictionary);
        }
        return EnumerableDeepEquals(left as IEnumerable, right as IEnumerable);
      } else if (!left.GetType().Equals(right.GetType())) {
        return false;
      } else {
        return left.Equals(right);
      }
    }

    /**
     * Compares two IEnumerable for deep equality.
     */
    private bool EnumerableDeepEquals(IEnumerable left, IEnumerable right) {
      var leftEnumerator = left.GetEnumerator();
      var rightEnumerator = right.GetEnumerator();
      var leftNext = leftEnumerator.MoveNext();
      var rightNext = rightEnumerator.MoveNext();
      while (leftNext && rightNext) {
        if (!ObjectDeepEquals(leftEnumerator.Current, rightEnumerator.Current)) {
          return false;
        }
        leftNext = leftEnumerator.MoveNext();
        rightNext = rightEnumerator.MoveNext();
      }

      return leftNext == rightNext;
    }

    /**
     * Compares two dictionaries for deep equality.
     */
    private bool DictionaryDeepEquals(IDictionary left, IDictionary right) {
      if (left.Count != right.Count) return false;

      foreach (object key in left.Keys) {
        if (!right.Contains(key)) return false;

        if (!ObjectDeepEquals(left[key], right[key])) {
          return false;
        }
      }

      return true;
    }

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
    // to be thrown from the top level coroutine (e.g WriteDoc) we provide this
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

    protected internal FirebaseFirestore db {
      get {
        FirebaseFirestore firestore = FirebaseFirestore.DefaultInstance;
        SetTargetBackend(firestore);
        return firestore;
      }
    }

    protected internal FirebaseFirestore TestFirestore(FirebaseApp app) {
      FirebaseFirestore firestore = FirebaseFirestore.GetInstance(app);
      SetTargetBackend(firestore);
      return firestore; 
    }

    protected internal FirebaseFirestore TestFirestore(string database) {
      FirebaseFirestore firestore = FirebaseFirestore.GetInstance(database);
      SetTargetBackend(firestore);
      return firestore;
    }
    
    protected internal FirebaseFirestore TestFirestore(FirebaseApp app, string database) {
      FirebaseFirestore firestore = FirebaseFirestore.GetInstance(app, database);
      SetTargetBackend(firestore);
      return firestore;
    }
    
    // Check if Firestore emulator is the target backend.
    protected bool IsUsingFirestoreEmulator() {
        return (TargetBackend == Backend.Emulator);
    }

    // Update the `Settings` of a Firestore instance to run tests against the production or
    // Firestore emulator backend.
    protected internal void SetTargetBackend(FirebaseFirestore db) {
      string targetHost = GetTargetHost();

      // Avoid updating `Settings` if not required. No changes are allowed to be made to the
      // settings of a <c>FirebaseFirestore</c> instance if it has invoked any non-static method.
      /// Attempting to do so will result in an exception.
      if (db.Settings.Host == targetHost) {
        return;
      }

      db.Settings.Host = targetHost;
      // Emulator does not support ssl.
      db.Settings.SslEnabled = IsUsingFirestoreEmulator() ? false : true;
    }

    private string GetTargetHost() {
      if (IsUsingFirestoreEmulator()) {
        #if UNITY_ANDROID
          // Special IP to access the hosting OS from Android Emulator.
          string localHost = "10.0.2.2";
        #else
          string localHost = "localhost";
        #endif // UNITY_ANDROID
        return localHost + ":" + EmulatorPort;
      }

      return FirebaseFirestore.DefaultInstance.Settings.Host;
    }

    // Cancel the currently running operation.
    protected void CancelOperation() {
      if (operationInProgress && cancellationTokenSource != null) {
        DebugLog("*** Cancelling operation *** ...");
        cancellationTokenSource.Cancel();
        cancellationTokenSource = null;
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
      DebugLog("INFO: Going to write the following data to " + doc.Parent.Id + "/" + doc.Id + ":");
      DebugLog(DictToString(data));
      Task setTask = doc.SetAsync(data);
      yield return new WaitForTaskCompletion(this, setTask);
      if (setTask.IsCanceled) {
        DebugLog("INFO: Write operation was cancelled.");
      } else if (setTask.IsFaulted) {
        DebugLog("ERROR: " + setTask.Exception.ToString());
      } else {
        // Update the collectionPath/documentId because:
        // 1) If the documentId field was empty, this will fill it in with the autoid. This allows
        //    you to manually test via a trivial 'click set', 'click get'.
        // 2) In the automated test, the caller might pass in an explicit docRef rather than pulling
        //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
        //    for the automated tests.)
        collectionPath = doc.Parent.Id;
        documentId = doc.Id;

        DebugLog("INFO: Wrote data successfully.");
      }
    }

    private IEnumerator UpdateDoc(DocumentReference doc, IDictionary<string, object> data) {
      DebugLog("INFO: Going to update " + doc.Parent.Id + "/" + doc.Id + " with the following data:");
      DebugLog(DictToString(data));
      Task updateTask = doc.UpdateAsync(data);
      yield return new WaitForTaskCompletion(this, updateTask);
      if (updateTask.IsCanceled) {
        DebugLog("INFO: Update operation was cancelled.");
      } else if (updateTask.IsFaulted) {
        DebugLog("ERROR: " + updateTask.Exception.ToString());
      } else {
        // Update the collectionPath/documentId because:
        // 1) In the automated test, the caller might pass in an explicit docRef rather than pulling
        //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
        //    for the automated tests.)
        collectionPath = doc.Parent.Id;
        documentId = doc.Id;

        DebugLog("INFO: Document updated successfully.");
      }
    }

    private IEnumerator ReadDoc(DocumentReference doc) {
      DebugLog("INFO: Going to read the document " + doc.Parent.Id + "/" + doc.Id + ":");

      Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, getTask);
      if (getTask.IsCanceled) {
        DebugLog("INFO: Read operation was cancelled.");
      } else if (getTask.IsFaulted) {
        DebugLog("ERROR: " + getTask.Exception.ToString());
      } else {
        DocumentSnapshot snap = getTask.Result;
        if (!snap.Exists) {
          DebugLog("INFO: Document does not exist.");
        } else {
          IDictionary<string, object> resultData = snap.ToDictionary();
          if(resultData != null && resultData.Count > 0) {
            DebugLog("INFO: Read document contents:");
            DebugLog(DictToString(resultData));
          } else {
            DebugLog("INFO: Document was empty.");
          }
        }
      }
    }

    // Perform a batch write.
    private IEnumerator PerformBatchWrite() {
      DocumentReference doc1 = db.Collection("col2").Document("batch_doc1");
      DocumentReference doc2 = db.Collection("col2").Document("batch_doc2");
      DocumentReference doc3 = db.Collection("col2").Document("batch_doc3");

      // Initialize doc1 and doc2 with some data.
      var initialData = new Dictionary<string, object>{
        {"field", "value"},
      };
      yield return new WaitForTaskCompletion(this, doc1.SetAsync(initialData));
      yield return new WaitForTaskCompletion(this, doc2.SetAsync(initialData));

      // Perform batch that deletes doc1, updates doc2, and overwrites doc3.
      DebugLog("INFO: Going to perform the following three operations in a batch:");
      DebugLog("\tDelete col2/batch_doc1");
      DebugLog("\tUpdate col2/batch_doc2");
      DebugLog("\tOverwrite col2/batch_doc3");
      yield return new WaitForTaskCompletion(this, doc1.Firestore.StartBatch()
          .Delete(doc1)
          .Update(doc2, new Dictionary<string, object> { { "field2", "value2" } })
          .Update(doc2, new Dictionary<FieldPath, object> { { new FieldPath("field3"), "value3" } })
          .Update(doc2, "field4", "value4")
          .Set(doc3, initialData)
          .CommitAsync());

      DebugLog("INFO: Batch operation completed.");

      DebugLog("INFO: Checking the resulting documents.");

      Task<DocumentSnapshot> get1 = doc1.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, get1);
      if (get1.IsCanceled) {
        DebugLog("INFO: Read operation for batch_doc1 was cancelled.");
      } else if (get1.IsFaulted) {
        DebugLog("ERROR: An error occurred while retrieving col2/batch_doc1.");
        DebugLog("ERROR: " + get1.Exception.ToString());
      } else {
        DocumentSnapshot snap = get1.Result;
        if(snap.Exists) {
          DebugLog("ERROR: col2/batch_doc1 should have been deleted.");
        } else {
          DebugLog("Success: col2/batch_doc1 does not exist.");
        }
      }

      Task<DocumentSnapshot> get2 = doc2.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, get2);
      if (get2.IsCanceled) {
        DebugLog("INFO: Read operation for batch_doc2 was cancelled.");
      } else if (get2.IsFaulted) {
        DebugLog("ERROR: An error occurred while retrieving col2/batch_doc2.");
        DebugLog("ERROR: " + get2.Exception.ToString());
      } else {
        DocumentSnapshot snap = get2.Result;
        if(snap.Exists) {
          bool deepEquals = ObjectDeepEquals(snap.ToDictionary(),
          new Dictionary<string, object> {
            { "field", "value" },
            { "field2", "value2" },
            { "field3", "value3" },
            { "field4", "value4" },
          });
          if(deepEquals) {
            DebugLog("Success: col2/batch_doc2 content is as expected.");
          } else {
            DebugLog("ERROR: col2/batch_doc2 has incorrect content.");
          }
        } else {
          DebugLog("ERROR: col2/batch_doc2 does not exist.");
        }
      }

      Task<DocumentSnapshot> get3 = doc3.GetSnapshotAsync();
      yield return new WaitForTaskCompletion(this, get3);
      if (get3.IsCanceled) {
        DebugLog("INFO: Read operation for batch_doc3 was cancelled.");
      } else if (get3.IsFaulted) {
        DebugLog("ERROR: An error occurred while retrieving col2/batch_doc3.");
        DebugLog("ERROR: " + get3.Exception.ToString());
      } else {
        DocumentSnapshot snap = get3.Result;
        if(snap.Exists) {
          bool deepEquals = ObjectDeepEquals(snap.ToDictionary(), initialData);
          if(deepEquals) {
            DebugLog("Success: col2/batch_doc3 content is as expected.");
          } else {
            DebugLog("ERROR: col2/batch_doc3 has incorrect content.");
          }
        } else {
          DebugLog("ERROR: col2/batch_doc3 does not exist.");
        }
      }
    }

    private IEnumerator PerformTransaction() {
      DocumentReference doc1 = db.Collection("col3").Document("txn_doc1");
      DocumentReference doc2 = db.Collection("col3").Document("txn_doc2");
      DocumentReference doc3 = db.Collection("col3").Document("txn_doc3");

      // Initialize doc1 and doc2 with some data.
      var initialData = new Dictionary<string, object>{
        {"field", "value"},
      };
      yield return new WaitForTaskCompletion(this, doc1.SetAsync(initialData));
      yield return new WaitForTaskCompletion(this, doc2.SetAsync(initialData));

      // Perform transaction that reads doc1, deletes doc1, updates doc2, and overwrites doc3.
      DebugLog("INFO: Going to perform the following three operations in a transaction:");
      DebugLog("\tDelete col3/txn_doc1");
      DebugLog("\tUpdate col3/txn_doc2");
      DebugLog("\tOverwrite col3/txn_doc3");
      var txnTask = doc1.Firestore.RunTransactionAsync<string>((transaction) => {
        return transaction.GetSnapshotAsync(doc1).ContinueWith((getTask) => {
          transaction.Delete(doc1);
          transaction.Update(doc2, new Dictionary<string, object> { { "field2", "value2" } });
          transaction.Update(doc2, new Dictionary<FieldPath, object> { { new FieldPath("field3"), "value3" } });
          transaction.Update(doc2, "field4", "value4");
          transaction.Set(doc3, initialData);
          // Since RunTransactionAsync<string> is used, we can return a string here, which can be
          // accessed via Task.Result when the task is completed.
          return "SUCCESS";
        });
      });
      yield return new WaitForTaskCompletion(this, txnTask);
      if (txnTask.IsCanceled) {
        DebugLog("INFO: Transaction operation was cancelled.");
      } else if (txnTask.IsFaulted) {
        DebugLog("ERROR: An error occurred while performing the transaction.");
        DebugLog("ERROR: " + txnTask.Exception.ToString());
      } else {
        string result = txnTask.Result;
        DebugLog("INFO: Transaction completed with status: " + result);
      }

      if (!(txnTask.IsFaulted || txnTask.IsCanceled)) {
        DebugLog("INFO: Checking the resulting documents.");

        Task<DocumentSnapshot> get1 = doc1.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(this, get1);
        if (get1.IsCanceled) {
          DebugLog("INFO: Read operation for txn_doc1 was cancelled.");
        } else if (get1.IsFaulted) {
          DebugLog("ERROR: An error occurred while retrieving col3/txn_doc1.");
          DebugLog("ERROR: " + get1.Exception.ToString());
        } else {
          DocumentSnapshot snap = get1.Result;
          if(snap.Exists) {
            DebugLog("ERROR: col3/txn_doc1 should have been deleted.");
          } else {
            DebugLog("Success: col3/txn_doc1 does not exist.");
          }
        }

        Task<DocumentSnapshot> get2 = doc2.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(this, get2);
        if (get2.IsCanceled) {
          DebugLog("INFO: Read operation for txn_doc2 was cancelled.");
        } else if (get2.IsFaulted) {
          DebugLog("ERROR: An error occurred while retrieving col3/txn_doc2.");
          DebugLog("ERROR: " + get2.Exception.ToString());
        } else {
          DocumentSnapshot snap = get2.Result;
          if(snap.Exists) {
            bool deepEquals = ObjectDeepEquals(snap.ToDictionary(),
            new Dictionary<string, object> {
              { "field", "value" },
              { "field2", "value2" },
              { "field3", "value3" },
              { "field4", "value4" },
            });
            if(deepEquals) {
              DebugLog("Success: col3/txn_doc2 content is as expected.");
            } else {
              DebugLog("ERROR: col3/txn_doc2 has incorrect content.");
            }
          } else {
            DebugLog("ERROR: col3/txn_doc2 does not exist.");
          }
        }

        Task<DocumentSnapshot> get3 = doc3.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(this, get3);
        if (get3.IsCanceled) {
          DebugLog("INFO: Read operation for txn_doc3 was cancelled.");
        } else if (get3.IsFaulted) {
          DebugLog("ERROR: An error occurred while retrieving col3/txn_doc3");
          DebugLog("ERROR: " + get3.Exception.ToString());
        } else {
          DocumentSnapshot snap = get3.Result;
          if(snap.Exists) {
            bool deepEquals = ObjectDeepEquals(snap.ToDictionary(), initialData);
            if(deepEquals) {
              DebugLog("Success: col3/txn_doc3 content is as expected.");
            } else {
              DebugLog("ERROR: col3/txn_doc3 has incorrect content.");
            }
          } else {
            DebugLog("ERROR: col3/txn_doc3 does not exist.");
          }
        }
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

        GUILayout.Space(20);

        GUILayout.BeginVertical();

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

        if (Button("Perform Batch Write", !operationInProgress)) {
          StartCoroutine(PerformBatchWrite());
        }

        if (Button("Perform Transaction", !operationInProgress)) {
          StartCoroutine(PerformTransaction());
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