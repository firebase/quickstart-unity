# Firebase Dynamic Links and Invites Quickstart

The Firebase Dynamic Links and Invites Unity Sample demonstrates sending and
receiving [Firebase Invites](https://firebase.google.com/docs/invites/) and
[Firebase Dynamic Links](https://firebase.google.com/docs/dynamic-links/)
using the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).


## Requirements

* [Unity](http://unity3d.com/) 5.3 or higher.
* [Xcode](https://developer.apple.com/xcode/) 9.4.1 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).


## Building the Sample

### iOS

  - Register your iOS app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and associate your iOS application.
      - For Invites, you will need an App Store ID. You can use something
        random such as 12345678.
      - You should use `com.google.FirebaseUnityInvitesTestApp.dev` as the
        package name while you're testing.
        - If you do not use the prescribed package name you will need to update
          the bundle identifier as described in the
          `Optional: Update the Project Bundle Identifier` below.
    - Download the `GoogleService-Info.plist` file associated with your
      Firebase project from the console.
      This file identifies your iOS app to the Firebase backend, and will
      need to be included in the sample later.
    - For further details please refer to the
      [general instructions](https://firebase.google.com/docs/ios/setup)
      which describes how to configure a Firebase application for iOS.
  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the `File > Open Project` menu item.
    - Click `Open`.
    - Navigate to the sample directory `testapp` in the file dialog and click
      `Open`.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Invites` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Invites and Dynamic Links` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseInvites.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
      downloaded previously.
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Invites` folder in the `Project`
      window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.FirebaseUnityInvitesTestApp.dev`
      as the project package name you will need to update the sample's Bundle
      Identifier.
      - Select the `File > Build Settings` menu option.
      - Select `iOS` in the `Platform` list.
      - Click `Player Settings`
      - In the `Player Settings` panel scroll down to `Bundle Identifier`
        and update the value to the package name you provided when you
        registered your app with Firebase.
  - Build for iOS
    - Select the `File > Build Settings` menu option.
    - Select `iOS` in the `Platform` list.
    - Click `Switch Platform` to select `iOS` as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click `Build and Run`.
  - Configure the iOS project capabilities to send invites and receive links.
    - Enable the Keychain Sharing capability on iOS 10 or above (required by
      Google Sign-In to send invites).
      You can enable this capability on your project in Xcode 8 by going to
      your project's settings, Capabilities, and turning on Keychain Sharing.
    - Copy the dynamic links domain for your project under the Dynamic Links
      tab of the [Firebase console](https://firebase.google.com/console/)
      Then, in your project's Capabilities tab:
      - Enable the Associated Domains capability.
      - Add applinks:YOUR_DYNAMIC_LINKS_DOMAIN
        For example "applinks:xyz.app.goo.gl".
  - See the *Using the Sample* section below.


### Android

  - Register your Android app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and attach your Android app to it.
      - You should use `com.google.FirebaseUnityInvitesTestApp.dev` as the
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
    - Navigate to `Assets/Firebase/Sample/Invites` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Invites and Dynamic Links` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseInvites.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
      downloaded previously.
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Invites` folder in the `Project`
      window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.FirebaseUnityInvitesTestApp.dev`
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

  - When you first run the app, it will check for an incoming dynamic link or
    invitation, and report whether it was able to fetch an invite.
  - To send an invite, use the `Send Invite` button on the right side of the
    screen.
  - This will open a screen that allows you to send an invite for the current
    app via email or SMS.
  - To simulate receiving an invitation from a friend, you can send yourself an
    invite, uninstall the test app, then click the link in your email.
  - This would normally send you to the Play Store or App Store to download the
    app. Because this is a test app, it will link to a nonexistent store page.
  - After clicking the invite link, re-install and run the app on your device
    or emulator, and see the invitation fetched on the receiving side.


## Support

[https://firebase.google.com/support/]()


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

