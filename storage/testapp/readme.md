# Cloud Storage for Firebase Quickstart

The Cloud Storage for Firebase Unity Sample demonstrates the
[Firebase SDK for Cloud Storage](https://firebase.google.com/docs/storage/)
with the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)
inside the Unity Editor.

## Requirements

* [Unity](http://unity3d.com/) 5.3 or higher.
* [Xcode](https://developer.apple.com/xcode/) 10.3 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Running the Sample inside the Editor

  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the `File > Open Project` menu item.
    - If Unity Hub appears, click `Add`. Otherwise click `Open`.
    - Navigate to the sample directory `testapp` in the file dialog and click
      `Open`.
      - You might be prompted to upgrade the project to your version of Unity.
        Click `Confirm` to upgrade the project and continue.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Storage` in the `Project` window.
    - Double click on the `MainScene` file to open it.
  - Import the `Firebase Storage` plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseStorage.unitypackage` from the
      directory that matches the version of Unity you use:
       - Unity 5.x and earlier use the .NET 3.x framework, so you need to
         import the `dotnet3/FirebaseStorage.unitypackage` package .
       - Unity 2017.x and newer allow the use of the .NET 4.x framework.  If
         your project is configured to use .NET 4.x, import the
         `dotnet4/FirebaseStorage.unitypackage` package.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Turn off secure access using [Public Rules](https://firebase.google.com/docs/storage/security/start)
    - Navigate to the Storage tab in the firebase console.
    - In the Rules section, replace the line
    `allow read, write: if request.auth != null;` with `allow read, write;`
  - Change the line `private const string MyStorageBucket = "gs://YOUR-FIREBASE-BUCKET/";`
    to replace YOUR-FIREBASE-BUCKET with the bucket found in the
    firebase console under the Storage tab.

Once you have done this, you can run the Unity Editor and test the application.
You will be able to enter text to upload to Cloud Storage and afterwards
download that same text.

## Building the Sample for Devices

### iOS

  - Register your iOS app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and associate your iOS application.
      - You should use `com.google.firebase.unity.storage.testapp` as the
        iOS bundle ID when creating the Firebase iOS app in the console.
        - If you do not use the prescribed Bundle ID, you will later need to
          update the bundle identifier in Unity as described in
          `Optional: Update the Project Bundle Identifier` below.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Storage` folder in the `Project`
      window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.storage.testapp`
      as the iOS bundle ID when creating your app in the Firebase
      Console then you will need to update the sample's Bundle
      Identifier.
      - Select the `File > Build Settings` menu option.
      - Select `iOS` in the `Platform` list.
      - Click `Player Settings`
      - In the `Settings for iOS` panel scroll down to `Bundle Identifier`
        and update the value to the `iOS bundle ID` you provided when you
        registered your app with Firebase.
  - Build for iOS
    - Select the `File > Build Settings` menu option.
    - Select `iOS` in the `Platform` list.
    - Click `Switch Platform` to select `iOS` as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click `Build and Run`.

### Android

  - Register your Android app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and attach your Android app to it.
      - You should use `com.google.firebase.unity.storage.testapp` as the
        Android package name while you're testing.
        - If you do not use the prescribed package name, you will need to update
          the bundle identifier as described in the
          `Optional: Update the Project Bundle Identifier` below.
      - Android apps must be signed by a key, and the key's signature must
        be registered to your project in the Firebase Console. To
        [generate a SHA1](https://developers.google.com/android/guides/client-auth),
        first you will need to set the keystore in the Unity project.
        - Locate the `Publishing Settings` under `Player Settings`.
        - Select an existing keystore, or create a new keystore using the
          toggle.
        - Select an existing key, or create a new key using "Create a new key".
        - After setting the keystore and key, you can generate a SHA1 by
          running this command:
          ```
          keytool -list -v -keystore <path_to_keystore> -alias <key_name>
          ```
        - Copy the SHA1 digest string into your clipboard.
        - Navigate to your Android App in your firebase console.
          - From the main console view, click on your Android App at the top and
            click the gear to open the settings page.
        - Scroll down to your apps at the bottom of the page and click on
          `Add Fingerprint`.
        - Paste the SHA1 digest of your key into the form.  The SHA1 box
          will illuminate if the string is valid.  If it's not valid, check
          that you have copied the entire SHA1 digest string.
    - Download the `google-services.json` file associated with your
        Firebase project from the console.
        This file identifies your Android app to the Firebase backend, and will
        need to be included in the sample later.
      - For further details please refer to the
        [general instructions](https://firebase.google.com/docs/android/setup)
        which describes how to configure a Firebase application for Android.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Storage` folder in the `Project`
      window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.storage.testapp`
            as the `Android package name` when you created your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the `File > Build Settings` menu option.
      - Select `Android` in the `Platform` list.
      - Click `Player Settings`
      - In the `Settings for Android` panel scroll down to `Bundle Identifier`
        and update the value to the package name you provided when you
        registered your app with Firebase.
  - Build for Android
    - Select the `File > Build Settings` menu option.
    - Select `Android` in the `Platform` list.
    - Click `Switch Platform` to select `Android` as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click `Build and Run`.


## Troubleshooting

  - When upgrading to a new Firebase release: import the new firebase
    unity package through **Assets > Import Package > Custom Package** as above.
    After the import is complete you may need to run the **Assets > Play
    Services Resolver** for the changes to be reflected in the editor. If
    issues persist, delete the plugin and install it again.
  - **Android:** After exiting the editor and returning you will need to
    reconfigure the **Project Keystore** in **Player Settings > Publishing
    Settings**.  Select your **Custom Keystore** from the dropdown list and
    enter its password.  Then, select your **Project Key** alias and enter
    your key's password.
    enabled in your project, you'll see compile errors from some types in the
  - Please see the
    [Known Issues](https://firebase.google.com/docs/unity/setup#known-issues)
    section of the
    Unity Setup Guide](https://firebase.google.com/docs/unity/setup) for other
    troubleshooting topics.


## Support

[https://firebase.google.com/support/](https://firebase.google.com/support/)


## License

Copyright 2016 Google, Inc.

Licensed to the Apache Software Foundation (ASF) under one or more contributor
license agreements.  See the NOTICE file distributed with this work for
additional information regarding copyright ownership.  The ASF licenses this
file to you under the Apache License, Version 2.0 (the "License"); you may not
use this file except in compliance with the License.  You may obtain a copy of
the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
License for the specific language governing permissions and limitations under
the License.

