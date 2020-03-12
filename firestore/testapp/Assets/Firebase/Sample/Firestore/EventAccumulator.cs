using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Firebase.Sample.Firestore {
  class EventAccumulator<T> {
    private TaskCompletionSource<object> completion;
    private readonly List<T> events = new List<T>();
    private int maxEvents = 0;
    private bool assertOnAnyEvent = false;

    /// <summary>
    /// Returns a listener callback suitable for passing to Listen().
    /// </summary>
    public Action<T> Listener {
      get {
        return (value) => {
          lock (this) {
            HardAssert(!assertOnAnyEvent, "Received unexpected event: " + value);
            Debug.Log("EventAccumulator Received new event: " + value);
            events.Add(value);
            CheckFulfilled();
          }
        };
      }
    }

    /// <summary>
    /// Waits for the specified number of events and returns them.
    /// </summary>
    public List<T> Await(int numEvents) {
      lock (this) {
        HardAssert(completion == null, "calling await while another await is running");
        completion = new TaskCompletionSource<object>();
        maxEvents = maxEvents + numEvents;
        CheckFulfilled();
      }

      completion.Task.Wait();

      lock (this) {
        completion = null;
        return events.GetRange(maxEvents - numEvents, numEvents);
      }
    }

    /// <summary>
    /// Waits for an event and returns it.
    /// </summary>
    public T Await() {
      return Await(1)[0];
    }

    /// <summary>
    /// Throw an exception if any more events are received.
    /// </summary>
    public void ThrowOnAnyEvent() {
      lock (this) {
        HardAssert(events.Count == maxEvents, "Received unexpected event: " + events.Last());
        assertOnAnyEvent = true;
      }
    }

    private void CheckFulfilled() {
      lock (this) {
        if (completion != null && events.Count >= maxEvents) {
          completion.SetResult(null);
        }
      }
    }

    private void HardAssert(bool condition, string message) {
      if (!condition) {
        string text = "Assertion Failure: " + message;
        Debug.Log(text);
        throw new Exception(text);
      }
    }
  }
}
