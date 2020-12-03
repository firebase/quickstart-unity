// Copyright 2017, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


// NOTE: This file based on
// http://github.com/googleapis/google-cloud-dotnet/blob/c4439e0099d9e4c414afbe666f0d2bb28f95b297/apis/Google.Cloud.Firestore/Google.Cloud.Firestore.Tests/SerializationTestData.cs
// This file has been modified from its original version. It has been adapted to use raw C# types
// instead of protos and to work in .Net 3.5.


using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;

namespace Firebase.Sample.Firestore {
  internal static class SerializationTestData {
    private static DateTime dateTime = new DateTime(1990, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    private static DateTimeOffset dateTimeOffset = new DateTimeOffset(1990, 1, 2, 3, 4, 5, TimeSpan.FromHours(1));

    internal class TestCase {
      internal enum TestOutcome {
        Success,
        // Given input type can be write to Firestore, can be read into Firestore raw types, but not
        // the input type.
        ReadToInputTypesNotSupported,
        // Given input type cannot be write to Firestore, nor can any data be read into the input
        // type.
        Unsupported
      }

      private TestOutcome _result = TestOutcome.Success;

      /// <summary>
      ///  Expected outcome of this test.
      /// </summary>
      internal TestOutcome Result {
        get { return _result; }
      private
        set { _result = value; }
      }

      /// <summary>
      /// The object to serialize into Firestore document.
      /// </summary>
      internal object Input { get; private set; }

      /// <summary>
      /// The expected output from the Firestore document deserialized to raw
      /// Firestore type (<code>Dictionary<string, object></code>, or primitive types).
      /// </summary>
      internal object ExpectedRawOutput { get; private set; }

      public TestCase(object input, object expectedRawOutput) {
        Input = input;
        ExpectedRawOutput = expectedRawOutput;
      }

      public TestCase(object input, object expectedRawOutput, TestOutcome result) {
        Input = input;
        ExpectedRawOutput = expectedRawOutput;
        Result = result;
      }
    }

    public static IEnumerable<TestCase> TestData(FirebaseFirestore database) {
      return new List<TestCase> {
        // Simple types
        { new TestCase(null, null) },
        { new TestCase(true, true) },
        { new TestCase(false, false) },
        { new TestCase("test", "test") },
        { new TestCase((byte)1, 1L) },
        { new TestCase((sbyte)1, 1L) },
        { new TestCase((short)1, 1L) },
        { new TestCase((ushort)1, 1L) },
        { new TestCase(1, 1L) },
        { new TestCase(1U, 1L) },
        { new TestCase(1L, 1L) },
        { new TestCase(1UL, 1L) },
        { new TestCase(1.5F, 1.5D) },
        { new TestCase(float.PositiveInfinity, double.PositiveInfinity) },
        { new TestCase(float.NegativeInfinity, double.NegativeInfinity) },
        { new TestCase(float.NaN, double.NaN) },
        { new TestCase(1.5D, 1.5D) },
        { new TestCase(double.PositiveInfinity, double.PositiveInfinity) },
        { new TestCase(double.NegativeInfinity, double.NegativeInfinity) },
        { new TestCase(double.NaN, double.NaN) },

        // Min/max values of each integer type
        { new TestCase(byte.MinValue, (long)byte.MinValue) },
        { new TestCase(byte.MaxValue, (long)byte.MaxValue) },
        { new TestCase(sbyte.MinValue, (long)sbyte.MinValue) },
        { new TestCase(sbyte.MaxValue, (long)sbyte.MaxValue) },
        { new TestCase(short.MinValue, (long)short.MinValue) },
        { new TestCase(short.MaxValue, (long)short.MaxValue) },
        { new TestCase(ushort.MinValue, (long)ushort.MinValue) },
        { new TestCase(ushort.MaxValue, (long)ushort.MaxValue) },
        { new TestCase(int.MinValue, (long)int.MinValue) },
        { new TestCase(int.MaxValue, (long)int.MaxValue) },
        { new TestCase(uint.MinValue, (long)uint.MinValue) },
        { new TestCase(uint.MaxValue, (long)uint.MaxValue) },
        { new TestCase(long.MinValue, long.MinValue) },
        { new TestCase(long.MaxValue, long.MaxValue) },
        // We don't cover the whole range of ulong
        { new TestCase((ulong)0, 0L) },
        { new TestCase((ulong) long.MaxValue, long.MaxValue) },

        // Enum types
        { new TestCase(ByteEnum.MinValue, (long)byte.MinValue) },
        { new TestCase(ByteEnum.MaxValue, (long)byte.MaxValue) },
        { new TestCase(SByteEnum.MinValue, (long)sbyte.MinValue) },
        { new TestCase(SByteEnum.MaxValue, (long)sbyte.MaxValue) },
        { new TestCase(Int16Enum.MinValue, (long)short.MinValue) },
        { new TestCase(Int16Enum.MaxValue, (long)short.MaxValue) },
        { new TestCase(UInt16Enum.MinValue, (long)ushort.MinValue) },
        { new TestCase(UInt16Enum.MaxValue, (long)ushort.MaxValue) },
        { new TestCase(Int32Enum.MinValue, (long)int.MinValue) },
        { new TestCase(Int32Enum.MaxValue, (long)int.MaxValue) },
        { new TestCase(UInt32Enum.MinValue, (long)uint.MinValue) },
        { new TestCase(UInt32Enum.MaxValue, (long)uint.MaxValue) },
        { new TestCase(Int64Enum.MinValue, (long)long.MinValue) },
        { new TestCase(Int64Enum.MaxValue, (long)long.MaxValue) },
        // We don't cover the whole range of ulong
        { new TestCase(UInt64Enum.MinValue, (long)0) },
        { new TestCase(UInt64Enum.MaxRepresentableValue, (long)long.MaxValue) },
        { new TestCase(CustomConversionEnum.Foo, "Foo") },
        { new TestCase(CustomConversionEnum.Bar, "Bar") },

        // Timestamps
        { new TestCase(Timestamp.FromDateTime(dateTime), Timestamp.FromDateTime(dateTime)) },
        { new TestCase(dateTime, Timestamp.FromDateTime(dateTime)) },
        { new TestCase(dateTimeOffset, Timestamp.FromDateTimeOffset(dateTimeOffset)) },

        // Blobs
        { new TestCase(new byte[] { 1, 2, 3, 4 }, Blob.CopyFrom(new byte[] { 1, 2, 3, 4 })) },
        { new TestCase(Blob.CopyFrom(new byte[] { 1, 2, 3, 4 }),
                       Blob.CopyFrom(new byte[] { 1, 2, 3, 4 })) },

        // GeoPoints
        { new TestCase(new GeoPoint(1.5, 2.5), new GeoPoint(1.5, 2.5)) },

        // Array values
        { new TestCase(new string[] { "x", "y" }, new List<object> { "x", "y" }) },
        { new TestCase(new List<string> { "x", "y" }, new List<object> { "x", "y" }) },
        { new TestCase(new int[] { 3, 4 }, new List<object> { 3L, 4L }) },

        // Deliberately DateTime rather than Timestamp here - we need to be able to detect the
        // element type to perform the per-element deserialization correctly
        { new TestCase(new List<DateTime> { dateTime, dateTime },
                       new List<object> { Timestamp.FromDateTime(dateTime),
                                          Timestamp.FromDateTime(dateTime) }) },

        // Map values (that can be deserialized again): dictionaries, attributed types, expandos
        // (which are just dictionaries), custom serialized map-like values

        // Dictionaries
        { new TestCase(new Dictionary<string, byte> { { "A", 10 }, { "B", 20 } },
                       new Dictionary<string, object> { { "A", 10L }, { "B", 20L } }) },
        { new TestCase(new Dictionary<string, int> { { "A", 10 }, { "B", 20 } },
                       new Dictionary<string, object> { { "A", 10L }, { "B", 20L } }) },
        { new TestCase(new Dictionary<string, object> { { "name", "Jon" }, { "score", 10L } },
                       new Dictionary<string, object> { { "name", "Jon" }, { "score", 10L } }) },
        // Attributed type (each property has an attribute)
        { new TestCase(new GameResult { Name = "Jon", Score = 10 },
                       new Dictionary<string, object> { { "name", "Jon" }, { "Score", 10L } }) },
        // Attributed type contained in a dictionary
        { new TestCase(
            new Dictionary<string, GameResult> { { "result",
                                                   new GameResult { Name = "Jon", Score = 10 } } },
            new Dictionary<string, object> {
              { "result", new Dictionary<string, object> { { "name", "Jon" }, { "Score", 10L } } }
            }) },
        // Attributed type containing a dictionary
        { new TestCase(
            new DictionaryInterfaceContainer {
              Integers = new Dictionary<string, int> { { "A", 10 }, { "B", 20 } }
            },
            new Dictionary<string, object> {
              { "Integers", new Dictionary<string, object> { { "A", 10L }, { "B", 20L } } }
            }) },
        // Attributed type serialized and deserialized by CustomPlayerConverter
        { new TestCase(new CustomPlayer { Name = "Amanda", Score = 15 },
                       new Dictionary<string, object> { { "PlayerName", "Amanda" },
                                                        { "PlayerScore", 15L } }) },

        // Attributed value type serialized and deserialized by CustomValueTypeConverter
        { new TestCase(new CustomValueType("xyz", 10),
                       new Dictionary<string, object> { { "Name", "xyz" }, { "Value", 10L } }) },

        // Attributed type with enums (name and number)
        { new TestCase(new ModelWithEnums { EnumDefaultByName = CustomConversionEnum.Foo,
                                            EnumAttributedByName = Int32Enum.MinValue,
                                            EnumByNumber = Int32Enum.MaxValue },
                       new Dictionary<string, object> { { "EnumDefaultByName", "Foo" },
                                                        { "EnumAttributedByName", "MinValue" },
                                                        { "EnumByNumber", (long)int.MaxValue } }) },

        // Attributed type with List field
        { new TestCase(new CustomUser { Name = "Jon", HighScore = 10,
                                        Emails = new List<string> { "jon@example.com" } },
                       new Dictionary<string, object> {
                         { "Name", "Jon" },
                         { "HighScore", 10l },
                         { "Emails", new List<object> { "jon@example.com" } }
                       }) },

        // Attributed type with IEnumerable field
        { new TestCase(
            new CustomUserEnumerableEmails() { Name = "Jon", HighScore = 10,
                                               Emails = new List<string> { "jon@example.com" } },
            new Dictionary<string, object> { { "Name", "Jon" },
                                             { "HighScore", 10l },
                                             { "Emails",
                                               new List<object> { "jon@example.com" } } }) },

        // Attributed type with Set field and custom converter.
        { new TestCase(
            new CustomUserSetEmailsWithConverter() {
              Name = "Jon", HighScore = 10, Emails = new HashSet<string> { "jon@example.com" }
            },
            new Dictionary<string, object> { { "Name", "Jon" },
                                             { "HighScore", 10l },
                                             { "Emails",
                                               new List<object> { "jon@example.com" } } }) },

        // Attributed struct
        { new TestCase(new StructModel { Name = "xyz", Value = 10 },
                       new Dictionary<string, object> { { "Name", "xyz" }, { "Value", 10L } }) },

        // Document references
        { new TestCase(database.Document("a/b"), database.Document("a/b")) },
      };
    }

    public static IEnumerable<TestCase> UnsupportedTestData() {
      return new List<TestCase> {
        // Nullable type handling
        { new TestCase(new NullableContainer { NullableValue = 10 },
                       new Dictionary<string, object> { { "NullableValue", 10L } },
                       TestCase.TestOutcome.Unsupported) },
        { new TestCase(new NullableEnumContainer { NullableValue = (Int32Enum)10 },
                       new Dictionary<string, object> { { "NullableValue", 10L } },
                       TestCase.TestOutcome.Unsupported) },
        // This one fails because the `NullableContainer` it gets back has a random value
        // while it should be null.
        { new TestCase(new NullableContainer { NullableValue = null },
                       new Dictionary<string, object> { { "NullableValue", null } },
                       TestCase.TestOutcome.Unsupported) },
        { new TestCase(new NullableEnumContainer { NullableValue = null },
                       new Dictionary<string, object> { { "NullableValue", null } },
                       TestCase.TestOutcome.Unsupported) },

        // IEnumerable values cannot be assigned from a List.
        // TODO(b/173894435): there should be a way to specify if it is serialization or
        // deserialization failure.
        { new TestCase(Enumerable.Range(3, 2).Select(i => (long)i),
                       Enumerable.Range(3, 2).Select(i => (long)i),
                       TestCase.TestOutcome.ReadToInputTypesNotSupported) },
        { new TestCase(
            new CustomUserSetEmails { Name = "Jon", HighScore = 10,
                                      Emails = new HashSet<string> { "jon@example.com" } },
            new Dictionary<string, object> { { "Name", "Jon" },
                                             { "HighScore", 10L },
                                             { "Emails", new List<object> { "jon@example.com" } } },
            TestCase.TestOutcome.ReadToInputTypesNotSupported) },
      };
    }

    // Only equatable for the sake of testing; that's not a requirement of the serialization code.
    [FirestoreData]
    internal class GameResult : IEquatable<GameResult> {
      [FirestoreProperty("name")]
      public string Name { get; set; }
      [FirestoreProperty] // No property name specified, so field will be Score
      public int Score { get; set; }

      public override int GetHashCode() { return Name.GetHashCode() ^ Score; }

      public override bool Equals(object obj) { return Equals(obj as GameResult); }

      public bool Equals(GameResult other) { return other != null && other.Name == Name && other.Score == Score; }
    }

    [FirestoreData]
    internal class NullableContainer : IEquatable<NullableContainer> {
      [FirestoreProperty]
      public long? NullableValue { get; set; }

      public override int GetHashCode() { return (int)NullableValue.GetValueOrDefault().GetHashCode(); }

      public override bool Equals(object obj) { return Equals(obj as NullableContainer); }

      public bool Equals(NullableContainer other) { return other != null && other.NullableValue == NullableValue; }
      public override string ToString() { return String.Format("NullableContainer: {0}", NullableValue.GetValueOrDefault()); }
    }

    [FirestoreData]
    internal class NullableEnumContainer : IEquatable<NullableEnumContainer> {
      [FirestoreProperty]
      public Int32Enum? NullableValue { get; set; }

      public override int GetHashCode() { return (int)NullableValue.GetValueOrDefault().GetHashCode(); }

      public override bool Equals(object obj) { return Equals(obj as NullableEnumContainer); }

      public bool Equals(NullableEnumContainer other) { return other != null && other.NullableValue == NullableValue; }
    }

    [FirestoreData]
    internal class DictionaryInterfaceContainer : IEquatable<DictionaryInterfaceContainer> {
      [FirestoreProperty]
      public IDictionary<string, int> Integers { get; set; }

      public override int GetHashCode() { return Integers.Sum(pair => pair.Key.GetHashCode() + pair.Value); }

      public override bool Equals(object obj) { return Equals(obj as DictionaryInterfaceContainer); }

      public bool Equals(DictionaryInterfaceContainer other) {
        if (other == null) {
          return false;
        }
        if (Integers == other.Integers) {
          return true;
        }
        if (Integers == null || other.Integers == null) {
          return false;
        }
        if (Integers.Count != other.Integers.Count) {
          return false;
        }
        int otherValue;
        return Integers.All(pair => other.Integers.TryGetValue(pair.Key, out otherValue) && pair.Value == otherValue);
      }
    }

    internal enum SByteEnum : sbyte {
      MinValue = sbyte.MinValue,
      MaxValue = sbyte.MaxValue
    }

    internal enum Int16Enum : short {
      MinValue = short.MinValue,
      MaxValue = short.MaxValue
    }

    internal enum Int32Enum : int {
      MinValue = int.MinValue,
      MaxValue = int.MaxValue
    }

    internal enum Int64Enum : long {
      MinValue = long.MinValue,
      MaxValue = long.MaxValue
    }

    internal enum ByteEnum : byte {
      MinValue = byte.MinValue,
      MaxValue = byte.MaxValue
    }

    internal enum UInt16Enum : ushort {
      MinValue = ushort.MinValue,
      MaxValue = ushort.MaxValue
    }

    internal enum UInt32Enum : uint {
      MinValue = uint.MinValue,
      MaxValue = uint.MaxValue
    }

    internal enum UInt64Enum : ulong {
      MinValue = ulong.MinValue,
      MaxRepresentableValue = long.MaxValue
    }

    [FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<CustomConversionEnum>))]
    internal enum CustomConversionEnum {
      Foo = 1,
      Bar = 2
    }

