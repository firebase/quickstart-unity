using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Tests.TestAsserts;

namespace Tests {
  public class TransactionAndBatchTests : FirestoreIntegrationTests {
    [UnityTest]
    public IEnumerator WriteBatch_ShouldWork() {
      DocumentReference doc1 = TestDocument();
      DocumentReference doc2 = TestDocument();
      DocumentReference doc3 = TestDocument();

      // Initialize doc1 and doc2 with some data.
      var initialData = new Dictionary<string, object> {
        { "field", "value" },
      };
      yield return AwaitSuccess(doc1.SetAsync(initialData));
      yield return AwaitSuccess(doc2.SetAsync(initialData));

      // Perform batch that deletes doc1, updates doc2, and overwrites doc3.
      yield return AwaitSuccess(
          doc1.Firestore.StartBatch()
              .Delete(doc1)
              .Update(doc2, new Dictionary<string, object> { { "field2", "value2" } })
              .Update(doc2,
                      new Dictionary<FieldPath, object> { { new FieldPath("field3"), "value3" } })
              .Update(doc2, "field4", "value4")
              .Set(doc3, initialData)
              .CommitAsync());

      {
        var getDoc1Task = doc1.GetSnapshotAsync();
        yield return AwaitSuccess(getDoc1Task);
        DocumentSnapshot snap = getDoc1Task.Result;
        Assert.That(snap.Exists, Is.False);
      }

      {
        var getDoc2Task = doc2.GetSnapshotAsync();
        yield return AwaitSuccess(getDoc2Task);
        DocumentSnapshot snap = getDoc2Task.Result;
        Assert.That(snap.ToDictionary(), Is.EquivalentTo(new Dictionary<string, object> {
          { "field", "value" },
          { "field2", "value2" },
          { "field3", "value3" },
          { "field4", "value4" },
        }));
      }

      {
        var getDoc3Task = doc3.GetSnapshotAsync();
        yield return AwaitSuccess(getDoc3Task);
        DocumentSnapshot snap = getDoc3Task.Result;
        Assert.That(snap.ToDictionary(), Is.EquivalentTo(initialData));
      }
    }

    [UnityTest]
    public IEnumerator WriteBatch_ShouldReportErrorOnInvalidDocument() {
      var docWithInvalidName = TestCollection().Document("__badpath__");
      Task commitWithInvalidDocTask = docWithInvalidName.Firestore.StartBatch()
                                          .Set(docWithInvalidName, TestData(0))
                                          .CommitAsync();
      yield return AwaitCompletion(commitWithInvalidDocTask);
      AssertTaskFaulted(commitWithInvalidDocTask, FirestoreError.InvalidArgument);
    }

