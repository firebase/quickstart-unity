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

    public static IEnumerable<object[]> TestData(FirebaseFirestore database) {
      // TODO(b/153551034): Refactor this using structs or classes.
      return new List<object[]>
      {
                // Simple types
                { new object[] { null, null } },
                { new object[] { true, true } },
                { new object[] { false, false } },
                { new object[] { "test", "test" } },
                { new object[] { (byte) 1, 1L } },
                { new object[] { (sbyte) 1, 1L } },
                { new object[] { (short) 1, 1L } },
                { new object[] { (ushort) 1, 1L } },
                { new object[] { 1, 1L } },
                { new object[] { 1U, 1L } },
                { new object[] { 1L, 1L } },
                { new object[] { 1UL, 1L } },
                { new object[] { 1.5F, 1.5D } },
                { new object[] { float.PositiveInfinity, double.PositiveInfinity } },
                { new object[] { float.NegativeInfinity, double.NegativeInfinity } },
                { new object[] { float.NaN, double.NaN } },
                { new object[] { 1.5D, 1.5D } },
                { new object[] { double.PositiveInfinity, double.PositiveInfinity } },
                { new object[] { double.NegativeInfinity, double.NegativeInfinity } },
                { new object[] { double.NaN, double.NaN } },

                // Min/max values of each integer type
                { new object[] { byte.MinValue, (long) byte.MinValue } },
                { new object[] { byte.MaxValue, (long) byte.MaxValue } },
                { new object[] { sbyte.MinValue, (long) sbyte.MinValue } },
                { new object[] { sbyte.MaxValue, (long) sbyte.MaxValue } },
                { new object[] { short.MinValue, (long) short.MinValue } },
                { new object[] { short.MaxValue, (long) short.MaxValue } },
                { new object[] { ushort.MinValue, (long) ushort.MinValue } },
                { new object[] { ushort.MaxValue, (long) ushort.MaxValue } },
                { new object[] { int.MinValue, (long) int.MinValue } },
                { new object[] { int.MaxValue, (long) int.MaxValue } },
                { new object[] { uint.MinValue, (long) uint.MinValue } },
                { new object[] { uint.MaxValue, (long) uint.MaxValue } },
                { new object[] { long.MinValue, long.MinValue } },
                { new object[] { long.MaxValue, long.MaxValue } },
                // We don't cover the whole range of ulong
                { new object[] { (ulong) 0, 0L } },
                { new object[] { (ulong) long.MaxValue, long.MaxValue } },

                // Enum types
                { new object[] { ByteEnum.MinValue, (long) byte.MinValue } },
                { new object[] { ByteEnum.MaxValue, (long) byte.MaxValue } },
                { new object[] { SByteEnum.MinValue, (long) sbyte.MinValue } },
                { new object[] { SByteEnum.MaxValue, (long) sbyte.MaxValue } },
                { new object[] { Int16Enum.MinValue, (long) short.MinValue } },
                { new object[] { Int16Enum.MaxValue, (long) short.MaxValue } },
                { new object[] { UInt16Enum.MinValue, (long) ushort.MinValue } },
                { new object[] { UInt16Enum.MaxValue, (long) ushort.MaxValue } },
                { new object[] { Int32Enum.MinValue, (long) int.MinValue } },
                { new object[] { Int32Enum.MaxValue, (long) int.MaxValue } },
                { new object[] { UInt32Enum.MinValue, (long) uint.MinValue } },
                { new object[] { UInt32Enum.MaxValue, (long) uint.MaxValue } },
                { new object[] { Int64Enum.MinValue, (long) long.MinValue } },
                { new object[] { Int64Enum.MaxValue, (long) long.MaxValue } },
                // We don't cover the whole range of ulong
                { new object[] { UInt64Enum.MinValue, (long) 0 } },
                { new object[] { UInt64Enum.MaxRepresentableValue, (long) long.MaxValue } },
                { new object[] { CustomConversionEnum.Foo, "Foo" } },
                { new object[] { CustomConversionEnum.Bar, "Bar" } },

                // Timestamps
                { new object[] { Timestamp.FromDateTime(dateTime),
                    Timestamp.FromDateTime(dateTime) } },
                { new object[] { dateTime,
                    Timestamp.FromDateTime(dateTime) } },
                { new object[] { dateTimeOffset,
                    Timestamp.FromDateTimeOffset(dateTimeOffset) } },

                // Blobs
                { new object[] { new byte[] { 1, 2, 3, 4 },
                    Blob.CopyFrom(new byte[] { 1, 2, 3, 4 } ) } },
                { new object[] { Blob.CopyFrom(new byte[] { 1, 2, 3, 4 } ),
                    Blob.CopyFrom(new byte[] { 1, 2, 3, 4 } ) } },

                // GeoPoints
                { new object[] { new GeoPoint(1.5, 2.5), new GeoPoint(1.5, 2.5) } },

                // Array values
                { new object[] { new string[] { "x", "y" }, new List<object> { "x", "y" } } },
                { new object[] { new List<string> { "x", "y" }, new List<object> { "x", "y" } } },
                { new object[] { new int[] { 3, 4 }, new List<object> { 3L, 4L } } },
                // Deliberately DateTime rather than Timestamp here - we need to be able to detect the element type to perform the
                // per-element deserialization correctly
                { new object[] { new List<DateTime> { dateTime, dateTime },
                    new List<object> { Timestamp.FromDateTime(dateTime), Timestamp.FromDateTime(dateTime) } } },

                // Map values (that can be deserialized again): dictionaries, attributed types, expandos (which are
                // just dictionaries), custom serialized map-like values

                // Dictionaries
                { new object[] { new Dictionary<string, byte> { { "A", 10 }, { "B", 20 } },
                    new Dictionary<string, object> { { "A", 10L }, { "B", 20L } } } },
                { new object[] { new Dictionary<string, int> { { "A", 10 }, { "B", 20 } },
                    new Dictionary<string, object> { { "A", 10L }, { "B", 20L } } } },
                { new object[] { new Dictionary<string, object> { { "name", "Jon" }, { "score", 10L } },
                    new Dictionary<string, object> { { "name", "Jon" }, { "score", 10L } } } },
                // Attributed type (each property has an attribute)
                { new object[] { new GameResult { Name = "Jon", Score = 10 },
                    new Dictionary<string, object> { { "name", "Jon" }, { "Score", 10L } } } },
                // Attributed type contained in a dictionary
                { new object[] {
                    new Dictionary<string, GameResult>
                      { { "result", new GameResult { Name = "Jon", Score = 10 } } },
                    new Dictionary<string, object>
                      { {"result",
                         new Dictionary<string, object>
                           { { "name", "Jon" }, { "Score", 10L } } } } }},
                // Attributed type containing a dictionary
                { new object[] { new DictionaryInterfaceContainer { Integers = new Dictionary<string, int> { { "A", 10 }, { "B", 20 } } },
                    new Dictionary<string, object> {
                        { "Integers", new Dictionary<string, object> { { "A", 10L }, { "B", 20L } } }
                    } } },
                // Attributed type serialized and deserialized by CustomPlayerConverter
                { new object[] { new CustomPlayer { Name = "Amanda", Score = 15 },
                    new Dictionary<string, object> { { "PlayerName", "Amanda" }, { "PlayerScore", 15L } } } },

                // Attributed value type serialized and deserialized by CustomValueTypeConverter
                { new object[] { new CustomValueType("xyz", 10),
                    new Dictionary<string, object> { { "Name", "xyz" }, { "Value", 10L } } } },

                // Attributed type with enums (name and number)
                { new object[] { new ModelWithEnums { EnumDefaultByName = CustomConversionEnum.Foo, EnumAttributedByName = Int32Enum.MinValue, EnumByNumber = Int32Enum.MaxValue },
                    new Dictionary<string, object> {
                        { "EnumDefaultByName", "Foo" },
                        { "EnumAttributedByName", "MinValue" },
                        { "EnumByNumber", (long)int.MaxValue }
                    } } },

                // Attributed struct
                { new object[] { new StructModel { Name = "xyz", Value = 10 },
                    new Dictionary<string, object> { { "Name", "xyz" }, { "Value", 10L } } } },

                // Document references
                { new object[] { database.Document("a/b"), database.Document("a/b") } },
            };
    }


    public static IEnumerable<object[]> UnsupportedTestData() {
      return new List<object[]>
      {
        // Nullable type handling
        { new object[] { new NullableContainer { NullableValue = 10 },
          new Dictionary<string, object> { { "NullableValue", 10L } } } },
        { new object[] { new NullableEnumContainer { NullableValue = (Int32Enum) 10 },
          new Dictionary<string, object> { { "NullableValue", 10L } } } },
        // This one fails because the `NullableContainer` it gets back has a random value
        // while it should be null.
        { new object[] { new NullableContainer { NullableValue = null },
          new Dictionary<string, object> { { "NullableValue", null } } } },
        { new object[] { new NullableEnumContainer { NullableValue = null },
          new Dictionary<string, object> { { "NullableValue", null } } } },
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
      [FirestoreProperty]
      public Email Email { get; set; }
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
        var map = (IDictionary<string, object>) value;
        return new CustomPlayer {
          Name = (string) map["PlayerName"],
          // Unbox to long, then convert to int.
          Score = (int) (long) map["PlayerScore"]
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
      public override bool Equals(object obj) { return obj is CustomValueType && Equals((CustomValueType) obj); }
      public bool Equals(CustomValueType other) { return Name == other.Name && Value == other.Value; }
      public override string ToString() { return String.Format("CustomValueType: {0}", new { Name, Value }); }
    }

    internal class CustomValueTypeConverter : FirestoreConverter<CustomValueType> {
      public override CustomValueType FromFirestore(object value) {
        var dictionary = (IDictionary<string, object>) value;
        return new CustomValueType(
            (string) dictionary["Name"],
            (int) (long) dictionary["Value"]
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
      public override bool Equals(object obj) { return obj is StructModel && Equals((StructModel) obj); }
      public bool Equals(StructModel other) { return Name == other.Name && Value == other.Value; }

      public override string ToString() { return String.Format("StructModel: {0}", new { Name, Value }); }
    }
  }
}
