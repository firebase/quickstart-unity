using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.TestTools;
using Firebase.Sample.Firestore;
using NUnit.Framework;
using UnityEngine;
using static Tests.TestAsserts;

namespace Tests {
  public class QueryAndListenerTests : FirestoreIntegrationTests {
    [UnityTest]
    public IEnumerator TestListenForSnapshotsInSync() {
      var events = new List<string>();

      var doc = TestDocument();
      var docAccumulator = new EventAccumulator<DocumentSnapshot>(mainThreadId);
      var docListener = doc.Listen(snapshot => {
        events.Add("doc");
        docAccumulator.Listener(snapshot);
      });

      yield return AwaitSuccess(doc.SetAsync(TestData(1)));

      yield return AwaitSuccess(docAccumulator.LastEventAsync());
      events.Clear();

      var syncAccumulator = new EventAccumulator<string>();
      var syncListener = doc.Firestore.ListenForSnapshotsInSync(() => {
        events.Add("sync");
        syncAccumulator.Listener("sync");
      });

      // Ensure that the Task from the ListenerRegistration is in the correct state.
      AssertTaskIsPending(syncListener.ListenerTask);

      yield return AwaitSuccess(doc.SetAsync(TestData(2)));

      yield return AwaitSuccess(docAccumulator.LastEventAsync());
      yield return AwaitSuccess(syncAccumulator.LastEventsAsync(2));

      var expectedEvents = new List<string> {
        "sync",  // Initial in-sync event
        "doc",   // From the Set()
        "sync"   // Another in-sync event
      };

      docListener.Stop();
      syncListener.Stop();
      yield return AwaitSuccess(syncListener.ListenerTask);
      Assert.That(events, Is.EquivalentTo(expectedEvents));
    }

    [UnityTest]
    public IEnumerator TestMultiInstanceDocumentReferenceListeners() {
      var db1Doc = TestDocument();
      var db1 = db1Doc.Firestore;
      var app1 = db1.App;

      var app2 = FirebaseApp.Create(app1.Options, "MultiInstanceDocumentReferenceListenersTest");
      var db2 = FirebaseFirestore.GetInstance(app2);
      var db2Doc = db2.Collection(db1Doc.Parent.Id).Document(db1Doc.Id);

      var db1DocAccumulator = new EventAccumulator<DocumentSnapshot>();
      db1Doc.Listen(db1DocAccumulator.Listener);
      yield return AwaitSuccess(db1DocAccumulator.LastEventAsync());

      var db2DocAccumulator = new EventAccumulator<DocumentSnapshot>();
      db2Doc.Listen(db2DocAccumulator.Listener);
      yield return AwaitSuccess(db2DocAccumulator.LastEventAsync());

      // At this point we have two firestore instances and separate listeners attached to each one
      // and all are in an idle state. Once the second instance is disposed the listeners on the
      // first instance should continue to operate normally and the listeners on the second instance
      // should not receive any more events.
      db2DocAccumulator.ThrowOnAnyEvent();

      app2.Dispose();
      yield return AwaitFaults(db2Doc.SetAsync(TestData(3)));

      yield return AwaitSuccess(db1Doc.SetAsync(TestData(3)));
      yield return AwaitSuccess(db1DocAccumulator.LastEventAsync());
    }

    [UnityTest]
    public IEnumerator TestMultiInstanceQueryListeners() {
      var db1Collection = TestCollection();
      var db1 = db1Collection.Firestore;
      var app1 = db1.App;

      var app2 = FirebaseApp.Create(app1.Options, "MultiInstanceQueryListenersTest");
      var db2 = FirebaseFirestore.GetInstance(app2);
      var db2Collection = db2.Collection(db1Collection.Id);

      var db1CollectionAccumulator = new EventAccumulator<QuerySnapshot>();
      db1Collection.Listen(db1CollectionAccumulator.Listener);
      yield return AwaitSuccess(db1CollectionAccumulator.LastEventAsync());

      var db2CollectionAccumulator = new EventAccumulator<QuerySnapshot>();
      db2Collection.Listen(db2CollectionAccumulator.Listener);
      yield return AwaitSuccess(db2CollectionAccumulator.LastEventAsync());

      // At this point we have two firestore instances and separate listeners
      // attached to each one and all are in an idle state. Once the second
      // instance is disposed the listeners on the first instance should
      // continue to operate normally and the listeners on the second
      // instance should not receive any more events.

      db2CollectionAccumulator.ThrowOnAnyEvent();

      app2.Dispose();
      yield return AwaitFaults(db2Collection.Document().SetAsync(TestData(1)));

      yield return AwaitSuccess(db1Collection.Document().SetAsync(TestData(1)));
      yield return AwaitSuccess(db1CollectionAccumulator.LastEventAsync());
    }

