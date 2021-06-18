using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Firebase.Firestore;
using static Tests.TestAsserts;

namespace Tests {

  // Tests for mutation operations of Firestore: set, update, mutating fieldValue, etc.
  public class MutationTests : FirestoreIntegrationTests {
    [UnityTest]
    public IEnumerator TestDeleteDocument() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> {
        { "f1", "v1" },
      };

      yield return AwaitSuccess(doc.SetAsync(data));

      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      DocumentSnapshot snap = getTask.Result;
      Assert.That(snap.Exists, "Written document should exist");

      var deleteTask = doc.DeleteAsync();
      yield return AwaitSuccess(deleteTask);

      getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      snap = getTask.Result;
      Assert.That(!snap.Exists, "Deleted document should not exist");
      Assert.That(snap.ToDictionary(), Is.Null);
    }

    [UnityTest]
    public IEnumerator TestWriteDocument() {
      DocumentReference doc = TestDocument();
      Dictionary<string, object> data = TestData();

      yield return AwaitSuccess(doc.SetAsync(data));
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      DocumentSnapshot snap = getTask.Result;
      Assert.That(snap.ToDictionary(), Is.EquivalentTo(data));
    }

    [UnityTest]
    public IEnumerator TestWriteDocumentViaCollection() {
      var addTask = TestCollection().AddAsync(TestData());
      yield return AwaitSuccess(addTask);

      var getTask = addTask.Result.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      DocumentSnapshot snap = getTask.Result;
      Assert.That(snap.ToDictionary(), Is.EquivalentTo(TestData()));
    }

    [UnityTest]
    public IEnumerator TestWriteInvalidDocumentFails() {
      var collectionWithInvalidName = TestCollection().Document("__badpath__").Collection("sub");
      var addTask = collectionWithInvalidName.AddAsync(TestData());
      yield return AwaitFaults(addTask);
    }

