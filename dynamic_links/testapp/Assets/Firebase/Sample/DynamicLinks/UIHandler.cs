// Copyright 2017 Google Inc. All rights reserved.
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

namespace Firebase.Sample.DynamicLinks {
  using Firebase;
  using Firebase.DynamicLinks;
  using Firebase.Extensions;
  using System;
  using System.Collections;
  using System.Threading.Tasks;
  using UnityEngine;

  // Handler for UI buttons on the scene.  Also performs some
  // necessary setup (initializing the firebase app, etc) on
  // startup.
  public class UIHandler : MonoBehaviour {

    public GUISkin fb_GUISkin;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private Vector2 scrollViewVector = Vector2.zero;
    bool UIEnabled = true;
    private string logText = "";
    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    const string kInvalidDomainUriPrefix = "THIS_IS_AN_INVALID_DOMAIN";
    const string kDomainUriPrefixInvalidError =
      "kDomainUriPrefix is not valid, link shortening will fail.\n" +
      "To resolve this:\n" +
      "* Goto the Firebase console https://firebase.google.com/console/\n" +
      "* Click on the Dynamic Links tab\n" +
      "* Copy the domain e.g x20yz.app.goo.gl\n" +
      "* Replace the value of kDomainUriPrefix with the copied domain.\n";
    public bool firebaseInitialized = false;

    // IMPORTANT: You need to set this to a valid domain from the Firebase
    // console (see kDomainUriPrefixInvalidError for the details).
    public string kDomainUriPrefix = kInvalidDomainUriPrefix;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start() {
      FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          Debug.LogError(
            "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }

    // Exit if escape (or back, on mobile) is pressed.
    public virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase() {
      DynamicLinks.DynamicLinkReceived += OnDynamicLink;
      firebaseInitialized = true;
    }

    void OnDestroy() {
      DynamicLinks.DynamicLinkReceived -= OnDynamicLink;
    }

    // Display the dynamic link received by the application.
    void OnDynamicLink(object sender, EventArgs args) {
      var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
      DebugLog(String.Format("Received dynamic link {0}",
                             dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString));
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s) {
      print(s);
      logText += s + "\n";

      while (logText.Length > kMaxLogSize) {
        int index = logText.IndexOf("\n");
        logText = logText.Substring(index + 1);
      }

      scrollViewVector.y = int.MaxValue;
    }

    public void DisableUI() {
      UIEnabled = false;
    }

    public void EnableUI() {
      UIEnabled = true;
    }

    // Render the log output in a scroll view.
    void GUIDisplayLog() {
      scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
      GUILayout.Label(logText);
      GUILayout.EndScrollView();
    }

    DynamicLinkComponents CreateDynamicLinkComponents() {
#if UNITY_5_6_OR_NEWER
      string appIdentifier = Application.identifier;
#else
      string appIdentifier = Application.bundleIdentifier;
#endif

      return new DynamicLinkComponents(
        // The base Link.
        new System.Uri("https://google.com/abc"),
        // The dynamic link domain.
        kDomainUriPrefix) {
        GoogleAnalyticsParameters = new Firebase.DynamicLinks.GoogleAnalyticsParameters() {
          Source = "mysource",
          Medium = "mymedium",
          Campaign = "mycampaign",
          Term = "myterm",
          Content = "mycontent"
        },
        IOSParameters = new Firebase.DynamicLinks.IOSParameters(appIdentifier) {
          FallbackUrl = new System.Uri("https://mysite/fallback"),
          CustomScheme = "mycustomscheme",
          MinimumVersion = "1.2.3",
          IPadBundleId = appIdentifier,
          IPadFallbackUrl = new System.Uri("https://mysite/fallbackipad")
        },
        ITunesConnectAnalyticsParameters =
          new Firebase.DynamicLinks.ITunesConnectAnalyticsParameters() {
            AffiliateToken = "abcdefg",
            CampaignToken = "hijklmno",
            ProviderToken = "pq-rstuv"
          },
        AndroidParameters = new Firebase.DynamicLinks.AndroidParameters(appIdentifier) {
          FallbackUrl = new System.Uri("https://mysite/fallback"),
          MinimumVersion = 12
        },
        SocialMetaTagParameters = new Firebase.DynamicLinks.SocialMetaTagParameters() {
          Title = "My App!",
          Description = "My app is awesome!",
          ImageUrl = new System.Uri("https://mysite.com/someimage.jpg")
        },
      };
    }

    public Uri CreateAndDisplayLongLink() {
      var longLink = CreateDynamicLinkComponents().LongDynamicLink;
      DebugLog(String.Format("Long dynamic link {0}", longLink));
      return longLink;
    }

    public Task<ShortDynamicLink> CreateAndDisplayShortLinkAsync() {
      return CreateAndDisplayShortLinkAsync(new DynamicLinkOptions());
    }

    public Task<ShortDynamicLink> CreateAndDisplayUnguessableShortLinkAsync() {
      return CreateAndDisplayShortLinkAsync(new DynamicLinkOptions {
        PathLength = DynamicLinkPathLength.Unguessable
      });
    }

    private Task<ShortDynamicLink> CreateAndDisplayShortLinkAsync(DynamicLinkOptions options) {
      if (kDomainUriPrefix == kInvalidDomainUriPrefix) {
        DebugLog(kDomainUriPrefixInvalidError);
        var source = new TaskCompletionSource<ShortDynamicLink>();
        source.TrySetException(new Exception(kDomainUriPrefixInvalidError));
        return source.Task;
      }

      var components = CreateDynamicLinkComponents();
      return DynamicLinks.GetShortLinkAsync(components, options)
        .ContinueWithOnMainThread((task) => {
          if (task.IsCanceled) {
            DebugLog("Short link creation canceled");
          } else if (task.IsFaulted) {
            DebugLog(String.Format("Short link creation failed {0}", task.Exception.ToString()));
          } else {
            ShortDynamicLink link = task.Result;
            DebugLog(String.Format("Generated short link {0}", link.Url));
            var warnings = new System.Collections.Generic.List<string>(link.Warnings);
            if (warnings.Count > 0) {
              DebugLog("Warnings:");
              foreach (var warning in warnings) {
                DebugLog("  " + warning);
              }
            }
          }
          return task.Result;
        });
    }

    // Render the buttons and other controls.
    void GUIDisplayControls() {
      if (UIEnabled) {
        controlsScrollViewVector =
            GUILayout.BeginScrollView(controlsScrollViewVector);
        GUILayout.BeginVertical();

        if (GUILayout.Button("Display Long Link")) {
          CreateAndDisplayLongLink();
        }

        if (GUILayout.Button("Create Short Link")) {
          CreateAndDisplayShortLinkAsync();
        }

        if (GUILayout.Button("Create Unguessable Short Link")) {
          CreateAndDisplayUnguessableShortLinkAsync();
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
    }

    // Render the GUI:
    void OnGUI() {
      GUI.skin = fb_GUISkin;
      if (dependencyStatus != DependencyStatus.Available) {
        GUILayout.Label("One or more Firebase dependencies are not present.");
        GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
        return;
      }
      Rect logArea, controlArea;

      if (Screen.width < Screen.height) {
        // Portrait mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width, Screen.height * 0.5f);
        logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width, Screen.height * 0.5f);
      } else {
        // Landscape mode
        controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
        logArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f, Screen.height);
      }

      GUILayout.BeginArea(logArea);
      GUIDisplayLog();
      GUILayout.EndArea();

      GUILayout.BeginArea(controlArea);
      GUIDisplayControls();
      GUILayout.EndArea();
    }
  }
}
