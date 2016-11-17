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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class UIHandler : MonoBehaviour {

  Firebase.Auth.FirebaseAuth auth;
  Firebase.Auth.FirebaseUser user;

  public GUISkin fb_GUISkin;
  private string logText = "";
  private string email = "";
  private string password = "";
  // Enable / disable password input box.
  // NOTE: In some versions of Unity the password input box does not work in
  // iOS simulators.
  public bool usePasswordInput = false;
  private Vector2 controlsScrollViewVector = Vector2.zero;
  private Vector2 scrollViewVector = Vector2.zero;
  bool UIEnabled = true;

  const int kMaxLogSize = 16382;
  Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

  // When the app starts, check to make sure that we have
  // the required dependencies to use Firebase, and if not,
  // add them if possible.
  void Start() {
    dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
    if (dependencyStatus != Firebase.DependencyStatus.Available) {
      Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
        dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          // This should never happen if we're only using Firebase Analytics.
          // It does not rely on any external dependencies.
          Debug.LogError(
              "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    } else {
      InitializeFirebase();
    }
  }

  // Handle initialization of the necessary firebase modules:
  void InitializeFirebase() {
    DebugLog("Setting up Firebase Auth");
    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    auth.StateChanged += AuthStateChanged;
  }

  // Exit if escape (or back, on mobile) is pressed.
  void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
  }

  void OnDestroy() {
    auth.StateChanged -= AuthStateChanged;
    auth = null;
  }

  void DisableUI() {
    UIEnabled = false;
  }

  void EnableUI() {
    UIEnabled = true;
  }

  // Output text to the debug log text field, as well as the console.
  public void DebugLog(string s) {
    Debug.Log(s);
    logText += s + "\n";

    while (logText.Length > kMaxLogSize) {
      int index = logText.IndexOf("\n");
      logText = logText.Substring(index + 1);
    }
    scrollViewVector.y = int.MaxValue;
  }

  // Track state changes of the auth object.
  void AuthStateChanged(object sender, System.EventArgs eventArgs) {
    if (auth.CurrentUser != user) {
      if (user == null && auth.CurrentUser != null) {
        DebugLog("Signed in " + auth.CurrentUser.DisplayName);
      } else if (user != null && auth.CurrentUser == null) {
        DebugLog("Signed out " + user.DisplayName);
      }
      user = auth.CurrentUser;
    }
  }

  public void CreateUser() {
    DebugLog(String.Format("Attempting to create user {0}...", email));
    DisableUI();

    auth.CreateUserWithEmailAndPasswordAsync(email, password)
      .ContinueWith(HandleCreateResult);
  }

  void HandleCreateResult(Task<Firebase.Auth.FirebaseUser> authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("User Creation canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("User Creation encountered an error.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DebugLog("User Creation completed.");
      if (auth.CurrentUser != null) {
        DebugLog("User Info: " + auth.CurrentUser.Email + "   " + auth.CurrentUser.ProviderId);
      }
      DebugLog("Signing out.");
      auth.SignOut();
    }
  }

  public void Signin() {
    DebugLog(String.Format("Attempting to sign in as {0}...", email));
    DisableUI();
    auth.SignInWithEmailAndPasswordAsync(email, password)
      .ContinueWith(HandleSigninResult);
  }

  // This is functionally equivalent to the Signin() function.  However, it
  // illustrates the use of Credentials, which can be aquired from many
  // different sources of authentication.
  public void SigninWithCredential() {
    DebugLog(String.Format("Attempting to sign in as {0}...", email));
    DisableUI();
    Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
    auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
  }

  // Attempt to sign in anonymously.
  public void SigninAnonymously() {
    DebugLog("Attempting to sign anonymously...");
    DisableUI();
    auth.SignInAnonymouslyAsync().ContinueWith(HandleSigninResult);
  }

  void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("SignIn canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("Login encountered an error.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DebugLog("Login completed.");
      DebugLog("Signing out.");
      auth.SignOut();
    }
  }

  public void DeleteUser() {
    DebugLog(String.Format("Attempting to delete user {0}...", email));
    DisableUI();
    auth.SignInWithEmailAndPasswordAsync(email, password)
      .ContinueWith(HandleDeleteSigninResult);
  }

  void HandleDeleteSigninResult(Task<Firebase.Auth.FirebaseUser> authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("Delete signin canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("Delete signin encountered an error while signing in.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DisableUI();
      auth.CurrentUser.DeleteAsync().ContinueWith(HandleDeleteResult);
      DebugLog("Signed in - deleting user.");
    }
  }

  void HandleDeleteResult(Task authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("Delete canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("Delete encountered an error while signing in.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DebugLog("Delete completed.");
    }
  }

  // Render the log output in a scroll view.
  void GUIDisplayLog() {
    scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
    GUILayout.Label(logText);
    GUILayout.EndScrollView();
  }

  // Render the buttons and other controls.
  void GUIDisplayControls(){
    if (UIEnabled) {
      controlsScrollViewVector =
          GUILayout.BeginScrollView(controlsScrollViewVector);
      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      GUILayout.Label("Email:", GUILayout.Width(Screen.width * 0.20f));
      email = GUILayout.TextField(email);
      GUILayout.EndHorizontal();

      GUILayout.Space(20);

      GUILayout.BeginHorizontal();
      GUILayout.Label("Password:", GUILayout.Width(Screen.width * 0.20f));
      password = usePasswordInput ? GUILayout.PasswordField(password, '*') :
          GUILayout.TextField(password);
      GUILayout.EndHorizontal();

      GUILayout.Space(20);

      if (GUILayout.Button("Create User")) {
        CreateUser();
      }
      if (GUILayout.Button("Sign In Anonymously")) {
        SigninAnonymously();
      }
      if (GUILayout.Button("Sign In With Email")) {
        Signin();
      }
      if (GUILayout.Button("Sign In With Credentials")) {
        SigninWithCredential();
      }
      if (GUILayout.Button("Delete User")) {
        DeleteUser();
      }
      GUILayout.EndVertical();
      GUILayout.EndScrollView();
    }
  }

  // Render the GUI:
  void OnGUI() {
    GUI.skin = fb_GUISkin;
    if (dependencyStatus != Firebase.DependencyStatus.Available) {
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
