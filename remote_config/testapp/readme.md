# Firebase Remote Config Quickstart

The Firebase Remote Config Unity Sample demonstrates retrieval of various
data types from
[Firebase Remote Config](https://firebase.google.com/docs/remote-config/)
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
      - You should use `com.google.firebase.unity.remoteconfig.testapp` as the
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
    - Navigate to `Assets/TestApp/MainScene` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Remote Config` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseRemoteConfig.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
      downloaded previously.
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets\TestApp` folder in the `Project` window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      
      NOTE: `GoogleService-Info.plist` can be placed anywhere in the project.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.remoteconfig.testapp`
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
  - See the *Using the Sample* section below.


### Android

  - Register your Android app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/),
      and attach your Android app to it.
      - You should use `com.google.firebase.unity.remoteconfig.testapp` as the
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
  - Import the `Firebase Remote Config` plugin.
    - Select the `Assets > Import Package > Custom Package` menu item.
    - Import `FirebaseRemoteConfig.unitypackage` from the
      [Firebase Unity SDK](https://firebase.google.com/download/unity),
    - Click the `Import` when the `Import Unity Package` window appears.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets\TestApp` folder in the `Project` window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      
      NOTE: `google-services.json` can be placed anywhere in the project.
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.remoteconfig.testapp`
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

Before running, you should add some data to the Firebase Console for the
sample to fetch.

  - Navigate to the [Firebase Console](https://console.firebase.google.com)
  - Select your project.
  - Click on `Remote Config` in the sidebar.
  - Add the following parameters for the sample to fetch:
    - A string named `config_test_string`
    - An integer named `config_test_int`
    - A floating point value named `config_test_float`
    - A boolean named `config_test_bool`
  - Click `Publish` in the upper right corner.

The sample provides a simple interface with two buttons:
  - The `Fetch Remote Data` button fetches remote configuration data from the
    server.
  - The `Display Current Data` button displays the data fetched from the last
    press of the `Fetch Remote Data` button.  If data hasn't been fetch from
    the server or the server isn't accessible (e.g the device is offline)
    the default values set in `UIHandler.cs` will be displayed.
  - The `Display All Keys` button displays all of the keys associated with
    config data from the last fetch. It then displays all keys that begin with
    "config_test_s".

Using Firebase Remote Config you can update and publish new data through the
Firebase Console and it will be reflected in your app.


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

