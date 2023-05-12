// Copyright 2021 Google LLC
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

using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Sample.Firestore;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Tests.TestAsserts;

namespace Tests {
  // Tests against features offered from the root Firestore instance.
  public class FirestoreInstanceTests : FirestoreIntegrationTests {
    [UnityTest]
    public IEnumerator Firestore_ShouldIntegrateWithAuth() {
      var firebaseAuth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      var data = TestData();

      firebaseAuth.SignOut();
      DocumentReference doc = db.Collection("private").Document();
      var setTask = doc.SetAsync(data);
      yield return AwaitFaults(setTask);
      AssertTaskFaulted(setTask, FirestoreError.PermissionDenied);

      try {
        yield return AwaitSuccess(firebaseAuth.SignInAnonymouslyAsync());

        // Write should now succeed.
        yield return AwaitSuccess(doc.SetAsync(data));

      } finally {
        // Make sure we signed out at the end of the test.
        firebaseAuth.SignOut();
      }
    }

    [UnityTest]
    public IEnumerator WaitForPendingWrites_ShouldWork() {
      DocumentReference doc = TestDocument();

      yield return AwaitSuccess(doc.Firestore.DisableNetworkAsync());

      // `pendingWrites1` completes without network because there are no pending writes at
      // the time it is created.
      var pendingWrites1 = doc.Firestore.WaitForPendingWritesAsync();
      yield return AwaitSuccess(pendingWrites1);

      doc.SetAsync(new Dictionary<string, object> { { "zip", 98101 } });
      // `pendingWrites2` will be pending because the SetAsync above is not acknowledged by the
      // backend yet.
      var pendingWrites2 = doc.Firestore.WaitForPendingWritesAsync();
      AssertTaskIsPending(pendingWrites2);

      yield return AwaitSuccess(doc.Firestore.EnableNetworkAsync());
      yield return AwaitSuccess(pendingWrites2);
    }

    // TODO(b/183603381): Add a test to verify WaitForPendingWritesAsync task fails in user change.
    // It requires to create underlying firestore instance with
    // a MockCredentialProvider first.

    [UnityTest]
    public IEnumerator Terminate_ShouldWork() {
      var appOptions = new AppOptions();
      appOptions.ProjectId = "test-terminate";  // Setting a ProjectId is required (b/158838266).
      FirebaseApp app = FirebaseApp.Create(appOptions, "App1");
      var db1 = FirebaseFirestore.GetInstance(app);
      var doc = db1.Document("abc/123");
      var accumulator = new EventAccumulator<DocumentSnapshot>();
      var registration = doc.Listen(accumulator.Listener);

      // Multiple calls to terminate should go through.
      yield return AwaitSuccess(db1.TerminateAsync());
      yield return AwaitSuccess(db1.TerminateAsync());

      // Make sure calling registration.Stop multiple times after termination works.
      registration.Stop();
      registration.Stop();

      // TODO(b/149105903) Uncomment this line when a C# exception can be thrown here.
      // Assert.Throws<NullReferenceException>(() => db1.DisableNetworkAsync());

      // Create a new functional instance.
      var db2 = FirebaseFirestore.GetInstance(app);
      Assert.That(db2, Is.Not.SameAs(db1));
      yield return AwaitSuccess(db2.DisableNetworkAsync());
      yield return AwaitSuccess(db2.EnableNetworkAsync());

      app.Dispose();
      // TODO(b/183604785): App.Dispose really should leads to Firestore terminated, a NRE here is
      // not ideal, but serves the purpose for now. Ideally, it should throw an exception
      // telling user it is terminated.
      Assert.Throws<NullReferenceException>(() => db2.DisableNetworkAsync());
    }

    [UnityTest]
    public IEnumerator ClearPersistence_ShouldWork() {
      var defaultOptions = db.App.Options;
      string path;

      // Verify that ClearPersistenceAsync() succeeds when invoked on a newly-created
      // FirebaseFirestore instance.
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        yield return AwaitSuccess(db.ClearPersistenceAsync());
        app.Dispose();
      }

      // Create a document to use to verify the behavior of ClearPersistenceAsync().
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        var docContents = new Dictionary<string, object> { { "foo", 42 } };

