# Firebase Installations Quickstart

The Firebase Installations Test Application (testapp) demonstrates fetching and
deleting Installations using the Firebase Installations API of the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).

## Requirements

* [Unity](http://unity3d.com/) 5.3 or higher.
* [Xcode](https://developer.apple.com/xcode/) 10.3 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Notes

* This testapp was designed for use on iOS and Android targets, and when
  running in the Unity editor. While the code will also execute on tvOS, there
  isn't an easy way for users to provide the click events required to use the
  UI elements on that platform.

## Building the Sample

### iOS

  - Register your iOS+ (iOS or tvOS) app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the `Add app` button,
      and selecting the **Unity** icon.
      - Check the box labeled `Register as Apple app`.
      - You should use `com.google.firebase.unity.installations.testapp` as the
        Apple bundle ID when creating the Unity app in the console.
        - If you do not use the prescribed Bundle ID, you will later need to
          update the bundle identifier in Unity as described in
          `Optional: Update the Project Bundle Identifier` below.
    - Download the `GoogleService-Info.plist` file associated with your
      Firebase project from the console. This file identifies your iOS+
      app to the Firebase backend, and will need to be included in the sample
      later.
    - For further details please refer to the
      [general instructions](https://firebase.google.com/docs/ios/setup)
      which describes how to configure a Firebase application for iOS
      and tvOS.
  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the `File > Open Project` menu item.
    - Click `Open`.
    - Navigate to the sample directory `testapp` in the file dialog and click
      `Open`.
      - You might be prompted to upgrade the project to your version of Unity.
        Click `Confirm` to upgrade the project and continue.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Installations` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Installations` plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseInstallations.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Installations` folder in the
      `Project` window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.firebase.unity.installations.testapp`
      as the Apple bundle ID when creating your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the `File > Build Settings` menu option.
      - Select `iOS` or `tvOS` in the `Platform` list, depending on your build
        target.
      - Click `Player Settings`.
      - In the `Settings for iOS` or `Settings for tvOS` panel, scroll down to
        `Bundle Identifier` and update the value to the `iOS bundle ID` you
        provided when you registered your app with Firebase.
  - Build for iOS or tvOS.
    - Select the `File > Build Settings` menu option.
    - Select either `iOS` or `tvOS` in the `Platform` list.
    - Click `Switch Platform` to enable your selection as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click `Build and Run`.
  - See the *Using the Sample* section below.


### Android

  - Register your Android app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and attach your Android app to it.
      - You should use `com.google.firebase.unity.installations.testapp` as the
        package name while you're testing.
        - If you do not use the prescribed package name you will need to update
          the bundle identifier as described in the
          `Optional: Update the Project Bundle Identifier` below.

      - To [generate a SHA1](https://developers.google.com/android/guides/client-auth),
        first you will need to set the keystore in the Unity project.
        - Locate the `Publishing Settings` under `Player Settings`.
        - Select an existing keystore, or create a new keystore using the toggle.
        - Select an existing key, or create a new key using "Create a new key".
      - After setting the keystore and key, you can generate a SHA1 by
        running this command:
        ```
        keytool -exportcert -list -v -alias <key_name> -keystore <path_to_keystore>
        ```
    - Download the `google-services.json` file associated with your
        Firebase project from the console.
        This file identifies your Android app to the Firebase backend, and will
        need to be included in the sample later.
      - For further details please refer to the
        [general instructions](https://firebase.google.com/docs/android/setup)
        which describes how to configure a Firebase application for Android.
  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the `File > Open Project` menu item.
    - Click `Open`.
    - Navigate to the sample directory `testapp` in the file dialog and click
      `Open`.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Installations` in the `Project` window.
    - Double click on `MainScene` file to open it.
  - Import the `Firebase Installations` plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseInstallations.unitypackage`.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Installations` folder in the
      `Project` window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.installations.testapp`
      as the project package name you will need to update the sample's Bundle
      Identifier.
      - Select the `File > Build Settings` menu option.
      - Select `Android` in the `Platform` list.
      - Click `Player Settings`
      - In the `Player Settings` panel scroll down to `Bundle Identifier`
        and update the value to the package name you provided when you
        registered your app with Firebase.
  - Build for Android
    - Select the `File > Build Settings` menu option.
    - Select `Android` in the `Platform` list.
    - Click `Switch Platform` to select `Android` as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click `Build and Run`.
  - See the *Using the Sample* section below.


## Using the Sample

The sample provides a simple interface with several buttons:

 - `Get ID` Fetches the current Installations or creates a new one.
 - `Delete` Deletes the current Installations.
 - `Get Token` Creates a token from the current Installations.

## Support

[https://firebase.google.com/support/](https://firebase.google.com/support/)


## License

Copyright 2020 Google LLC.

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