    [FirestoreData]
    public sealed class ModelWithEnums {
      [FirestoreProperty]
      public CustomConversionEnum EnumDefaultByName { get; set; }

      [FirestoreProperty(ConverterType = typeof(FirestoreEnumNameConverter<Int32Enum>))]
      public Int32Enum EnumAttributedByName { get; set; }

      [FirestoreProperty]
      public Int32Enum EnumByNumber { get; set; }

      public override bool Equals(object obj) {
        if (!(obj is ModelWithEnums)) {
          return false;
        }
        ModelWithEnums other = obj as ModelWithEnums;
        return EnumDefaultByName == other.EnumDefaultByName &&
            EnumAttributedByName == other.EnumAttributedByName &&
            EnumByNumber == other.EnumByNumber;
      }

      public override int GetHashCode() { return 0; }
    }

    [FirestoreData]
    public sealed class CustomUser {
      [FirestoreProperty]
      public int HighScore { get; set; }
      [FirestoreProperty]
      public string Name { get; set; }
      [FirestoreProperty] public List<string> Emails { get; set; }

      public override bool Equals(object obj) {
        if (!(obj is CustomUser)) {
          return false;
        }

        CustomUser other = (CustomUser)obj;
        return HighScore == other.HighScore && Name == other.Name &&
               Emails.SequenceEqual(other.Emails);
      }

