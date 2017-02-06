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
using System.Collections.Generic;
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
  private string displayName = "";
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
    AuthStateChanged(this, null);
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

  // Display user information.
  void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel) {
    string indent = new String(' ', indentLevel * 2);
    var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
    foreach (var property in userProperties) {
      if (!String.IsNullOrEmpty(property.Value)) {
        DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
      }
    }
  }

  // Track state changes of the auth object.
  void AuthStateChanged(object sender, System.EventArgs eventArgs) {
    if (auth.CurrentUser != user) {
      bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
      if (!signedIn && user != null) {
        DebugLog("Signed out " + user.UserId);
      }
      user = auth.CurrentUser;
      if (signedIn) {
        DebugLog("Signed in " + user.UserId);
        displayName = user.DisplayName ?? "";
        DisplayUserInfo(user, 1);
        DebugLog("  Anonymous: " + user.IsAnonymous);
        DebugLog("  Email Verified: " + user.IsEmailVerified);
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        if (providerDataList.Count > 0) {
          DebugLog("  Provider Data:");
          foreach (var providerData in user.ProviderData) {
            DisplayUserInfo(providerData, 2);
          }
        }
      }
    }
  }

  // Log the result of the specified task, returning true if the task
  // completed successfully, false otherwise.
  bool LogTaskCompletion(Task task, string operation) {
    bool complete = false;
    if (task.IsCanceled) {
      DebugLog(operation + " canceled.");
    } else if (task.IsFaulted) {
      DebugLog(operation + " encounted an error.");
      DebugLog(task.Exception.ToString());
    } else if (task.IsCompleted) {
      DebugLog(operation + " completed");
      complete = true;
    }
    return complete;
  }

  public void CreateUser() {
    DebugLog(String.Format("Attempting to create user {0}...", email));
    DisableUI();

    // This passes the current displayName through to HandleCreateResult
    // so that it can be passed to UpdateUserProfile().  displayName will be
    // reset by AuthStateChanged() when the new user is created and signed in.
    string newDisplayName = displayName;
    auth.CreateUserWithEmailAndPasswordAsync(email, password)
      .ContinueWith((task) => HandleCreateResult(task, newDisplayName: newDisplayName));
  }

  void HandleCreateResult(Task<Firebase.Auth.FirebaseUser> authTask,
                          string newDisplayName = null) {
    EnableUI();
    if (LogTaskCompletion(authTask, "User Creation")) {
      if (auth.CurrentUser != null) {
        DebugLog(String.Format("User Info: {0}  {1}", auth.CurrentUser.Email,
                               auth.CurrentUser.ProviderId));
        UpdateUserProfile(newDisplayName: newDisplayName);
      }
    }
  }

  // Update the user's display name with the currently selected display name.
  public void UpdateUserProfile(string newDisplayName = null) {
    if (user == null) {
      DebugLog("Not signed in, unable to update user profile");
      return;
    }
    displayName = newDisplayName ?? displayName;
    DebugLog("Updating user profile");
    DisableUI();
    user.UpdateUserProfileAsync(new Firebase.Auth.UserProfile {
        DisplayName = displayName,
        PhotoUrl = user.PhotoUrl,
      }).ContinueWith(HandleUpdateUserProfile);
  }

  void HandleUpdateUserProfile(Task authTask) {
    EnableUI();
    if (LogTaskCompletion(authTask, "User profile")) {
      DisplayUserInfo(user, 1);
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
    LogTaskCompletion(authTask, "Sign-in");
  }

  public void ReloadUser() {
    if (user == null) {
      DebugLog("Not signed in, unable to reload user.");
      return;
    }
    DebugLog("Reload User Data");
    user.ReloadAsync().ContinueWith(HandleReloadUser);
  }

  void HandleReloadUser(Task authTask) {
    if (LogTaskCompletion(authTask, "Reload")) {
      DisplayUserInfo(user, 1);
    }
  }

  public void GetUserToken() {
    if (user == null) {
      DebugLog("Not signed in, unable to get token.");
      return;
    }
    DebugLog("Fetching user token");
    user.TokenAsync(false).ContinueWith(HandleGetUserToken);
  }

  void HandleGetUserToken(Task<string> authTask) {
    if (LogTaskCompletion(authTask, "User token fetch")) {
      DebugLog("Token = " + authTask.Result);
    }
  }

  public void SignOut() {
    DebugLog("Signing out.");
    auth.SignOut();
  }


  public void DeleteUser() {
    if (auth.CurrentUser != null) {
      DebugLog(String.Format("Attempting to delete user {0}...", auth.CurrentUser.UserId));
      DisableUI();
      auth.CurrentUser.DeleteAsync().ContinueWith(HandleDeleteResult);
    } else {
      DebugLog("Sign-in before deleting user.");
    }
  }

  void HandleDeleteResult(Task authTask) {
    EnableUI();
    LogTaskCompletion(authTask, "Delete user");
  }

  // Show the providers for the current email address.
  public void DisplayProviders() {
    auth.FetchProvidersForEmailAsync(email).ContinueWith((authTask) => {
        if (LogTaskCompletion(authTask, "Fetch Providers")) {
          DebugLog(String.Format("Email Providers for '{0}':", email));
          foreach (string provider in authTask.Result) {
            DebugLog(provider);
          }
        }
      });
  }

  // Send a password reset email to the current email address.
  public void SendPasswordResetEmail() {
    auth.SendPasswordResetEmailAsync(email).ContinueWith((authTask) => {
        if (LogTaskCompletion(authTask, "Send Password Reset Email")) {
          DebugLog("Password reset email sent to " + email);
        }
      });
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

      GUILayout.BeginHorizontal();
      GUILayout.Label("Display Name:", GUILayout.Width(Screen.width * 0.20f));
      displayName = GUILayout.TextField(displayName);
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
      if (GUILayout.Button("Reload User")) {
        ReloadUser();
      }
      if (GUILayout.Button("Get User Token")) {
        GetUserToken();
      }
      if (GUILayout.Button("Sign Out")) {
        SignOut();
      }
      if (GUILayout.Button("Delete User")) {
        DeleteUser();
      }
      if (GUILayout.Button("Show Providers")) {
        DisplayProviders();
      }
      if (GUILayout.Button("Password Reset Email")) {
        SendPasswordResetEmail();
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
