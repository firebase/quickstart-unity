# Firebase Remote Config Quickstart

The Firebase Remote Config Unity Sample demonstrates retrieval of various
data types from
[Firebase Remote Config](https://firebase.google.com/docs/remote-config/)
using the
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
    - Associate your project to an app by clicking the `Add app` button,
      and selecting the **Unity** icon.
      - Check the box labeled **Register as Apple app**.
      - You should use `com.google.firebase.unity.remoteconfig.testapp` as the
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
    - If Unity Hub appears, click **Add**. Otherwise click **Open**.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
      - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/RemoteConfig` in the **Project**
      window.
    - Double click on the `MainScene` file to open it.
  - Import the Firebase Remote Config plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseRemoteConfig.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/RemoteConfig` folder in the
      `Project` window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.firebase.unity.remoteconfig.testapp`
      as the **Apple bundle ID** when creating your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **iOS** or **tvOS** in the **Platform** list, depending on your
        build target.
      - Click **Player Settings**.
      - In the **Settings for iOS** or **Settings for tvOS** panel, scroll down
        to **Bundle Identifier** and update the value to the **Apple bundle
        ID** you provided when you registered your app with Firebase.
  - Build for iOS or tvOS.
    - Select the **File > Build Settings** menu option.
    - Select either **iOS** or **tvOS** in the **Platform** list.
    - Click **Switch Platform** to enable your selection as the target
      platform.
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
      - You should use `com.google.firebase.unity.remoteconfig.testapp` as the
        Android package name while you're testing.
        - If you do not use the prescribed package name, you will need to update
          the bundle identifier as described in
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
          - From the main console view, click on your Android App at the top
            and click the gear to open the settings page.
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
    - If Unity Hub appears, click **Add**.  Otherwise click **Open**.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
      - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/RemoteConfig` in the **Project**
      window.
    - Double click on the `MainScene` file to open it.
  - Import the Firebase Remote Config plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseRemoteConfig.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/RemoteConfig` folder in the
      **Project** window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.firebase.unity.remoteconfig.testapp`
      as the **Android package name** when you created your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **Android** in the **Platform** list.
      - Click **Player Settings**.
      - In the **Settings for Android** panel scroll down to
        **Bundle Identifier** and update the value to the package name you
        provided when you registered your app with Firebase.
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
  - Please see the
    [Known Issues](https://firebase.google.com/docs/unity/setup#known-issues)
    section of the
    [Unity Setup Guide](https://firebase.google.com/docs/unity/setup) for other
    troubleshooting topics.
  - When running the app, if all that you see is a blue horizon, then please
    ensure that you followed the steps to **Open the scene `MainScene`**
    above.

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