    [UnityTest]
    public IEnumerator TestMultiInstanceSnapshotsInSyncListeners() {
      var db1Doc = TestDocument();
      var db1 = db1Doc.Firestore;
      var app1 = db1.App;

      var app2 = FirebaseApp.Create(app1.Options, "MultiInstanceSnapshotsInSyncTest");
      var db2 = FirebaseFirestore.GetInstance(app2);
      var db2Doc = db2.Collection(db1Doc.Parent.Id).Document(db1Doc.Id);

      var db1SyncAccumulator = new EventAccumulator<string>();
      var db1SyncListener =
          db1.ListenForSnapshotsInSync(() => { db1SyncAccumulator.Listener("db1 in sync"); });
      yield return AwaitSuccess(db1SyncAccumulator.LastEventAsync());

      var db2SyncAccumulator = new EventAccumulator<string>();
      db2.ListenForSnapshotsInSync(() => { db2SyncAccumulator.Listener("db2 in sync"); });
      yield return AwaitSuccess(db2SyncAccumulator.LastEventAsync());

      db1Doc.Listen((snap) => {});
      yield return AwaitSuccess(db1SyncAccumulator.LastEventAsync());

      db2Doc.Listen((snap) => {});
      yield return AwaitSuccess(db2SyncAccumulator.LastEventAsync());

      // At this point we have two firestore instances and separate listeners
      // attached to each one and all are in an idle state. Once the second
      // instance is disposed the listeners on the first instance should
      // continue to operate normally and the listeners on the second
      // instance should not receive any more events.

      db2SyncAccumulator.ThrowOnAnyEvent();

      app2.Dispose();
      yield return AwaitFaults(db2Doc.SetAsync(TestData(2)));

      yield return AwaitSuccess(db1Doc.SetAsync(TestData(2)));
      yield return AwaitSuccess(db1SyncAccumulator.LastEventAsync());
    }

    [UnityTest]
    public IEnumerator TestDocumentSnapshot() {
      DocumentReference doc = db.Collection("col2").Document();
      var data = TestData();

      yield return AwaitSuccess(doc.SetAsync(data));
      var task = doc.GetSnapshotAsync();
      yield return AwaitSuccess(task);
      var snap = task.Result;

      VerifyDocumentSnapshotGetValueWorks(snap, data);
      VerifyDocumentSnapshotTryGetValueWorks(snap, data);
      VerifyDocumentSnapshotContainsFieldWorks(snap, data);
    }

    private void VerifyDocumentSnapshotGetValueWorks(DocumentSnapshot snap,
                                                     Dictionary<string, object> data) {
      Assert.That(snap.GetValue<string>("name"), Is.EqualTo(data["name"]));
      Assert.That(snap.GetValue<Dictionary<string, object>>("metadata"),
                  Is.EquivalentTo(data["metadata"] as IEnumerable),
                  "Resulting data.metadata does not match.");
      Assert.That(snap.GetValue<string>("metadata.deep.field"), Is.EqualTo("deep-field-1"));
      Assert.That(snap.GetValue<string>(new FieldPath("metadata", "deep", "field")),
                  Is.EqualTo("deep-field-1"));
      // Nonexistent field.
      Assert.Throws(typeof(InvalidOperationException), () => snap.GetValue<object>("nonexistent"));
      // Existent field deserialized to wrong type.
      Assert.Throws(typeof(ArgumentException), () => snap.GetValue<long>("name"));
    }

    private void VerifyDocumentSnapshotTryGetValueWorks(DocumentSnapshot snap,
                                                        Dictionary<string, object> data) {
      // Existent field.
      String name;
      Assert.That(snap.TryGetValue<string>("name", out name), Is.True);
      Assert.That(name, Is.EqualTo(data["name"]));

      // Nonexistent field.
      Assert.That(snap.TryGetValue<string>("namex", out name), Is.False);
      Assert.That(name, Is.Null);

      // Existent field deserialized to wrong type.
      Assert.Throws(typeof(ArgumentException), () => {
        long l;
        snap.TryGetValue<long>("name", out l);
      });
    }

    private void VerifyDocumentSnapshotContainsFieldWorks(DocumentSnapshot snap,
                                                          Dictionary<string, object> data) {
      // Existent fields.
      Assert.That(snap.ContainsField("name"), Is.True);
      Assert.That(snap.ContainsField("metadata.deep.field"), Is.True);
      Assert.That(snap.ContainsField(new FieldPath("metadata", "deep", "field")), Is.True);

      // Nonexistent field.
      Assert.That(snap.ContainsField("namex"), Is.False);
    }