    [UnityTest]
    public IEnumerator Transaction_ShouldWork() {
      DocumentReference doc1 = TestDocument();
      DocumentReference doc2 = TestDocument();
      DocumentReference doc3 = TestDocument();

      // Initialize doc1 and doc2 with some data.
      var initialData = new Dictionary<string, object> {
        { "field", "value" },
      };
      yield return AwaitSuccess(doc1.SetAsync(initialData));
      yield return AwaitSuccess(doc2.SetAsync(initialData));

      // Perform transaction that reads doc1, deletes doc1, updates doc2, and overwrites doc3.
      var transactionTask = doc1.Firestore.RunTransactionAsync<string>((transaction) => {
        Assert.That(mainThreadId, Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
        return transaction.GetSnapshotAsync(doc1).ContinueWith((getTask) => {
          Assert.That(getTask.Result.ToDictionary(), Is.EquivalentTo(initialData));
          transaction.Delete(doc1);
          transaction.Update(doc2, new Dictionary<string, object> { { "field2", "value2" } });
          transaction.Update(
              doc2, new Dictionary<FieldPath, object> { { new FieldPath("field3"), "value3" } });
          transaction.Update(doc2, "field4", "value4");
          transaction.Set(doc3, initialData);
          return "txn result";
        });
      });

      yield return AwaitSuccess(transactionTask);
      string result = transactionTask.Result;
      Assert.That(result, Is.EqualTo("txn result"));

      {
        var getTask = doc1.GetSnapshotAsync();
        yield return AwaitSuccess(getTask);
        DocumentSnapshot snap = getTask.Result;
        Assert.That(snap.Exists, Is.False);
      }

      {
        var getTask = doc2.GetSnapshotAsync();
        yield return AwaitSuccess(getTask);
        DocumentSnapshot snap = getTask.Result;
        Assert.That(snap.ToDictionary(), Is.EquivalentTo(new Dictionary<string, object> {
          { "field", "value" },
          { "field2", "value2" },
          { "field3", "value3" },
          { "field4", "value4" },
        }));
      }

      {
        var getTask = doc3.GetSnapshotAsync();
        yield return AwaitSuccess(getTask);
        DocumentSnapshot snap = getTask.Result;
        Assert.That(snap.ToDictionary(), Is.EquivalentTo(initialData));
      }
    }

    [UnityTest]
    public IEnumerator TransactionWithNonGenericTask_ShouldWork() {
      DocumentReference doc = TestDocument();
      yield return AwaitSuccess(db.RunTransactionAsync((transaction) => {
        transaction.Set(doc, TestData(1));
        // Create a plain (non-generic) `Task` result.
        return Task.CompletedTask;
      }));
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      DocumentSnapshot snap = getTask.Result;
      Assert.That(snap.ToDictionary(), Is.EquivalentTo(TestData(1)));
    }

    [UnityTest]
    public IEnumerator Transaction_CanAbortOnFailedTask() {
      int retries = 0;
      Task txnTask = db.RunTransactionAsync((transaction) => {
        retries++;
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        tcs.SetException(new InvalidOperationException("Failed Task"));
        return tcs.Task;
        // TODO(183714287): Why below makes txnTask succeed?
        // return Task.FromException(new InvalidOperationException("Failed Task"));
      });
      yield return AwaitCompletion(txnTask);
      Exception e = AssertTaskFaulted(txnTask);
      Assert.That(retries, Is.EqualTo(1));
      Assert.That(e, Is.TypeOf<InvalidOperationException>());
      Assert.That(e.Message, Is.EqualTo("Failed Task"));
    }

    [UnityTest]
    public IEnumerator Transaction_CanAbortOnException() {
      int retries = 0;
      Task txnTask = db.RunTransactionAsync((transaction) => {
        retries++;
        throw new InvalidOperationException("Failed Exception");
      });
      yield return AwaitCompletion(txnTask);
      Exception e = AssertTaskFaulted(txnTask);
      Assert.That(retries, Is.EqualTo(1));
      Assert.That(e, Is.TypeOf<InvalidOperationException>());
      Assert.That(e.Message, Is.EqualTo("Failed Exception"));
    }

    [UnityTest]
    public IEnumerator Transaction_AbortOnInvalidDocuments() {
      Task txnTask = db.RunTransactionAsync((transaction) => {
        var docWithInvalidName = TestCollection().Document("__badpath__");
        return transaction.GetSnapshotAsync(docWithInvalidName);
      });
      yield return AwaitCompletion(txnTask);
      AssertTaskFaulted(txnTask, FirestoreError.InvalidArgument, "__badpath__");
    }

    [UnityTest]
    public IEnumerator Transaction_AbortWithoutRetryOnPermanentError() {
      int retries = 0;
      DocumentReference doc = TestDocument();
      // Try to update a document that doesn't exist. Should fail permanently (no retries)
      // with a "Not Found" error.
      Task txnTask = db.RunTransactionAsync((transaction) => {
        retries++;
        transaction.Update(doc, TestData(0));
        return Task.CompletedTask;
      });
      yield return AwaitCompletion(txnTask);
      AssertTaskFaulted(txnTask, FirestoreError.NotFound, doc.Id);
      Assert.That(retries, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator Transaction_RetriesWithOutOfBandWrites() {
      int retries = 0;
      DocumentReference doc = TestDocument();
      Task txnTask = db.RunTransactionAsync((transaction) => {
        retries++;
        return transaction.GetSnapshotAsync(doc)
            .ContinueWith((snapshot) => {
              // Queue a write via the transaction.
              transaction.Set(doc, TestData(0));
              // But also write the document (out-of-band) so the transaction is retried.
              return doc.SetAsync(TestData(retries));
            })
            .Unwrap();
      });
      yield return AwaitCompletion(txnTask);
      AssertTaskFaulted(txnTask, FirestoreError.FailedPrecondition);
      // The transaction API will retry 6 times before giving up.
      Assert.That(retries, Is.EqualTo(6));
    }

    [UnityTest]
    public IEnumerator Transaction_RollsBackIfExceptionIsThrown() {
      // This test covers this bug: https://github.com/firebase/quickstart-unity/issues/1042
      DocumentReference doc = TestDocument();
      Task txnTask = db.RunTransactionAsync(transaction => {
        return transaction.GetSnapshotAsync(doc).ContinueWith(snapshotTask => {
          transaction.Set(doc, new Dictionary<string, object> { { "key", 42 } }, null);
          throw new TestException();
        });
      });

      yield return AwaitCompletion(txnTask);
      Exception exception = AssertTaskFaulted(txnTask);
      Assert.That(exception, Is.TypeOf<TestException>());

      // Verify that the transaction was rolled back.
      var getTask = doc.GetSnapshotAsync();
      yield return AwaitSuccess(getTask);
      DocumentSnapshot snap = getTask.Result;
      Assert.That(snap.Exists, Is.False);
    }

    private class TestException : Exception {
      public TestException() {}
    }
  }
}