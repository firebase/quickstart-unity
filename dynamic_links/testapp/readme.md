# Firebase Dynamic Links Quickstart

The Firebase Dynamic Links Unity Sample demonstrates sending and
receiving
[Firebase Dynamic Links](https://firebase.google.com/docs/dynamic-links/)
using the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).


## Requirements

* [Unity](http://unity3d.com/) The quickstart project requires 2019 or higher.
* [Xcode](https://developer.apple.com/xcode/) 13.3.1 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Notes

* Dynamic Links is not supported on tvOS.

## Building the Sample

### iOS

  - Register your iOS app with Firebase.
    - Create a project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - Check the box labeled **Register as Apple app**.
      - You should use `com.google.FirebaseUnityDynamicLinksTestApp.dev` as the
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
      page which describes how to configure a Firebase application for iOS.
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
    - Navigate to `Assets/Firebase/Sample/Functions` in the **Project** window.
    - Double click on `MainScene` file to open it.
      - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
  - Import the Firebase Dynamic Links plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseDynamicLinks.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Functions` folder in the
      **Project** window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.FirebaseUnityDynamicLinksTestApp.dev`
      as the **Apple bundle ID** when creating your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **iOS** in the **Platform** list
      - Click **Player Settings**.
      - In the **Settings for iOS** panel, scroll down to
        **Bundle Identifier** and update the value to the **Apple bundle ID** you
        provided when you registered your app with Firebase.
  - Copy the dynamic links domain URI prefix for your project under the Dynamic
    Links tab of the [Firebase console](https://firebase.google.com/console/)
    e.g. `xyz.app.goo.gl` and assign it to the string **DomainUriPrefix** on
      the `UIHandler` object in the `MainScene`.
    - Optional: If you want to use a custom Dynamic Links domain, follow
      [these instructions](https://firebase.google.com/docs/dynamic-links/custom-domains)
      to set up the domain in Firebase console and in your project's
      `Info.plist`.
  - Build for iOS.
    - Select the **File > Build Settings** menu option.
    - Select **iOS** in the **Platform** list.
    - Click **Switch Platform** to enable your selection as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click **Build and Run**.
  - Configure the Xcode project capabilities to send invites and receive links.
    - Enable the Keychain Sharing capability (required by Google Sign-In to
      send invites).
      - You can enable this capability on your project in Xcode by going to
        your project's settings, **Capabilities**, and turning on
        **Keychain Sharing**.
    - Copy the domain URI prefix for your project under the **Dynamic Links** tab of
      the [Firebase console](https://firebase.google.com/console/) Then, in your
      project's Capabilities tab:
      - Enable the Associated Domains capability.
      - Add `applinks:YOUR_DYNAMIC_LINKS_DOMAIN`. For example:
        `applinks:xyz.app.goo.gl`.
  - See the **Using the Sample** section below.


### Android

  - Register your Android app with Firebase.
    - Create a Unity project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - You should use `com.google.FirebaseUnityDynamicLinksTestApp.dev` as the
        **Android package name** while you're testing.
        - If you do not use the prescribed package name, you will need to update
          the bundle identifier as described in **Optional: Update the Project
          Bundle Identifier** below.
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
      - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Functions` in the **Project** window.
    - Double click on `MainScene` file to open it.
  - Import the Firebase Dynamic Links plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseDynamicLinks.unitypackage`.
    - When the **Import Unity Package** window appears, click the **Import**
      button.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Functions` folder in the
      **Project** window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.FirebaseUnityDynamicLinksTestApp.dev`
      as the **Android package name** when you created your app in the Firebase
      Console, you will need to update the sample's Bundle Identifier.
      - Select the **File > Build Settings** menu option.
      - Select **Android** in the **Platform** list.
      - Click **Player Settings**.
      - In the **Settings for Android** panel scroll down to
        **Bundle Identifier** and update the value to the
        **Android package name** you provided when you registered your app with
        Firebase.
  - Copy the dynamic links domain URI prefix for your project under the
    **Dynamic Links** tab of the
    [Firebase console](https://firebase.google.com/console/)
    e.g. `xyz.app.goo.gl` and assign it to the string **DomainUriPrefix** on the
    `UIHandler` object in the `MainScene`.
    - Optional: If you want to use a custom Dynamic Links domain, follow
      [these instructions](https://firebase.google.com/docs/dynamic-links/custom-domains)
      to set up the domain in Firebase console.
  - Build for Android.
    - Select the **File > Build Settings** menu option.
    - Select **Android** in the **Platform** list.
    - Click **Switch Platform** to select **Android** as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner
      of the Unity status bar.
    - Click **Build and Run**.
  - See the **Using the Sample** section below.


## Using the Sample

**Note:** the UI elements of the quickstart app respond only to mouse clicks,
and so will not be responsive on tvOS.

  - Receiving a link
    - When you first run the app, it will check for an incoming dynamic link
      and report whether it was able to fetch a link.
    - To simulate receiving a dynamic link, you can send yourself an email,
      uninstall the test app, then click the link in your email.
    - This would normally send you to the Play Store or App Store to download
      the app. Because this is a test app, it will link to a nonexistent store
      page.
    - After clicking the dynamic link, re-install and run the app on your
      device or emulator, and see the dynamic link fetched on the receiving
      side.
  - Creating a link
    - Clicking the Display Long Link button creates and displays a long
      dynamic link.
    - Clicking the Create Short Link button creates and displays a short link by
      contacting the link shortening service.
    - Clicking the Create Unguessable Short Link button creates and displays an
      unguessable short link.


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

