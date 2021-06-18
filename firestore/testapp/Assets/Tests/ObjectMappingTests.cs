using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Firestore;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Tests.TestAsserts;

namespace Tests {
  public class ObjectMappingTests : FirestoreIntegrationTests {
    [UnitySetUp]
    public IEnumerator DisableNetworkBeforeRunningTests() {
      // All tests verify features that do not rely on network/backend, disabling it
      // to make the tests run faster.
      yield return AwaitSuccess(db.DisableNetworkAsync());
    }

    [UnityTearDown]
    public IEnumerator ReEnableNetworkAfterRunningTests() {
      yield return AwaitSuccess(db.EnableNetworkAsync());
    }

    [UnityTest]
    public IEnumerator ObjectMapping_ShouldWork() {
      DocumentReference doc = TestDocument();

      foreach (SerializationTestData.TestCase test in SerializationTestData.TestData(
                   doc.Firestore)) {
        // Write input to `doc` via serialization
        SerializeToDoc(doc, test.Input);
        var getTask = doc.GetSnapshotAsync(Source.Cache);
        yield return AwaitSuccess(getTask);
        DocumentSnapshot docSnap = getTask.Result;

        // Read `doc` back via deserialization with both Firestore Types and input types
        object actualOutputWithRawFirestoreTypes, actualOutputWithInputTypes;
        DeserializeDoc(docSnap, test.Input, out actualOutputWithRawFirestoreTypes,
                       out actualOutputWithInputTypes);

        Assert.That(actualOutputWithRawFirestoreTypes, Is.EqualTo(test.ExpectedRawOutput),
                    "Deserialized value with Firestore Raw types does not match expected");
        Assert.That(actualOutputWithInputTypes, Is.EqualTo(test.Input),
                    "Deserialized value with input types does not match the input");
      }
    }

    [UnityTest]
    public IEnumerator ObjectMapping_ShouldThrowForUnsupportedData() {
      DocumentReference doc = TestDocument();

      foreach (SerializationTestData.TestCase test in SerializationTestData.UnsupportedTestData()) {
        object actualWithInputTypes;
        Assert.Throws(typeof(NotSupportedException), () => SerializeToDoc(doc, test.Input));

        // Write the doc with expected output, then deserialize it back to input type.
        SerializeToDoc(doc, test.ExpectedRawOutput);
        Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync(Source.Cache);
        yield return AwaitSuccess(getTask);
        var exception = Assert.Throws<TargetInvocationException>(() => {
          DeserializeWithInputTypes(getTask.Result, test.Input, out actualWithInputTypes);
        });
        Assert.That(exception.InnerException, Is.TypeOf<NotSupportedException>());
      }
    }

    [UnityTest]
    public IEnumerator ObjectMapping_ShouldThrowForUnsupportedReadToInputTypesData() {
      DocumentReference doc = TestDocument();

      foreach (SerializationTestData.TestCase test in SerializationTestData
                   .UnsupportedReadToInputTypesTestData()) {
        object actualWithFirestoreRawTypes, actualWithInputTypes;

        SerializeToDoc(doc, test.Input);

        Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync(Source.Cache);
        yield return AwaitSuccess(getTask);
        DeserializeWithRawFirestoreTypes(getTask.Result, out actualWithFirestoreRawTypes);
        Assert.That(actualWithFirestoreRawTypes, Is.EqualTo(test.ExpectedRawOutput),
                    "Deserialized value with Firestore Raw types does not match expected");

        var exception = Assert.Throws<TargetInvocationException>(() => {
          DeserializeWithInputTypes(getTask.Result, test.Input, out actualWithInputTypes);
        });
        Assert.That(exception.InnerException, Is.TypeOf<NotSupportedException>());
      }
    }

    private void DeserializeDoc(DocumentSnapshot snapshot, object input, out object nativeOutput,
                                out object convertedOutput) {
      DeserializeWithRawFirestoreTypes(snapshot, out nativeOutput);

      DeserializeWithInputTypes(snapshot, input, out convertedOutput);
    }

    private static void SerializeToDoc(DocumentReference doc, object input) {
      // Wrap in a dictionary so we can write it as a document even if input is a primitive type.
      var docData = new Dictionary<string, object> { { "field", input } };
      // The returning task completes when receiving backend acknowledgement, we need to ignore this
      // because we are offline.
      doc.SetAsync(docData);
    }

    private static void DeserializeWithRawFirestoreTypes(DocumentSnapshot snapshot,
                                                         out object rawOutput) {
      rawOutput = snapshot.GetValue<object>("field");
    }

    private static void DeserializeWithInputTypes(DocumentSnapshot snapshot, object input,
                                                  out object outputWithInputTypes) {
      // To get the converted value out, we have to use reflection to call GetValue<> with
      // input.GetType() as the generic parameter.
      MethodInfo method =
          typeof(DocumentSnapshot)
              .GetMethod("GetValue",
                         new Type[] { typeof(string), typeof(ServerTimestampBehavior) });
      MethodInfo genericMethod =
          method.MakeGenericMethod(input != null ? input.GetType() : typeof(object));
      outputWithInputTypes =
          genericMethod.Invoke(snapshot, new object[] { "field", ServerTimestampBehavior.None });
    }

    // Unity on iOS can't JIT compile code, which breaks some usages of
    // reflection such as the way DeserializeWithInputTypes method calls the
    // DocumentSnapshot.GetValue<>() method with dynamic generic types. As a
    // workaround, we have to enumerate all the different versions that we need.
    // Normal users won't have to do this.
    // See https://docs.unity3d.com/Manual/ScriptingRestrictions.html for more details.
    private void UnityCompileHack() {
      // NOTE: This never actually runs. It just forces the compiler to generate
      // the code we need.
      DocumentSnapshot x = null;
      x.GetValue<bool>("");
      x.GetValue<string>("");
      x.GetValue<byte>("");
      x.GetValue<sbyte>("");
      x.GetValue<short>("");
      x.GetValue<ushort>("");
      x.GetValue<int>("");
      x.GetValue<uint>("");
      x.GetValue<long>("");
      x.GetValue<ulong>("");
      x.GetValue<float>("");
      x.GetValue<double>("");
      x.GetValue<Timestamp>("");
      x.GetValue<DateTime>("");
      x.GetValue<DateTimeOffset>("");
      x.GetValue<Blob>("");
      x.GetValue<GeoPoint>("");
      x.GetValue<SerializationTestData.StructModel>("");
      x.GetValue<SerializationTestData.ByteEnum>("");
      x.GetValue<SerializationTestData.SByteEnum>("");
      x.GetValue<SerializationTestData.Int16Enum>("");
      x.GetValue<SerializationTestData.UInt16Enum>("");
      x.GetValue<SerializationTestData.Int32Enum>("");
      x.GetValue<SerializationTestData.UInt32Enum>("");
      x.GetValue<SerializationTestData.Int64Enum>("");
      x.GetValue<SerializationTestData.UInt64Enum>("");
      x.GetValue<SerializationTestData.CustomConversionEnum>("");
      x.GetValue<SerializationTestData.CustomValueType>("");
      x.GetValue<Dictionary<string, SerializationTestData.GameResult>>("");
    }
  }
}