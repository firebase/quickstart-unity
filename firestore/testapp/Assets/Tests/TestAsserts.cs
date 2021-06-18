using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Firestore;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {

  public class FirestoreAssertionFailure : Exception {
    public FirestoreAssertionFailure(string message) : base(message) {}
  }

  public static class TestAsserts {
    public static IEnumerator AwaitCompletion(Task t) {
      yield return new WaitUntil((() => t.IsCompleted));
    }

    public static IEnumerator AwaitSuccess(Task t) {
      yield return AwaitCompletion(t);
      AssertTaskSucceeded(t);
    }

    public static IEnumerator AwaitFaults(Task t) {
      yield return AwaitCompletion(t);
      AssertTaskFaulted(t);
    }

    private static void AssertTaskProperties(Task t, bool isCompleted, bool isFaulted,
                                             bool isCanceled) {
      Assert.That(t.IsCompleted, Is.EqualTo(isCompleted));
      Assert.That(t.IsFaulted, Is.EqualTo(isFaulted));
      Assert.That(t.IsCanceled, Is.EqualTo(isCanceled));
    }

    public static void AssertTaskSucceeded(Task t) {
      AssertTaskProperties(t, isCompleted: true, isFaulted: false, isCanceled: false);
    }

    public static void AssertTaskIsPending(Task t) {
      AssertTaskProperties(t, isCompleted: false, isFaulted: false, isCanceled: false);
    }

    public static Exception AssertTaskFaulted(Task t) {
      Assert.That(t.Exception, Is.Not.Null,
                  "Task is supposed to fail with exception, yet it succeeds.");

      AggregateException e = t.Exception;
      Assert.That(e.InnerExceptions.Count, Is.EqualTo(1), "Task faulted with multiple exceptions");
      AssertTaskProperties(t, isCompleted: true, isFaulted: true, isCanceled: false);
      return e.InnerExceptions[0];
    }

    public static FirestoreException AssertTaskFaulted(Task t, FirestoreError expectedError,
                                                       string expectedMessageRegex = null) {
      var exception = AssertTaskFaulted(t);
      Assert.That(
          exception, Is.TypeOf<FirestoreException>(),
          "The task faulted (as expected); however, its exception was expected to " +
              $"be FirestoreException with ErrorCode={expectedError} but the actual exception " +
              $"was {exception.GetType()}: {exception}");

      var firestoreException = (FirestoreException)exception;
      Assert.That(firestoreException.ErrorCode, Is.EqualTo(expectedError),
                  "The task faulted with FirestoreException (as expected); however, its " +
                      $"ErrorCode was expected to be {expectedError} but the actual ErrorCode was" +
                      $" {firestoreException.ErrorCode}: {firestoreException}");

      if (expectedMessageRegex != null) {
        Assert.That(Regex.IsMatch(firestoreException.Message, expectedMessageRegex,
                                  RegexOptions.Singleline),
                    $"The task faulted with FirestoreException with ErrorCode={expectedError} " +
                        "(as expected); however, its message did not match the regular " +
                        $"expression {expectedMessageRegex}: {firestoreException.Message}");
      }

      return firestoreException;
    }

    public static async Task AssertExpectedDocument(DocumentReference doc,
                                                    Dictionary<string, object> expected) {
      var snap = await doc.GetSnapshotAsync();
      Assert.That(snap.ToDictionary(), Is.EquivalentTo(expected));
    }
  }

  // Tests for `TestAsserts` itself.
  public class TestAssertsTests {
    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldWork_WithFirestoreException() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetException(new FirestoreException(FirestoreError.Unavailable));
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Unavailable); },
                  Throws.Nothing);
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldWork_WithMatchingMessage() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetException(new FirestoreException(FirestoreError.Ok, "TheActualMessage"));
      yield return tcs.Task;
      Assert.That(() => {
        TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok, "The.*Message");
      }, Throws.Nothing);
    }

    // Verify that AssertTaskFaults() fails if the Task faults with an AggregateException that,
    // when flattened, resolves to a FirestoreException with the expected error code.
    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithAggregateException() {
      var tcs = new TaskCompletionSource<object>();
      var firestoreException = new FirestoreException(FirestoreError.Unavailable);
      var aggregateException1 = new AggregateException(new[] { firestoreException });
      var aggregateException2 = new AggregateException(new[] { aggregateException1 });
      tcs.SetException(aggregateException2);
      yield return tcs.Task;

      Assert.That(
          () => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Unavailable); },
          Throws.Exception,
          "AssertTaskFaults() should have thrown an exception because the AggregateException" +
              "has multiple nested AggregateException instances.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithUnexpectedErrorCode() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetResult(new FirestoreException(FirestoreError.Unavailable));
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok); },
                  Throws.Exception,
                  "AssertTaskFaults() should have thrown an exception because the task faulted " +
                      "with an incorrect error code.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_WhenTaskCompletes() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetResult(null);
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok); },
                  Throws.Exception,
                  "AssertTaskFaults() should have thrown an exception because the task faulted " +
                      "with an incorrect error code.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithNonFirestoreException() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetException(new InvalidOperationException());
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok); },
                  Throws.Exception,
                  "AssertTaskFaults() should have thrown an exception because the task faulted " +
                      "with an incorrect exception type.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithUnexpectedAggregateException() {
      var tcs = new TaskCompletionSource<object>();
      var exception1 = new InvalidOperationException();
      var exception2 = new AggregateException(new[] { exception1 });
      var exception3 = new AggregateException(new[] { exception2 });
      tcs.SetException(exception3);
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok); },
                  Throws.Exception,
                  "AssertTaskFaults() should have thrown an exception because the task faulted " +
                      "with an AggregateException that flattened to an unexpected exception.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithMoreThanOneInnerExceptions() {
      var tcs = new TaskCompletionSource<object>();
      var exception1 = new InvalidOperationException();
      var exception2 = new InvalidOperationException();
      var exception3 = new AggregateException(new[] { exception1, exception2 });
      tcs.SetException(exception3);
      yield return tcs.Task;
      Assert.That(() => { TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok); },
                  Throws.Exception,
                  "AssertTaskFaults() should have thrown an exception because the task faulted " +
                      "with an AggregateException that could not be fully flattened.");
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithMissingMessages() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetException(new FirestoreException(FirestoreError.Ok));
      yield return tcs.Task;
      Exception thrownException = Assert.Throws<AssertionException>(
          () => TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok, "SomeMessageRegex"));
      Assert.That(thrownException.Message, Contains.Substring("SomeMessageRegex"),
                  "AssertTaskFaults() threw an exception (as expected); however, its message was " +
                      "incorrect: " + thrownException.Message);
    }

    [UnityTest]
    public IEnumerator AssertTaskFaulted_ShouldThrow_IfFaultedWithMismatchMessages() {
      var tcs = new TaskCompletionSource<object>();
      tcs.SetException(new FirestoreException(FirestoreError.Ok, "TheActualMessage"));
      yield return tcs.Task;
      Exception thrownException = Assert.Throws<AssertionException>(
          () => TestAsserts.AssertTaskFaulted(tcs.Task, FirestoreError.Ok, "The.*MeaningOfLife"));

      Assert.That(thrownException.Message, Contains.Substring("TheActualMessage"));
      Assert.That(thrownException.Message, Contains.Substring("The.*MeaningOfLife"),
                  "AssertTaskFaults() threw an exception (as expected); however, its message was " +
                      thrownException.Message);
    }
  }
}
