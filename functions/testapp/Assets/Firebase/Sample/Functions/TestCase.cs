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
  using Firebase;
  using Firebase.Extensions;
  using Firebase.Functions;
  using System;
  using System.Threading.Tasks;

  public class TestCase {
    // The name of the HTTPS callable function to call.
    public string Name { get; private set; }

    // The parameters to pass to the function.
    public object Input { get; private set; }

    // The data expected to be returned from the function.
    public object ExpectedData { get; private set; }

    // The error code expected to be returned from the function.
    public FunctionsErrorCode ExpectedError { get; private set; }

    public TestCase(string name, object input, object expectedResult,
        FunctionsErrorCode expectedError = FunctionsErrorCode.None) {
      Name = name;
      Input = input;
      ExpectedData = expectedResult;
      ExpectedError = expectedError;
    }

    // Runs the given test and returns whether it passed.
    public Task RunAsync(FirebaseFunctions functions,
        Utils.Reporter reporter) {
      var func = functions.GetHttpsCallable(Name);
      return func.CallAsync(Input).ContinueWithOnMainThread((task) => {
        if (ExpectedError == FunctionsErrorCode.None) {
          // We expected no error.
          if (task.IsFaulted) {
            // The function unexpectedly failed.
            throw task.Exception;
          }
          // The function succeeded.
          if (!Utils.DeepEquals(ExpectedData, task.Result.Data, reporter)) {
            throw new Exception(String.Format("Incorrect result. Got {0}. Want {1}.",
              Utils.DebugString(task.Result.Data),
              Utils.DebugString(ExpectedData)));
          }
          return;
        }

        // The function was expected to fail.
        FunctionsException e = null;
        foreach (var inner in task.Exception.InnerExceptions) {
          if (inner is FunctionsException) {
            e = (FunctionsException)inner;
            break;
          }
        }
        if (e == null) {
          // We didn't get a proper Functions Exception.
          throw task.Exception;
        }

        if (e.ErrorCode != ExpectedError) {
          // The code wasn't right.
          throw new Exception(String.Format("Error {0}: {1}", e.ErrorCode, e.Message));
        }
        reporter(String.Format("  Got expected error {0}: {1}", e.ErrorCode,
          e.Message));
      });
    }
  }
}