      public override int GetHashCode() {
        return 0;
      }
    }

    [FirestoreData]
    public sealed class CustomUserEnumerableEmails {
      [FirestoreProperty]
      public int HighScore { get; set; }
      [FirestoreProperty] public string Name { get; set; }
      [FirestoreProperty] public IEnumerable<string> Emails { get; set; }

      public override bool Equals(object obj) {
        if (!(obj is CustomUserEnumerableEmails)) {
          return false;
        }

        CustomUserEnumerableEmails other = (CustomUserEnumerableEmails)obj;
        return HighScore == other.HighScore && Name == other.Name &&
               Emails.SequenceEqual(other.Emails);
      }

      public override int GetHashCode() {
        return 0;
      }
    }

    [FirestoreData]
    public sealed class CustomUserSetEmails {
      [FirestoreProperty]
      public int HighScore { get; set; }
      [FirestoreProperty] public string Name { get; set; }
      [FirestoreProperty] public HashSet<string> Emails { get; set; }

      public override bool Equals(object obj) {
        if (!(obj is CustomUserSetEmails)) {
          return false;
        }

        CustomUserSetEmails other = (CustomUserSetEmails)obj;
        return HighScore == other.HighScore && Name == other.Name &&
               Emails.SequenceEqual(other.Emails);
      }

