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
  public UnityEngine.UI.InputField login;
  public UnityEngine.UI.InputField password;
  public UnityEngine.UI.Button createUserButton;
  public UnityEngine.UI.Button loginButton;
  public UnityEngine.UI.Button loginWithCredButton;
  public UnityEngine.UI.Button deleteUserButton;

  public Text outputText;
  Firebase.App app;
  Firebase.Auth auth;

  public void DebugLog(string s) {
    print(s);
    outputText.text += s + "\n";
  }

  public void ClearDebugLog() {
    outputText.text = "";
  }

  // When the app starts, create a firebase app object,
  // Initialize the Sender and Receiver objects
  void Start() {
    DebugLog("Setting up firebase...");
    Firebase.AppOptions ops = new Firebase.AppOptions();
    DebugLog(String.Format("Created the AppOptions, with appID: {0}", ops.AppID));
    DebugLog("About to Create App");
    app = Firebase.App.Create(ops);
    DebugLog(String.Format("Created the firebase app: {0}", app.Name));
    auth = Firebase.Auth.GetAuth(app);
  }

  void Update() {
    if (!Application.isMobilePlatform && Input.GetKey("escape")) {
      Application.Quit();
    }
  }

  void DisableUI() {
    login.DeactivateInputField();
    password.DeactivateInputField();
    createUserButton.interactable = false;
    loginButton.interactable = false;
    loginWithCredButton.interactable = false;
    deleteUserButton.interactable = false;
  }

  void EnableUI() {
    login.ActivateInputField();
    password.ActivateInputField();
    createUserButton.interactable = true;
    loginButton.interactable = true;
    loginWithCredButton.interactable = true;
    deleteUserButton.interactable = true;
  }

  public void CreateUser() {
    ClearDebugLog();
    DebugLog(String.Format("Attempting to create user {0}...", login.text));
    DisableUI();

    auth.CreateUserWithEmailAndPassword(login.text, password.text)
      .ContinueWith(HandleCreateResult);
  }

  void HandleCreateResult(Task<Firebase.Auth.User> authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("User Creation canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("User Creation encountered an error.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DebugLog("User Creation completed.");
      DebugLog("Signing out.");
      auth.SignOut();
    }
  }

  public void Signin() {
    ClearDebugLog();
    DebugLog(String.Format("Attempting to sign in as {0}...", login.text));
    DisableUI();
    auth.SignInWithEmailAndPassword(login.text, password.text)
      .ContinueWith(HandleSigninResult);
  }

  // This is functionally equivalent to the Signin() function.  However, it
  // illustrates the use of Credentials, which can be aquired from many
  // different sources of authentication.
  public void SigninWithCredential() {
    ClearDebugLog();
    DebugLog(String.Format("Attempting to sign in as {0}...", login.text));
    DisableUI();
    Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(login.text, password.text);
    auth.SignInWithCredential(cred).ContinueWith(HandleSigninResult);
  }

  void HandleSigninResult(Task<Firebase.Auth.User> authTask) {
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
    ClearDebugLog();
    DebugLog(String.Format("Attempting to delete user {0}...", login.text));
    DisableUI();
    auth.SignInWithEmailAndPassword(login.text, password.text)
      .ContinueWith(HandleDeleteSigninResult);
  }

  void HandleDeleteSigninResult(Task<Firebase.Auth.User> authTask) {
    EnableUI();
    if (authTask.IsCanceled) {
      DebugLog("Delete signin canceled.");
    } else if (authTask.IsFaulted) {
      DebugLog("Delete signin encountered an error while signing in.");
      DebugLog(authTask.Exception.ToString());
    } else if (authTask.IsCompleted) {
      DisableUI();
      auth.CurrentUser.Delete().ContinueWith(HandleDeleteResult);
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
}
