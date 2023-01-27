# Firebase Installations Quickstart

The Firebase Installations Test Application (testapp) demonstrates fetching and
deleting Installations using the Firebase Installations API of the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).

## Requirements

* [Unity](http://unity3d.com/) The quickstart project requires 2019 or higher.
* [Xcode](https://developer.apple.com/xcode/) 13.3.1 or higher
  (when developing for iOS or tvOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Building the Sample

### iOS or tvOS

  - Register your iOS+ (iOS or tvOS) app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - Check the box labeled **Register as Apple app**.
      - You should use `com.google.firebase.unity.installations.testapp` as the
        **Apple bundle ID** when creating the Unity app in the console.
        - If you do not use the prescribed Bundle ID, you will later need to
          update the bundle identifier in Unity as described in
          **Optional: Update the Project Bundle Identifier** below.
    - Download the `GoogleService-Info.plist` file associated with your
      Firebase project from the console. This file identifies your iOS+
      app to the Firebase backend, and will need to be included in the sample
      later.
    - For further details please refer to the
      [general instructions](https://firebase.google.com/docs/ios/setup)
      page which describes how to configure a Firebase application for iOS
      and tvOS.
  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the **File > Open Project** menu item.
    - Click **Open**.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
      - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Installations` in the **Project**
      window.
    - Double click on `MainScene` file to open.
  - Import the Firebase Installations plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseInstallations.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Installations` folder in the
      **Project** window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.firebase.unity.installations.testapp`
      as the **Apple bundle ID** when creating your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **iOS** or **tvOS** in the **Platform** list, depending on your
        build target.
      - Click **Player Settings**.
      - In the **Settings for iOS** or **Settings for tvOS** panel, scroll
        down to **Bundle Identifier** and update the value to the 
        **Apple bundle ID** you provided when you registered your app with
        Firebase.
  - Build for iOS or tvOS.
    - Select the **File > Build Settings** menu option.
    - Select either **iOS** or **tvOS** in the **Platform** list.
    - Click **Switch Platform** to enable your selection as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click **Build and Run**.
  - See the **Using the Sample** section below.


### Android

  - Register your Android app with Firebase.
    - Create a Unity project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - You should use `com.google.firebase.unity.installations.testapp` as the
        package name while you're testing.
        - If you do not use the prescribed package name you will need to update
          the bundle identifier as described in the
          **Optional: Update the Project Bundle Identifier** below.
      - Android apps must be signed by a key, and the key's signature must
        be registered to your project in the Firebase Console. To
        [generate a SHA1](https://developers.google.com/android/guides/client-auth),
        first you will need to set the keystore in the Unity project.
        - Locate the **Publishing Settings** under **Player Settings** in the
          Unity editor.
        - Select an existing keystore, or create a new keystore using the
          toggle.    
        - Select an existing key, or create a new key using 
          **Create a new key**.
      - After setting the keystore and key, you can generate a SHA1 by
          running this command:
          ```
          keytool -list -v -keystore <path_to_keystore> -alias <key_name>
          ```
        - Copy the SHA1 digest string into your clipboard.
        - Navigate to your Android App in your firebase console.
        - From the main console view, click on your Android App at the top, then
          click the gear to open the settings page.
        - Scroll down to your apps at the bottom of the page and click on
          **Add Fingerprint**.
        - Paste the SHA1 digest of your key into the form. The SHA1 box
          will illuminate if the string is valid. If it's not valid, check
          that you have copied the entire SHA1 digest string.
    - Download the `google-services.json` file associated with your
        Firebase project from the console.
        This file identifies your Android app to the Firebase backend, and will
        need to be included in the sample later.
      - For further details please refer to the
        [general instructions](https://firebase.google.com/docs/android/setup)
        page which describes how to configure a Firebase application for
        Android.
  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the **File > Open Project** menu item.
    - Click **Open**.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Installations` in the **Project**
      window.
    - Double click on `MainScene` file to open it.
  - Import the `Firebase Installations` plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseInstallations.unitypackage`.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Installations` folder in the
      **Project** window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.firebase.unity.installations.testapp`
      as the project package name you will need to update the sample's Bundle
      Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **Android** in the **Platform** list.
      - Click **Player Settings**.
      - In the **Player Settings** panel scroll down to **Bundle Identifier**
        and update the value to the package name you provided when you
        registered your app with Firebase.
  - Build for Android.
    - Select the **File > Build Settings** menu option.
    - Select **Android** in the **Platform** list.
    - Click **Switch Platform** to select **Android** as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click **Build and Run**.
  - See the **Using the Sample** section below.

## Notes

### Usage on tvOS

This testapp was designed for use on iOS and Android targets, and when
running in the Unity editor. While the code will also execute on tvOS,
the buttons will be unresponsive as there isn't an easy way to provide
the app with the click / touch events to orchestrate the UI elements on
that platform.

## Using the Sample

**Note:** the UI elements of the quickstart app respond only to mouse clicks,
and so will not be responsive on tvOS.

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