    [UnityTest]
    public IEnumerator TestDocumentSnapshotServerTimestampBehavior() {
      DocumentReference doc = db.Collection("col2").Document();

      // Disable network so we can test unresolved server timestamp behavior.
      yield return AwaitSuccess(doc.Firestore.DisableNetworkAsync());

      doc.SetAsync(new Dictionary<string, object> { { "timestamp", "prev" } });
      doc.SetAsync(new Dictionary<string, object> { { "timestamp", FieldValue.ServerTimestamp } });

      Task<DocumentSnapshot> task = doc.GetSnapshotAsync();
      yield return AwaitSuccess(task);
      var snap = task.Result;

      // Default / None should return null.
      Assert.That(snap.ToDictionary()["timestamp"], Is.Null);
      Assert.That(snap.GetValue<object>("timestamp"), Is.Null);
      Assert.That(snap.ToDictionary(ServerTimestampBehavior.None)["timestamp"], Is.Null);
      Assert.That(snap.GetValue<object>("timestamp", ServerTimestampBehavior.None), Is.Null);

      // Previous should be "prev"
      Assert.That(snap.ToDictionary(ServerTimestampBehavior.Previous)["timestamp"],
                  Is.EqualTo("prev"));
      Assert.That(snap.GetValue<object>("timestamp", ServerTimestampBehavior.Previous),
                  Is.EqualTo("prev"));

      // Estimate should be a timestamp.
      Assert.That(snap.ToDictionary(ServerTimestampBehavior.Estimate)["timestamp"],
                  Is.TypeOf(typeof(Timestamp)), "Estimate should be a Timestamp");
      Assert.That(snap.GetValue<object>("timestamp", ServerTimestampBehavior.Estimate),
                  Is.TypeOf(typeof(Timestamp)), "Estimate should be a Timestamp");

      yield return AwaitSuccess(doc.Firestore.EnableNetworkAsync());
    }

    [UnityTest]
    public IEnumerator TestDocumentSnapshotIntegerIncrementBehavior() {
      DocumentReference doc = TestDocument();

      var data = TestData();
      yield return AwaitSuccess(doc.SetAsync(data));

      var incrementValue = FieldValue.Increment(1L);

      var updateData = new Dictionary<string, object> { { "metadata.createdAt", incrementValue } };

      yield return AwaitSuccess(doc.UpdateAsync(updateData));
      var t = doc.GetSnapshotAsync();
      yield return AwaitSuccess(t);
      DocumentSnapshot snap = t.Result;

      var expected = TestData();
      ((Dictionary<string, object>)expected["metadata"])["createdAt"] = 2L;

      Assert.That(snap.ToDictionary(), Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestDocumentSnapshotDoubleIncrementBehavior() {
      DocumentReference doc = TestDocument();

      var data = TestData();
      yield return AwaitSuccess(doc.SetAsync(data));

      var incrementValue = FieldValue.Increment(1.5);

      var updateData = new Dictionary<string, object> { { "metadata.createdAt", incrementValue } };

      yield return AwaitSuccess(doc.UpdateAsync(updateData));
      var t = doc.GetSnapshotAsync();
      yield return AwaitSuccess(t);
      DocumentSnapshot snap = t.Result;

      var expected = TestData();
      ((Dictionary<string, object>)expected["metadata"])["createdAt"] = 2.5;

      Assert.That(snap.ToDictionary(), Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestDocumentListen() {
      var doc = TestDocument();
      var initialData = TestData(1);
      var newData = TestData(2);

      yield return AwaitSuccess(doc.SetAsync(initialData));

      var accumulator = new EventAccumulator<DocumentSnapshot>(mainThreadId);
      var registration = doc.Listen(accumulator.Listener);

      // Ensure that the Task from the ListenerRegistration is in the correct state.
      AssertTaskIsPending(registration.ListenerTask);

      // Wait for the first snapshot.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(initialData));
        Assert.That(snapshot.Metadata.IsFromCache, Is.True);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      // Write new data and wait for the resulting snapshot.
      {
        doc.SetAsync(newData);
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(newData));
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.True);
      }

      {
        // Remove the listener and make sure we don't get events anymore.
        accumulator.ThrowOnAnyEvent();
        registration.Stop();
        AssertTaskSucceeded(registration.ListenerTask);
        yield return AwaitSuccess(doc.SetAsync(TestData(3)));

        // Ensure that the Task from the ListenerRegistration correctly fails with an error.
        var docWithInvalidName = TestCollection().Document("__badpath__");
        var callbackInvoked = false;
        var registration2 = docWithInvalidName.Listen(snap => { callbackInvoked = true; });
        yield return AwaitCompletion(registration2.ListenerTask);
        AssertTaskFaulted(registration2.ListenerTask, FirestoreError.InvalidArgument,
                          "__badpath__");
        registration2.Stop();
        Thread.Sleep(50);
        Assert.That(callbackInvoked, Is.False);
      }
    }

    [UnityTest]
    public IEnumerator DocumentSnapshot_ShouldReturnCorrectMetadataChanges() {
      var doc = TestDocument();
      var initialData = TestData(1);
      var newData = TestData(2);

      yield return AwaitSuccess(doc.SetAsync(initialData));

      var accumulator = new EventAccumulator<DocumentSnapshot>();
      var registration = doc.Listen(MetadataChanges.Include, accumulator.Listener);

      // Ensure that the Task from the ListenerRegistration is in the correct state.
      AssertTaskIsPending(registration.ListenerTask);

      // Wait for the first snapshot.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(initialData));
        Assert.That(snapshot.Metadata.IsFromCache, Is.True);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      // Wait for new snapshot once we're synced with the backend.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(initialData));
        Assert.That(snapshot.Metadata.IsFromCache, Is.False);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      // Write new data and wait for the resulting snapshot.
      {
        doc.SetAsync(newData);
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(newData));
        Assert.That(snapshot.Metadata.IsFromCache, Is.False);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.True);
      }

      // Wait for new snapshot once write completes.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        DocumentSnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      {
        // Remove the listener and make sure we don't get events anymore.
        accumulator.ThrowOnAnyEvent();
        registration.Stop();
        AssertTaskSucceeded(registration.ListenerTask);
        yield return AwaitSuccess(doc.SetAsync(TestData(3)));

        // Ensure that the Task from the ListenerRegistration correctly fails with an error.
        var docWithInvalidName = TestCollection().Document("__badpath__");
        var callbackInvoked = false;
        var registration2 =
            docWithInvalidName.Listen(MetadataChanges.Include, snap => { callbackInvoked = true; });
        yield return AwaitCompletion(registration2.ListenerTask);
        AssertTaskFaulted(registration2.ListenerTask, FirestoreError.InvalidArgument,
                          "__badpath__");
        registration2.Stop();
        Thread.Sleep(50);
        Assert.That(callbackInvoked, Is.False);
      }
    }

