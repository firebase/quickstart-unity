// Copyright 2016 Google Inc. All rights reserved.
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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Firebase;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {

    public Text outputText;
    App app;

    public void DebugLog(string s) {
        print(s);
        outputText.text += s + "\n";
    }

    // When the app starts, create a firebase app object,
    // set the user property and id, and enable analytics.
    void Start() {
        DebugLog("Setting up firebase...");
        AppOptions ops = new AppOptions();

        DebugLog(String.Format("Created the AppOptions, with appID: {0}", ops.AppID));

        app = App.Create(ops);

        DebugLog(String.Format("Created the firebase app {0}", app));
        Analytics.Initialize(app);
        DebugLog("Initialized the firebase analytics API");

        DebugLog("Enabling data collection.");
        Analytics.SetAnalyticsCollectionEnabled(true);

        DebugLog("Set user properties.");
        // Set the user's sign up method.
        Analytics.SetUserProperty(Analytics.UserPropertySignUpMethod, "Google");
        // Set the user ID.
        Analytics.SetUserId("uber_user_510");
    }

    // End our analytics session when the program exits.
    void OnDestroy() {
        Analytics.Terminate();
    }

    public void AnalyticsLogin() {
        // Log an event with no parameters.
        DebugLog ("Logging a login event.");
        Analytics.LogEvent (Analytics.EventLogin);
    }

    public void AnalyticsProgress() {
        // Log an event with no parameters.
        DebugLog ("Logging a progress event.");
        Analytics.LogEvent("progress", "percent", 0.4f);
    }

    public void AnalyticsScore() {
        // Log an event with no parameters.
        DebugLog ("Logging a post-score event.");
        Analytics.LogEvent(Analytics.EventPostScore, Analytics.ParameterScore, 42);
    }

    public void AnalyticsGroupJoin() {
        // Log an event with no parameters.
        DebugLog ("Logging a group join event.");
        Analytics.LogEvent(Analytics.EventJoinGroup, Analytics.ParameterGroupID,
            "spoon_welders");
    }

    public void AnalyticsLevelUp() {
        // Log an event with no parameters.
        DebugLog ("Logging a level up event.");
        Parameter[] LevelUpParameters = {
            new Parameter(Analytics.ParameterLevel, 5),
            new Parameter(Analytics.ParameterCharacter, "mrspoon"),
            new Parameter("hit_accuracy", 3.14f)
        };
        Analytics.LogEvent(Analytics.EventLevelUp, LevelUpParameters);
    }
}
