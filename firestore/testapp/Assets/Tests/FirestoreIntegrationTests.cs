using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

namespace Tests {
  public class FirestoreIntegrationTests {
    [UnityTest]
    public IEnumerator TestGetKnownValueWorks() {
      var db = FirebaseFirestore.DefaultInstance;
      DocumentReference doc1 = db.Collection("col1").Document("doc1");
      var task = doc1.GetSnapshotAsync();
      yield return new WaitUntil(() => task.IsCompleted);
      var snap = task.Result;
      IDictionary<string, object> dict = snap.ToDictionary();
      Assert.IsTrue(dict.ContainsKey("field1"), "Resulting document is missing 'field1' field.");
      Assert.AreEqual("value1", dict["field1"].ToString(), "'field1' is not equal to 'value1'");
    }
  }
}
