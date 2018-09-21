// Copyright 2018 Google Inc. All rights reserved.
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

namespace Firebase.Sample.Functions {
  using System;
  using System.Collections;

  // Helpers for writing tests.
  public class Utils {
    // A delegate callers can pass in to get back log messages.
    public delegate void Reporter(string message);

    // Takes a string and returns it as a quoted string with escapes.
    private static string Quote(string s) {
      s = s.Replace("\"", "\\\"");
      s = s.Replace("\n", "\\n");
      return "\"" + s + "\"";
    }

    // Converts an arbitrary map, list, or primitive to a string suitable for
    // outputting for debug purposes, such as test output.
    public static string DebugString(object o, string indent = "") {
      if (o == null) {
        return "null";
      }
      if (o is IDictionary) {
        var d = (IDictionary)o;
        string s = "{\n";
        foreach (var key in d.Keys) {
          s += String.Format("{0}  {1}: {2},\n", indent, Quote(key.ToString()),
            DebugString(d[key], indent + "  "));
        }
        s += "}";
        return s;
      }
      if (o is IList) {
        var l = (IList)o;
        string s = "[\n";
        foreach (var i in l) {
          s += String.Format("{0}  {1},\n", indent,
            DebugString(i, indent + "  "));
        }
        s += "]";
        return s;
      }
      if (o is string) {
        return Quote((string)o);
      }
      return String.Format("({0}) {1}", o.GetType(), o);
    }

    // Returns true iff the given dictionaries are deeply equal.
    // Sends messages to reporter with details of any differences.
    private static bool DeepEqualsDictionary(IDictionary d1, IDictionary d2,
      Reporter reporter) {
      foreach (var key in d1.Keys) {
        if (!d2.Contains(key)) {
          reporter("Removed key " + Quote(key.ToString()));
          return false;
        }
        if (!DeepEquals(d1[key], d2[key], reporter)) {
          reporter("  for key " + Quote(key.ToString()));
          return false;
        }
      }
      foreach (var key in d2.Keys) {
        if (!d1.Contains(key)) {
          reporter("Added key " + Quote(key.ToString()));
          return false;
        }
      }
      return true;
    }

    // Returns true iff the given lists are deeply equal.
    // Sends messages to reporter with details of any differences.
    private static bool DeepEqualsList(IList l1, IList l2, Reporter reporter) {
      if (l1.Count != l2.Count) {
        reporter("Size mismatch");
        return false;
      }
      for (int i = 0; i < l1.Count; i++) {
        if (!DeepEquals(l1[i], l2[i], reporter)) {
          reporter(String.Format("  for index {0}", i));
          return false;
        }
      }
      return true;
    }

    // Returns true iff the given objects are deeply equal.
    // Sends messages to reporter with details of any differences.
    public static bool DeepEquals(object o1, object o2, Reporter reporter) {
      if (o1 is IDictionary && o2 is IDictionary) {
        return DeepEqualsDictionary((IDictionary)o1, (IDictionary)o2, reporter);
      }
      if (o1 is IList && o2 is IList) {
        return DeepEqualsList((IList)o1, (IList)o2, reporter);
      }
      if (!object.Equals(o1, o2)) {
        reporter(String.Format("{0} != {1}", DebugString(o1), DebugString(o2)));
        return false;
      }
      return true;
    }
  }
}