    [UnityTest]
    public IEnumerator QuerySnapshot_ShouldReturnCorrectData() {
      var collection = TestCollection();
      var data1 = TestData(1);
      var data2 = TestData(2);

      yield return AwaitSuccess(collection.Document("a").SetAsync(data1));

      var accumulator = new EventAccumulator<QuerySnapshot>(mainThreadId);
      var registration = collection.Listen(accumulator.Listener);

      // Ensure that the Task from the ListenerRegistration is in the correct state.
      AssertTaskIsPending(registration.ListenerTask);

      // Wait for the first snapshot.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Count, Is.EqualTo(1));
        Assert.That(snapshot[0].ToDictionary(), Is.EquivalentTo(data1));
        Assert.That(snapshot.Metadata.IsFromCache, Is.True);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      // Write a new document and wait for the resulting snapshot.
      {
        collection.Document("b").SetAsync(data2);
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Count, Is.EqualTo(2));
        Assert.That(snapshot[0].ToDictionary(), Is.EquivalentTo(data1));
        Assert.That(snapshot[1].ToDictionary(), Is.EquivalentTo(data2));
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.True);
      }

      {
        // Remove the listener and make sure we don't get events anymore
        accumulator.ThrowOnAnyEvent();
        registration.Stop();
        AssertTaskSucceeded(registration.ListenerTask);
        yield return AwaitSuccess(collection.Document("c").SetAsync(TestData(3)));

        // Ensure that the Task from the ListenerRegistration correctly fails with an error.
        var collectionWithInvalidName = TestCollection().Document("__badpath__").Collection("sub");
        var callbackInvoked = false;
        var registration2 = collectionWithInvalidName.Listen(snap => { callbackInvoked = true; });
        yield return AwaitCompletion(registration2.ListenerTask);
        AssertTaskFaulted(registration2.ListenerTask, FirestoreError.InvalidArgument,
                          "__badpath__");
        registration2.Stop();
        Thread.Sleep(50);
        Assert.That(callbackInvoked, Is.False);
      }
    }

    [UnityTest]
    public IEnumerator QuerySnapshot_ShouldReturnCorrectMetadata() {
      var collection = TestCollection();

      var data1 = TestData(1);
      var data2 = TestData(2);

      yield return AwaitSuccess(collection.Document("a").SetAsync(data1));

      var accumulator = new EventAccumulator<QuerySnapshot>();
      var registration = collection.Listen(MetadataChanges.Include, accumulator.Listener);

      // Ensure that the Task from the ListenerRegistration is in the correct state.
      AssertTaskIsPending(registration.ListenerTask);

      // Wait for the first snapshot.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Count, Is.EqualTo(1));
        Assert.That(snapshot[0].ToDictionary(), Is.EquivalentTo(data1));
        Assert.That(snapshot.Metadata.IsFromCache, Is.True);
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      // Wait for new snapshot once we're synced with the backend.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Metadata.IsFromCache, Is.False);
      }

      // Write a new document and wait for the resulting snapshot.
      {
        collection.Document("b").SetAsync(data2);
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Count, Is.EqualTo(2));
        Assert.That(snapshot[0].ToDictionary(), Is.EquivalentTo(data1));
        Assert.That(snapshot[1].ToDictionary(), Is.EquivalentTo(data2));
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.True);
      }

      // Wait for new snapshot once write completes.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);
      }

      {
        // Remove the listener and make sure we don't get events anymore.
        accumulator.ThrowOnAnyEvent();
        registration.Stop();
        AssertTaskSucceeded(registration.ListenerTask);
        yield return AwaitSuccess(collection.Document("c").SetAsync(TestData(3)));

        // Ensure that the Task from the ListenerRegistration correctly fails with an error.
        var collectionWithInvalidName = TestCollection().Document("__badpath__").Collection("sub");
        var callbackInvoked = false;
        var registration2 = collectionWithInvalidName.Listen(MetadataChanges.Include,
                                                             snap => { callbackInvoked = true; });
        yield return AwaitCompletion(registration2.ListenerTask);
        AssertTaskFaulted(registration2.ListenerTask, FirestoreError.InvalidArgument,
                          "__badpath__");
        registration2.Stop();
        Thread.Sleep(50);
        Assert.That(callbackInvoked, Is.False);
      }
    }

    [UnityTest]
    public IEnumerator QuerySnapshot_ShouldChangeCorrectly() {
      var collection = TestCollection();

      var initialData = TestData(1);
      var updatedData = TestData(2);

      yield return AwaitSuccess(collection.Document("a").SetAsync(initialData));

      var accumulator = new EventAccumulator<QuerySnapshot>();
      var registration = collection.Listen(MetadataChanges.Include, accumulator.Listener);

      // Wait for the first snapshot.
      yield return AwaitSuccess(accumulator.LastEventAsync());

      // Wait for new snapshot once we're synced with the backend.
      yield return AwaitSuccess(accumulator.LastEventAsync());

      // Update the document and wait for the latency compensated snapshot.
      {
        collection.Document("a").SetAsync(updatedData);
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot[0].ToDictionary(), Is.EquivalentTo(updatedData));
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.True);
      }

      // Wait for backend acknowledged snapshot.
      {
        var lastEventTask = accumulator.LastEventAsync();
        yield return AwaitSuccess(lastEventTask);
        QuerySnapshot snapshot = lastEventTask.Result;
        Assert.That(snapshot.Metadata.HasPendingWrites, Is.False);

        var changes = snapshot.GetChanges();
        Assert.That(changes, Is.Empty);
        var changesIncludingMetadata = snapshot.GetChanges(MetadataChanges.Include);
        var changeList = changesIncludingMetadata.ToList();
        Assert.That(changeList.Count(), Is.EqualTo(1));

        var changedDocument = changeList.First().Document;
        Assert.That(changedDocument.Metadata.HasPendingWrites, Is.False);
      }

      {
        // Remove the listener and make sure we don't get events anymore.
        accumulator.ThrowOnAnyEvent();
        registration.Stop();
        yield return AwaitSuccess(collection.Document("c").SetAsync(TestData(3)));
      }
    }

    [UnityTest]
    public IEnumerator CommonQueries_ShouldWork() {
      // Initialize collection with a few test documents to query against.
      var collection = TestCollection();
      collection.Document("a").SetAsync(new Dictionary<string, object> {
        { "num", 1 },
        { "state", "created" },
        { "active", true },
        { "nullable", "value" },
      });
      collection.Document("b").SetAsync(new Dictionary<string, object> {
        { "num", 2 },
        { "state", "done" },
        { "active", false },
        { "nullable", null },
      });
      collection.Document("c").SetAsync(new Dictionary<string, object> {
        { "num", 3 },
        { "state", "done" },
        { "active", true },
        { "nullable", null },
      });
      // Put in a nested collection (with same ID) for testing collection group queries.
      collection.Document("d")
          .Collection(collection.Id)
          .Document("d-nested")
          .SetAsync(new Dictionary<string, object> {
            { "num", 4 },
            { "state", "created" },
            { "active", false },
            { "nullable", null },
          });
      yield return AwaitSuccess(db.WaitForPendingWritesAsync());

      yield return AssertQueryResults(desc: "EqualTo", query: collection.WhereEqualTo("num", 1),
                                      docIds: AsList("a"));
      yield return AssertQueryResults(desc: "EqualTo (FieldPath)",
                                      query: collection.WhereEqualTo(new FieldPath("num"), 1),
                                      docIds: AsList("a"));

      yield return AssertQueryResults(desc: "NotEqualTo",
                                      query: collection.WhereNotEqualTo("num", 1),
                                      docIds: AsList("b", "c"));
      yield return AssertQueryResults(desc: "NotEqualTo (FieldPath)",
                                      query: collection.WhereNotEqualTo(new FieldPath("num"), 1),
                                      docIds: AsList("b", "c"));
      yield return AssertQueryResults(
          desc: "NotEqualTo (FieldPath) on nullable",
          query: collection.WhereNotEqualTo(new FieldPath("nullable"), null), docIds: AsList("a"));

      yield return AssertQueryResults(desc: "LessThanOrEqualTo",
                                      query: collection.WhereLessThanOrEqualTo("num", 2),
                                      docIds: AsList("a", "b"));
      yield return AssertQueryResults(
          desc: "LessThanOrEqualTo (FieldPath)",
          query: collection.WhereLessThanOrEqualTo(new FieldPath("num"), 2),
          docIds: AsList("a", "b"));

      yield return AssertQueryResults(desc: "LessThan", query: collection.WhereLessThan("num", 2),
                                      docIds: AsList("a"));
      yield return AssertQueryResults(desc: "LessThan (FieldPath)",
                                      query: collection.WhereLessThan(new FieldPath("num"), 2),
                                      docIds: AsList("a"));

      yield return AssertQueryResults(desc: "GreaterThanOrEqualTo",
                                      query: collection.WhereGreaterThanOrEqualTo("num", 2),
                                      docIds: AsList("b", "c"));
      yield return AssertQueryResults(
          desc: "GreaterThanOrEqualTo (FieldPath)",
          query: collection.WhereGreaterThanOrEqualTo(new FieldPath("num"), 2),
          docIds: AsList("b", "c"));

      yield return AssertQueryResults(
          desc: "GreaterThan", query: collection.WhereGreaterThan("num", 2), docIds: AsList("c"));
      yield return AssertQueryResults(desc: "GreaterThan (FieldPath)",
                                      query: collection.WhereGreaterThan(new FieldPath("num"), 2),
                                      docIds: AsList("c"));

      yield return AssertQueryResults(
          desc: "two EqualTos",
          query: collection.WhereEqualTo("state", "done").WhereEqualTo("active", false),
          docIds: AsList("b"));

      yield return AssertQueryResults(desc: "OrderBy, Limit",
                                      query: collection.OrderBy("num").Limit(2),
                                      docIds: AsList("a", "b"));
      yield return AssertQueryResults(desc: "OrderBy, Limit (FieldPath)",
                                      query: collection.OrderBy(new FieldPath("num")).Limit(2),
                                      docIds: AsList("a", "b"));

      yield return AssertQueryResults(desc: "OrderByDescending, Limit",
                                      query: collection.OrderByDescending("num").Limit(2),
                                      docIds: AsList("c", "b"));
      yield return AssertQueryResults(
          desc: "OrderByDescending, Limit (FieldPath)",
          query: collection.OrderByDescending(new FieldPath("num")).Limit(2),
          docIds: AsList("c", "b"));

      yield return AssertQueryResults(
          desc: "StartAfter", query: collection.OrderBy("num").StartAfter(2), docIds: AsList("c"));
      yield return AssertQueryResults(
          desc: "EndBefore", query: collection.OrderBy("num").EndBefore(2), docIds: AsList("a"));
      yield return AssertQueryResults(desc: "StartAt, EndAt",
                                      query: collection.OrderBy("num").StartAt(2).EndAt(2),
                                      docIds: AsList("b"));

      // Collection Group Query
      yield return AssertQueryResults(desc: "CollectionGroup",
                                      query: db.CollectionGroup(collection.Id),
                                      docIds: AsList("a", "b", "c", "d-nested"));
    }

    private static List<T> AsList<T>(params T[] elements) {
      return elements.ToList();
    }

    private IEnumerator AssertQueryResults(string desc, Query query, List<string> docIds) {
      var getTask = query.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      var snapshot = getTask.Result;

      Assert.That(snapshot.AsEnumerable().Select(d => d.Id), Is.EquivalentTo(docIds),
                  desc + ": Query results");
    }

    [UnityTest]
    public IEnumerator LimitToLastWithMirrorQuery_ShouldWork() {
      var collection = TestCollection();
      // TODO(b/149105903): Uncomment this line when exception can be raised from SWIG and below.
      // Assert.Throws(typeof(InvalidOperationException), () =>
      // Await(c.LimitToLast(2).GetSnapshotAsync())

      // Initialize data with a few test documents to query against.
      collection.Document("a").SetAsync(new Dictionary<string, object> {
        { "k", "a" },
        { "sort", 0 },
      });
      collection.Document("b").SetAsync(new Dictionary<string, object> {
        { "k", "b" },
        { "sort", 1 },
      });
      collection.Document("c").SetAsync(new Dictionary<string, object> {
        { "k", "c" },
        { "sort", 1 },
      });
      collection.Document("d").SetAsync(new Dictionary<string, object> {
        { "k", "d" },
        { "sort", 2 },
      });
      yield return AwaitSuccess(db.WaitForPendingWritesAsync());

      // Setup `limit` query.
      var limit = collection.Limit(2).OrderBy("sort");
      var limitAccumulator = new EventAccumulator<QuerySnapshot>();
      var limitRegistration = limit.Listen(limitAccumulator.Listener);

      // Setup mirroring `limitToLast` query.
      var limitToLast = collection.LimitToLast(2).OrderByDescending("sort");
      var limitToLastAccumulator = new EventAccumulator<QuerySnapshot>();
      var limitToLastRegistration = limitToLast.Listen(limitToLastAccumulator.Listener);

      // Verify both query get expected result.
      var lastEventTask = limitAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      var data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "a" }, { "sort", 0L } },
        new Dictionary<string, object> { { "k", "b" }, { "sort", 1L } }
      }));
      lastEventTask = limitToLastAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "b" }, { "sort", 1L } },
        new Dictionary<string, object> { { "k", "a" }, { "sort", 0L } }
      }));

      // Unlisten then re-listen limit query.
      limitRegistration.Stop();
      limit.Listen(limitAccumulator.Listener);

      // Verify `limit` query still works.
      lastEventTask = limitAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "a" }, { "sort", 0L } },
        new Dictionary<string, object> { { "k", "b" }, { "sort", 1L } }
      }));

      // Add a document that would change the result set.
      yield return AwaitSuccess(collection.Document("d").SetAsync(new Dictionary<string, object> {
        { "k", "e" },
        { "sort", -1 },
      }));

      // Verify both query get expected result.
      lastEventTask = limitAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "e" }, { "sort", -1L } },
        new Dictionary<string, object> { { "k", "a" }, { "sort", 0L } }
      }));
      lastEventTask = limitToLastAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "a" }, { "sort", 0L } },
        new Dictionary<string, object> { { "k", "e" }, { "sort", -1L } }
      }));

      // Unlisten to limitToLast, update a doc, then relisten to limitToLast
      limitToLastRegistration.Stop();
      yield return AwaitSuccess(collection.Document("a").UpdateAsync("sort", -2));
      limitToLast.Listen(limitToLastAccumulator.Listener);

      // Verify both query get expected result.
      lastEventTask = limitAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "a" }, { "sort", -2L } },
        new Dictionary<string, object> { { "k", "e" }, { "sort", -1L } }
      }));
      lastEventTask = limitToLastAccumulator.LastEventAsync();
      yield return AwaitSuccess(lastEventTask);
      data = QuerySnapshotToValues(lastEventTask.Result);
      Assert.That(data, Is.EquivalentTo(new List<Dictionary<string, object>> {
        new Dictionary<string, object> { { "k", "e" }, { "sort", -1L } },
        new Dictionary<string, object> { { "k", "a" }, { "sort", -2L } }
      }));
    }

    private static List<Dictionary<string, object>> QuerySnapshotToValues(QuerySnapshot snap) {
      List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
      foreach (DocumentSnapshot doc in snap) {
        result.Add(doc.ToDictionary());
      }
      return result;
    }

    [UnityTest]
    public IEnumerator ArrayContainsQuery_ShouldReturnCorrectDocuments() {
      // Initialize collection with a few test documents to query against.
      var collection = TestCollection();
      collection.Document("a").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { 42 } },
      });

      collection.Document("b").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { "a", 42, "c" } },
      });

      collection.Document("c").SetAsync(new Dictionary<string, object> {
        { "array",
          new List<object> {
            41.999, "42", new Dictionary<string, object> { { "array", new List<object> { 42 } } }
          } }
      });

      collection.Document("d").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { 42 } },
        { "array2", new List<object> { "bingo" } },
      });
      yield return AwaitSuccess(db.WaitForPendingWritesAsync());

      yield return AssertQueryResults(
          desc: "ArrayContains", query: collection.WhereArrayContains(new FieldPath("array"), 42),
          docIds: AsList("a", "b", "d"));
      yield return AssertQueryResults(desc: "ArrayContains",
                                      query: collection.WhereArrayContains("array", 42),
                                      docIds: AsList("a", "b", "d"));
    }

    [UnityTest]
    public IEnumerator ArrayContainsAnyQuery_ShouldReturnCorrectDocuments() {
      // Initialize collection with a few test documents to query against.
      var collection = TestCollection();
      collection.Document("a").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { 42 } },
      });

      collection.Document("b").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { "a", 42, "c" } },
      });

      collection.Document("c").SetAsync(new Dictionary<string, object> {
        { "array",
          new List<object> {
            41.999, "42", new Dictionary<string, object> { { "array", new List<object> { 42 } } }
          } }
      });

      collection.Document("d").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { 42 } },
        { "array2", new List<object> { "bingo" } },
      });

      collection.Document("e").SetAsync(
          new Dictionary<string, object> { { "array", new List<object> { 43 } } });

      collection.Document("f").SetAsync(new Dictionary<string, object> {
        { "array", new List<object> { new Dictionary<string, object> { { "a", 42 } } } }
      });

      collection.Document("g").SetAsync(new Dictionary<string, object> {
        { "array", 42 },
      });

      yield return AwaitSuccess(db.WaitForPendingWritesAsync());

      yield return AssertQueryResults(
          desc: "ArrayContainsAny",
          query: collection.WhereArrayContainsAny("array", new List<object> { 42, 43 }),
          docIds: AsList("a", "b", "d", "e"));

      yield return AssertQueryResults(
          desc: "ArrayContainsAnyObject",
          query: collection.WhereArrayContainsAny(
              new FieldPath("array"),
              new List<object> { new Dictionary<string, object> { { "a", 42 } } }),
          docIds: AsList("f"));
    }

    [UnityTest]
    public IEnumerator InQuery_ShouldReturnCorrectDocuments() {
      // Initialize collection with a few test documents to query against.
      var collection = TestCollection();
      collection.Document("a").SetAsync(new Dictionary<string, object> {
        { "zip", 98101 },
        { "nullable", null },
      });
      collection.Document("b").SetAsync(new Dictionary<string, object> {
        { "zip", 98102 },
        { "nullable", "value" },
      });
      collection.Document("c").SetAsync(new Dictionary<string, object> {
        { "zip", 98103 },
        { "nullable", null },
      });
      collection.Document("d").SetAsync(
          new Dictionary<string, object> { { "zip", new List<object> { 98101 } } });
      collection.Document("e").SetAsync(new Dictionary<string, object> {
        { "zip", new List<object> { "98101", new Dictionary<string, object> { { "zip", 98101 } } } }
      });
      collection.Document("f").SetAsync(new Dictionary<string, object> {
        { "zip", new Dictionary<string, object> { { "code", 500 } } },
        { "nullable", 123 },
      });
      collection.Document("g").SetAsync(new Dictionary<string, object> {
        { "zip", new List<object> { 98101, 98102 } },
        { "nullable", null },
      });
      yield return AwaitSuccess(db.WaitForPendingWritesAsync());

      yield return AssertQueryResults(
          desc: "InQuery",
          query: collection.WhereIn(
              "zip", new List<object> { 98101, 98103, new List<object> { 98101, 98102 } }),
          docIds: AsList("a", "c", "g"));

      yield return AssertQueryResults(
          desc: "InQueryWithObject",
          query: collection.WhereIn(
              new FieldPath("zip"),
              new List<object> { new Dictionary<string, object> { { "code", 500 } } }),
          docIds: AsList("f"));

      yield return AssertQueryResults(
          desc: "InQueryWithDocIds",
          query: collection.WhereIn(FieldPath.DocumentId, new List<object> { "c", "e" }),
          docIds: AsList("c", "e"));

      yield return AssertQueryResults(
          desc: "NotInQuery",
          query: collection.WhereNotIn(
              "zip", new List<object> { 98101, 98103, new List<object> { 98101, 98102 } }),
          docIds: AsList("b", "d", "e", "f"));

      yield return AssertQueryResults(
          desc: "NotInQueryWithObject",
          query: collection.WhereNotIn(
              new FieldPath("zip"),
              new List<object> { new List<object> { 98101, 98102 },
                                 new Dictionary<string, object> { { "code", 500 } } }),
          docIds: AsList("a", "b", "c", "d", "e"));

      yield return AssertQueryResults(
          desc: "NotInQueryWithDocIds",
          query: collection.WhereNotIn(FieldPath.DocumentId, new List<object> { "a", "c", "e" }),
          docIds: AsList("b", "d", "f", "g"));

      yield return AssertQueryResults(
          desc: "NotInQueryWithNulls",
          query: collection.WhereNotIn(new FieldPath("nullable"), new List<object> { null }),
          docIds: new List<String> {});
    }

    [UnityTest]
    // Tests that DocumentReference and Query respect the Source parameter passed to
    // GetSnapshotAsync().  We don't exhaustively test the behavior. We just do enough
    // checks to verify that cache, default, and server produce distinct results.
    public IEnumerator GetBySource_ShouldWork() {
      DocumentReference doc = TestDocument();

      yield return AwaitSuccess(doc.SetAsync(TestData()));

      // Verify FromCache gives us cached results even when online.
      {
        var getByCache = doc.GetSnapshotAsync(Source.Cache);
        yield return AwaitSuccess(getByCache);
        DocumentSnapshot docSnap = getByCache.Result;
        Assert.That(docSnap.Metadata.IsFromCache, Is.True);
        var getParentByCache = doc.Parent.GetSnapshotAsync(Source.Cache);
        yield return AwaitSuccess(getParentByCache);
        QuerySnapshot querySnap = getParentByCache.Result;
        Assert.That(querySnap.Metadata.IsFromCache, Is.True);
      }

      // Verify Default gives us non-cached results when online.
      {
        var getByDefault = doc.GetSnapshotAsync(Source.Default);
        yield return AwaitSuccess(getByDefault);
        DocumentSnapshot docSnap = getByDefault.Result;
        Assert.That(docSnap.Metadata.IsFromCache, Is.False);
        var getParentByDefault = doc.Parent.GetSnapshotAsync(Source.Default);
        yield return AwaitSuccess(getParentByDefault);
        QuerySnapshot querySnap = getParentByDefault.Result;
        Assert.That(querySnap.Metadata.IsFromCache, Is.False);
      }

      {
        // Disable network so we can test offline behavior.
        yield return AwaitSuccess(doc.Firestore.DisableNetworkAsync());

        // Verify Default gives us cached results when offline.
        var getByDefault = doc.GetSnapshotAsync(Source.Default);
        yield return AwaitSuccess(getByDefault);
        DocumentSnapshot docSnap = getByDefault.Result;
        Assert.That(docSnap.Metadata.IsFromCache, Is.True);
        var getParentByDefault = doc.Parent.GetSnapshotAsync(Source.Default);
        yield return AwaitSuccess(getParentByDefault);
        QuerySnapshot querySnap = getParentByDefault.Result;
        Assert.That(querySnap.Metadata.IsFromCache, Is.True);

        var getByServer = doc.GetSnapshotAsync(Source.Server);
        yield return AwaitCompletion(getByServer);
        AssertTaskFaulted(getByServer, FirestoreError.Unavailable);
        var getParentByServer = doc.Parent.GetSnapshotAsync(Source.Server);
        yield return AwaitCompletion(getParentByServer);
        AssertTaskFaulted(getParentByServer, FirestoreError.Unavailable);

        yield return AwaitSuccess(doc.Firestore.EnableNetworkAsync());
      }
    }
  }
}