    [UnityTest]
    public IEnumerator TestWriteDocumentWithIntegersSaveAsLongs() {
      DocumentReference doc = db.Collection("col2").Document();
      var data = new Dictionary<string, object> {
        { "f1", 2 },
        { "map",
          new Dictionary<string, object> {
            { "nested f3", 4 },
          } },
      };
      var expected = new Dictionary<string, object> {
        { "f1", 2L },
        { "map",
          new Dictionary<string, object> {
            { "nested f3", 4L },
          } },
      };

      yield return AwaitSuccess(doc.SetAsync(data));
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      var actual = getTask.Result.ToDictionary();
      Assert.That(actual, Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestUpdateDocument() {
      DocumentReference doc = TestDocument();
      var data = TestData();
      var updateData =
          new Dictionary<string, object> { { "name", "foo" }, { "metadata.createdAt", 42L } };
      var expected = TestData();
      expected["name"] = "foo";
      ((Dictionary<string, object>)expected["metadata"])["createdAt"] = 42L;

      yield return AwaitSuccess(doc.SetAsync(data));
      yield return AwaitSuccess(doc.UpdateAsync(updateData));
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      DocumentSnapshot snap = getTask.Result;

      var actual = snap.ToDictionary();
      Assert.That(actual, Is.EquivalentTo(expected));
    }

    [UnityTest]
    // See b/174676322 for why this test is added.
    public IEnumerator TestMultipleDeletesInOneUpdate() {
      DocumentReference doc = TestDocument();
      var setTask = doc.SetAsync(new Dictionary<string, object> {
        { "key1", "value1" },
        { "key2", "value2" },
        { "key3", "value3" },
        { "key4", "value4" },
        { "key5", "value5" },
      });
      yield return AwaitSuccess(setTask);

      var updateTask = doc.UpdateAsync(new Dictionary<string, object> {
        { "key1", FieldValue.Delete },
        { "key3", FieldValue.Delete },
        { "key5", FieldValue.Delete },
      });
      yield return AwaitSuccess(updateTask);

      var getTask = doc.GetSnapshotAsync(Source.Cache);
      yield return AwaitSuccess(getTask);
      DocumentSnapshot snapshot = getTask.Result;
      var expected = new Dictionary<string, object> {
        { "key2", "value2" },
        { "key4", "value4" },
      };
      Assert.That(snapshot.ToDictionary(), Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestUpdateFieldInDocument() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> {
        { "f1", "v1" },
        { "f2", "v2" },
      };

      yield return AwaitSuccess(doc.SetAsync(data));
      yield return AwaitSuccess(doc.UpdateAsync("f2", "v2b"));

      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      var actual = getTask.Result.ToDictionary();
      var expected = new Dictionary<string, object> {
        { "f1", "v1" },
        { "f2", "v2b" },
      };
      Assert.That(actual, Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestCreateViaArrayUnion() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L) } };
      yield return AwaitSuccess(doc.SetAsync(data));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 1L, 2L } } });
    }

    [UnityTest]
    public IEnumerator TestAppendViaUpdateArrayUnion() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L) } };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 4L) } };
      yield return AwaitSuccess(doc.UpdateAsync(data));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 1L, 2L, 4L } } });
    }

    [UnityTest]
    public IEnumerator TestAppendViaSetMergeArrayUnion() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L) } };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(2L, 3L) } };
      yield return AwaitSuccess(doc.SetAsync(data, SetOptions.MergeAll));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 1L, 2L, 3L } } });
    }

    [UnityTest]
    public IEnumerator TestAppendObjectViaUpdateArrayUnion() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L) } };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> {
        { "array", FieldValue.ArrayUnion(1L, new Dictionary<string, object> { { "a", "value" } }) }
      };
      AwaitSuccess(doc.UpdateAsync(data));
      yield return AssertExpectedDocument(doc, new Dictionary<string, object> {
        { "array",
          new List<object> { 1L, 2L, new Dictionary<string, object> { { "a", "value" } } } }
      });
    }

    [UnityTest]
    public IEnumerator TestRemoveViaUpdateArrayRemove() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L) } };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> { { "array", FieldValue.ArrayRemove(1L, 4L) } };
      AwaitSuccess(doc.UpdateAsync(data));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 2L } } });
    }

    [UnityTest]
    public IEnumerator TestRemoveViaSetMergeArrayRemove() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> { { "array", FieldValue.ArrayUnion(1L, 2L, 3L) } };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> { { "array", FieldValue.ArrayRemove(1L, 3L) } };
      AwaitSuccess(doc.SetAsync(data, SetOptions.MergeAll));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 2L } } });
    }

    [UnityTest]
    public IEnumerator TestRemoveObjectsViaUpdateArrayRemove() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> {
        { "array",
          FieldValue.ArrayUnion(1L, new Dictionary<string, object> { { "a", "value" } }, 3L) }
      };
      yield return AwaitSuccess(doc.SetAsync(data));

      data = new Dictionary<string, object> {
        { "array", FieldValue.ArrayRemove(new Dictionary<string, object> { { "a", "value" } }) }
      };
      AwaitSuccess(doc.UpdateAsync(data));
      yield return AssertExpectedDocument(
          doc, new Dictionary<string, object> { { "array", new List<object> { 1L, 3L } } });
    }

    [UnityTest]
    public IEnumerator TestUpdateFieldPath() {
      DocumentReference doc = TestDocument();
      var data = new Dictionary<string, object> {
        { "f1", "v1" },
        { "f2",
          new Dictionary<string, object> {
            { "a",
              new Dictionary<string, object> {
                { "b",
                  new Dictionary<string, object> {
                    { "c", "v2" },
                  } },
              } },
          } },
      };

      var updateData = new Dictionary<FieldPath, object> {
        { new FieldPath("f2", "a", "b", "c"), "v2b" },
        { new FieldPath("f2", "x", "y", "z"), "v3" },
      };

      var expected = new Dictionary<string, object> {
        { "f1", "v1" },
        { "f2",
          new Dictionary<string, object> {
            { "a",
              new Dictionary<string, object> {
                { "b",
                  new Dictionary<string, object> {
                    { "c", "v2b" },
                  } },
              } },
            { "x",
              new Dictionary<string, object> {
                { "y",
                  new Dictionary<string, object> {
                    { "z", "v3" },
                  } },
              } },
          } },
      };

      yield return AwaitSuccess(doc.SetAsync(data));
      yield return AwaitSuccess(doc.UpdateAsync(updateData));
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);

      var actual = getTask.Result.ToDictionary();
      Assert.That(actual, Is.EquivalentTo(expected));
    }

    [UnityTest]
    public IEnumerator TestSetOptions() {
      DocumentReference doc = TestDocument();
      var initialData = new Dictionary<string, object> {
        { "field1", "value1" },
      };

      var set1 = new Dictionary<string, object> { { "field2", "value2" } };
      var setOptions1 = SetOptions.MergeAll;

      var set2 =
          new Dictionary<string, object> { { "field3", "value3" }, { "not-field4", "not-value4" } };
      var setOptions2 = SetOptions.MergeFields("field3");

      var set3 =
          new Dictionary<string, object> { { "field4", "value4" }, { "not-field5", "not-value5" } };
      var setOptions3 = SetOptions.MergeFields(new FieldPath("field4"));

      var expected = new Dictionary<string, object> {
        { "field1", "value1" },
        { "field2", "value2" },
        { "field3", "value3" },
        { "field4", "value4" },
      };

      yield return AwaitSuccess(doc.SetAsync(initialData));
      yield return AwaitSuccess(doc.SetAsync(set1, setOptions1));
      yield return AwaitSuccess(
          doc.Firestore.StartBatch().Set(doc, set2, setOptions2).CommitAsync());
      yield return AwaitSuccess(db.RunTransactionAsync(transaction => {
        transaction.Set(doc, set3, setOptions3);
        return Task.CompletedTask;
      }));

      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      Assert.That(getTask.Result.ToDictionary(), Is.EquivalentTo(expected));
    }
  }
}