      public override int GetHashCode() {
        return 0;
      }
    }

    [FirestoreData(ConverterType = typeof(CustomUserSetEmailsConverter))]
    public sealed class CustomUserSetEmailsWithConverter {
      [FirestoreProperty]
      public int HighScore { get; set; }
      [FirestoreProperty] public string Name { get; set; }
      [FirestoreProperty] public HashSet<string> Emails { get; set; }

      public override bool Equals(object obj) {
        if (!(obj is CustomUserSetEmailsWithConverter)) {
          return false;
        }

        var other = (CustomUserSetEmailsWithConverter)obj;
        return HighScore == other.HighScore && Name == other.Name &&
               Emails.SequenceEqual(other.Emails);
      }

      public override int GetHashCode() {
        return 0;
      }
    }

    public class CustomUserSetEmailsConverter
        : FirestoreConverter<CustomUserSetEmailsWithConverter> {
      public override CustomUserSetEmailsWithConverter FromFirestore(object value) {
        if (value == null) {
          throw new ArgumentNullException("value");  // Shouldn't happen
        }

        var map = (Dictionary<string, object>)value;
        var emails = (List<object>)map["Emails"];
        var emailSet = new HashSet<string>(emails.Select(o => o.ToString()));
        return new CustomUserSetEmailsWithConverter { Name = (string)map["Name"],
                                                      HighScore = Convert.ToInt32(map["HighScore"]),
                                                      Emails = emailSet };
      }

      public override object ToFirestore(CustomUserSetEmailsWithConverter value) {
        return new Dictionary<string, object> {
          { "Name", value.Name },
          { "HighScore", value.HighScore },
          { "Emails", value.Emails },
        };
      }
    }