        var doc = db.Collection("TestCollection").Document();
        path = doc.Path;
        // It is not necessary to Await on the Task returned from SetAsync() below. This is
        // because the document has already been written to persistence by the time that
        // SetAsync() returns.
        doc.SetAsync(docContents);
        yield return AwaitSuccess(db.TerminateAsync());
        app.Dispose();
      }

      // As a sanity check, verify that the document created in the previous block exists.
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        var doc = db.Document(path);
        yield return AwaitSuccess(doc.GetSnapshotAsync(Source.Cache));
        app.Dispose();
      }

      // Call ClearPersistenceAsync() after TerminateAsync().
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        yield return AwaitSuccess(db.TerminateAsync());
        yield return AwaitSuccess(db.ClearPersistenceAsync());
        app.Dispose();
      }

      // Verify that ClearPersistenceAsync() deleted the document that was created above.
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        var doc = db.Document(path);
        var getTask = doc.GetSnapshotAsync(Source.Cache);
        yield return AwaitCompletion(getTask);
        AssertTaskFaulted(getTask, FirestoreError.Unavailable);
        yield return AwaitSuccess(db.TerminateAsync());
        app.Dispose();
      }

      // Verify that ClearPersistenceAsync() fails if invoked after the first operation and
      // before a call to TerminateAsync().
      {
        var app = FirebaseApp.Create(defaultOptions, "TestClearPersistenceApp");
        var db = FirebaseFirestore.GetInstance(app);
        yield return AwaitSuccess(db.EnableNetworkAsync());
        var clearTask = db.ClearPersistenceAsync();
        yield return AwaitCompletion(clearTask);
        AssertTaskFaulted(clearTask, FirestoreError.FailedPrecondition);
        yield return AwaitSuccess(db.TerminateAsync());
        app.Dispose();
      }
    }

    [UnityTest]
    public IEnumerator FirestoreSettings_ShouldWork() {
      FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
      DocumentReference doc = db.Collection("coll").Document();
      var data = new Dictionary<string, object> {
        { "f1", "v1" },
      };
      yield return AwaitSuccess(doc.SetAsync(data));

      // Verify it can get snapshot from cache. This is the behavior when
      // persistence is set to true in settings, which is the default setting.
      yield return AwaitSuccess(doc.GetSnapshotAsync(Source.Cache));

      // Terminate the current instance and create a new one (via DefaultInstance) such that
      // we can apply a new FirebaseFirestoreSettings.
      yield return AwaitSuccess(db.TerminateAsync());
      db = FirebaseFirestore.DefaultInstance;
      db.Settings.PersistenceEnabled = false;

      doc = db.Collection("coll").Document();
      yield return AwaitSuccess(doc.SetAsync(data));

      // Verify it cannot get snapshot from cache. This behavior only exists with memory
      // persistence.
      var getTask = doc.GetSnapshotAsync(Source.Cache);
      yield return AwaitCompletion(getTask);
      AssertTaskFaulted(getTask, FirestoreError.Unavailable);

      // Restart SDK again to test mutating existing settings.
      yield return AwaitSuccess(db.TerminateAsync());
      db = FirebaseFirestore.DefaultInstance;
      Assert.That(db.Settings.PersistenceEnabled, Is.True);
      db.Settings.PersistenceEnabled = false;

      doc = db.Collection("coll").Document();
      data = new Dictionary<string, object> {
        { "f1", "v1" },
      };
      yield return AwaitSuccess(doc.SetAsync(data));

      // Verify it cannot get snapshot from cache. This behavior only exists with memory
      // persistence.
      getTask = doc.GetSnapshotAsync(Source.Cache);
      yield return AwaitCompletion(getTask);
      AssertTaskFaulted(getTask, FirestoreError.Unavailable);

      yield return AwaitSuccess(db.TerminateAsync());
      db = FirebaseFirestore.DefaultInstance;

      // There is no way to actually verify the cache size is applied, we simply
      // verify the size is set properly in the settings object.
      long fiveMb = 5 * 1024 * 1024;
      db.Settings.CacheSizeBytes = fiveMb;
      Assert.That(db.Settings.CacheSizeBytes, Is.EqualTo(fiveMb));
    }
  }
}
