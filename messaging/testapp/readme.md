# Firebase Messaging Quickstart

The Firebase Messaging Unity Sample demonstrates receiving messages from
from
[Firebase Cloud Messaging](https://firebase.google.com/docs/cloud-messaging/)
using the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).


## Requirements

* [Unity](http://unity3d.com/) 5.3 or higher.
* [Xcode](https://developer.apple.com/xcode/) 7.3 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).


## Building the Sample

### iOS

  - Register your iOS app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and associate your iOS application.
      - You should use `com.google.firebase.unity.messaging.testapp` as the
        package name while you're testing.
        - If you do not use the prescribed package name you will need to update
          the bundle identifier as described in the
          `Optional: Update the Project Bundle Identifier` below.
    - Associate the project with an
      [APNs certificate](https://firebase.google.com/docs/cloud-messaging/ios/certs)
      - Inside your project in the Firebase console, select the gear icon,
        select `Project Settings`, and then select the `Cloud Messaging` tab.
      - Select the `Upload Certificate` button for your development
        certificate, your production certificate, or both. At least one is
        required.
      - For each certificate, select the .p12 file, and provide the password,
        if any. Make sure the bundle ID for this certificate matches the
        bundle ID of your app. Select Save.
    - Download the `GoogleService-Info.plist` file associated with your
      Firebase project from the console.
      This file identifies your iOS app to the Firebase backend, and will
      need to be included in the sample later.
    - For further details please refer to the
      [general instructions](https://firebase.google.com/docs/ios/setup)
      which describes how to configure a Firebase application for iOS.
  - Download the [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open the sample project in the Unity editor.
    - Select the `File > Open Project` menu item.
    - Click `Open`.
    - Navigate to the sample directory `testapp` in the file dialog and click
      `Open`.
  - Open the scene `MainScene`.
    - Navigate to `Assets/TestApp/MainScene` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Messaging` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseMessaging.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
      downloaded previously.
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets\TestApp` folder in the `Project` window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      
      NOTE: `GoogleService-Info.plist` can be placed anywhere in the project.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.messaging.testapp`
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
    - Click `Build and run`, when Xcode opens stop the build.
      - *NOTE* If you click `Build and run` and let the sample run, it will
        not be able to receive messages.
    - Configure the Xcode project for push messaging.
      - Select the `Unity-iPhone` project from the `Navigator area`.
      - Select the `Unity-iPhone` target from the `Editor area`.
      - Select `Capabilities` from the `Editor area`.
      - Switch `Push Notifications` to `On`.
      - Scroll down to `Background Modes` and switch it to `On`.
      - Tick the `Remote notifications` box under `Background Modes`.
    - Build the Xcode project by selecting `Project->Run` from the menu.
  - See the *Using the Sample* section below.


### Android

  - Register your Android app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and attach your Android app to it.
      - You should use `com.google.firebase.unity.messaging.testapp` as the
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
    - Navigate to `Assets/TestApp/MainScene` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Messaging` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseMessaging.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
      downloaded previously.
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets\TestApp` folder in the `Project` window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      
      NOTE: `google-services.json` can be placed anywhere in the project.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.messaging.testapp`
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

Before running the sample on iOS you must create a
[APNs certificate](https://firebase.google.com/docs/cloud-messaging/ios/certs)
if you haven't already, then associate it with your sample project in the
[Firebase console](https://firebase.google.com/console/):

  - Inside your project in the Firebase console, select the gear icon,
    select `Project Settings`, and then select the `Cloud Messaging` tab.
  - Select the `Upload Certificate` button for your development certificate,
    your production certificate, or both. At least one is required.
  - For each certificate, select the .p12 file, and provide the password,
    if any. Make sure the bundle ID for this certificate matches the
    bundle ID of your app. Select Save.

Failure to associate the sample with an APNs certificate will result in the
iOS application being unable to receive messages.

  - When you run the app, it will print: `Received Registration Token: <code>`.
    - When running the app on iOS, the token can be accessed via Xcode's
      console output.
    - When running the app on Android, the token can be accessed using the
      ADB command line with the `adb logcat` command.

  - You can [send a notification](https://firebase.google.com/docs/cloud-messaging/android/first-message)
    with the [Firebase Console](https://firebase.google.com/console/) with this
    token.
- Received notifications are logged by the app.

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