    [FirestoreData(ConverterType = typeof(EmailConverter))]
    public sealed class Email {
      public readonly string Address;
      public Email(string address) { Address = address; }
    }

    public class EmailConverter : FirestoreConverter<Email> {
      public override Email FromFirestore(object value) {
        if (value == null) {
          throw new ArgumentNullException("value"); // Shouldn't happen
        } else if (value is string) {
          return new Email(value as string);
        } else {
          throw new ArgumentException(String.Format("Unexpected data: {}", value.GetType()));
        }
      }
      public override object ToFirestore(Email value) { return value == null ? null : value.Address; }
    }

    [FirestoreData]
    public class GuidPair {
      [FirestoreProperty]
      public string Name { get; set; }

      [FirestoreProperty(ConverterType = typeof(GuidConverter))]
      public Guid Guid { get; set; }

      [FirestoreProperty(ConverterType = typeof(GuidConverter))]
      public Guid? GuidOrNull { get; set; }
    }

    // Like GuidPair, but without the converter specified - it has to come
    // from a converter registry instead.
    [FirestoreData]
    public class GuidPair2 {
      [FirestoreProperty]
      public string Name { get; set; }

      [FirestoreProperty]
      public Guid Guid { get; set; }

      [FirestoreProperty]
      public Guid? GuidOrNull { get; set; }
    }

