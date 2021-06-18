using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Firebase.Sample.Firestore {
  class EventAccumulator<T> {
    private readonly BlockingCollection<T> queue = new BlockingCollection<T>();
    private int mainThreadId = -1;

    public EventAccumulator() {}
    public EventAccumulator(int mainThreadId) {
      this.mainThreadId = mainThreadId;
    }

    /// <summary>
    /// Returns a listener callback suitable for passing to Listen().
    /// </summary>
    public Action<T> Listener {
      get {
        return (value) => {
          if (mainThreadId > 0) {
            Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(mainThreadId),
                        "Listener callback from non-main thread.");
          }

          queue.Add(value);
        };
      }
    }

    /// <summary>
    /// Waits for the specified number of events.
    /// </summary>
    public Task<List<T>> LastEventsAsync(int numEvents) {
      return Task.Run(() => {
        List<T> result = new List<T>();
        while (numEvents-- > 0) {
          result.Add(queue.Take());
        }

        return result;
      });
    }

    /// <summary>
    /// Waits for an event and returns it.
    /// </summary>
    public Task<T> LastEventAsync() {
      return Task.Run(queue.Take);
    }

    /// <summary>
    /// Throw an exception if any more events are received.
    /// </summary>
    public void ThrowOnAnyEvent() {
      queue.CompleteAdding();
      Assert.That(queue, Is.Empty);
    }
  }
}