    public class GuidConverter : FirestoreConverter<Guid> {
      public override Guid FromFirestore(object value) {
        if (value == null) {
          throw new ArgumentNullException("value"); // Shouldn't happen
        } else if (value is string) {
          return new Guid(value as string);
        } else {
          throw new ArgumentException(String.Format("Unexpected data: {0}", value.GetType()));
        }
      }
      public override object ToFirestore(Guid value) { return value.ToString("N"); }
    }

    // Only equatable for the sake of testing; that's not a requirement of the serialization code.
    [FirestoreData(ConverterType = typeof(CustomPlayerConverter))]
    public class CustomPlayer : IEquatable<CustomPlayer> {
      public string Name { get; set; }
      public int Score { get; set; }

      public override int GetHashCode() { return Name.GetHashCode() ^ Score; }
      public override bool Equals(object obj) { return Equals(obj as CustomPlayer); }
      public bool Equals(CustomPlayer other) { return other != null && other.Name == Name && other.Score == Score; }
    }

    public class CustomPlayerConverter : FirestoreConverter<CustomPlayer> {
      public override CustomPlayer FromFirestore(object value) {
        var map = (IDictionary<string, object>)value;
        return new CustomPlayer {
          Name = (string)map["PlayerName"],
          // Unbox to long, then convert to int.
          Score = (int)(long)map["PlayerScore"]
        };
      }

      public override object ToFirestore(CustomPlayer value) {
        return new Dictionary<string, object>
        {
                    { "PlayerName", value.Name },
                    { "PlayerScore", value.Score }
                };
      }
    }

    [FirestoreData(ConverterType = typeof(CustomValueTypeConverter))]
    internal struct CustomValueType : IEquatable<CustomValueType> {
      public readonly string Name;
      public readonly int Value;

      public CustomValueType(string name, int value) {
        Name = name;
        Value = value;
      }

      public override int GetHashCode() { return Name.GetHashCode() + Value; }
      public override bool Equals(object obj) { return obj is CustomValueType && Equals((CustomValueType)obj); }
      public bool Equals(CustomValueType other) { return Name == other.Name && Value == other.Value; }
      public override string ToString() { return String.Format("CustomValueType: {0}", new { Name, Value }); }
    }

    internal class CustomValueTypeConverter : FirestoreConverter<CustomValueType> {
      public override CustomValueType FromFirestore(object value) {
        var dictionary = (IDictionary<string, object>)value;
        return new CustomValueType(
            (string)dictionary["Name"],
            (int)(long)dictionary["Value"]
        );
      }

      public override object ToFirestore(CustomValueType value) {
        return new Dictionary<string, object>
        {
                    { "Name", value.Name },
                    { "Value", value.Value }
                };
      }
    }

    [FirestoreData]
    internal struct StructModel : IEquatable<StructModel> {
      [FirestoreProperty]
      public string Name { get; set; }
      [FirestoreProperty]
      public int Value { get; set; }

      public override int GetHashCode() { return Name.GetHashCode() + Value; }
      public override bool Equals(object obj) { return obj is StructModel && Equals((StructModel)obj); }
      public bool Equals(StructModel other) { return Name == other.Name && Value == other.Value; }

      public override string ToString() { return String.Format("StructModel: {0}", new { Name, Value }); }
    }
  }
}
