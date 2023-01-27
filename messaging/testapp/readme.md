# Firebase Messaging Quickstart

The Firebase Messaging Unity Sample demonstrates receiving messages from
from
[Firebase Cloud Messaging](https://firebase.google.com/docs/cloud-messaging/)
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
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - Check the box labeled **Register as Apple app**.
      - You should use `com.google.FirebaseUnityMessagingTestApp.dev` as the
        **Apple bundle ID** when creating the Unity app in the console.
        - If you do not use the prescribed Bundle ID, you will later need to
          update the bundle identifier in Unity as described in
          **Optional: Update the Project Bundle Identifier** below.
    - Associate the project with an
      [APNs certificate](https://firebase.google.com/docs/cloud-messaging/ios/certs)
      - Inside your project in the Firebase console, select the gear icon,
        select **Project Settings**, and then select the **Cloud Messaging**
        tab.
      - Select the **Upload Certificate** button for your development
        certificate, your production certificate, or both. At least one is
        required.
      - For each certificate, select the `.p12` file, and provide the password,
        if any. Make sure the bundle ID for this certificate matches the
        bundle ID of your app. Select **Save**.
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
    - Navigate to `Assets/Firebase/Sample/Messaging` in the **Project** window.
    - Double click on `MainScene` file to open it.
  - Import the Firebase Cloud Messaging plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseMessaging.unitypackage`.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Messaging` folder in the
      **Project** window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.FirebaseUnityMessagingTestApp.dev`
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
    - Click **Build and run**, when Xcode opens stop the build.
      - *NOTE* If you click **Build and run** and let the sample run, it will
        not be able to receive messages.
    - Configure the Xcode project for push messaging.
      - Select the **Unity-iPhone** project from the **Navigator area**.
      - Select the **Unity-iPhone** target from the **Editor area**.
      - Select the **General** tab from the **Editor area**.
      - Scroll down to **Linked Frameworks and Libraries** and click the **+**
        button to add a framework.
        - In the window that appears, scroll to **UserNotifications.framework**
          and click on that entry, then click on **Add**.
      - Select the **Capabilities** tab from the **Editor area**.
      - Switch **Push Notifications** to **On**.
      - Scroll down to **Background Modes** and switch it to **On**.
      - Tick the **Remote notifications** box under **Background Modes**.
    - Build the Xcode project by selecting **Project > Run** from the menu.
  - See the **Using the Sample** section below.


### Android

  - Register your Android app with Firebase.
    - Create a Unity project in the
      [Firebase console](https://firebase.google.com/console/).
    - Associate your project to an app by clicking the **Add app** button,
      and selecting the **Unity** icon.
      - You should use `com.google.FirebaseUnityMessagingTestApp.dev` as the
        Android package name while you're testing.
        - If you do not use the prescribed package name, you will need to update
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
          - From the main console view, click on your Android App at the top and
            click the gear to open the settings page.
        - Scroll down to your apps at the bottom of the page and click on
          **Add Fingerprint**.
        - Paste the SHA1 digest of your key into the form. The SHA1 box
          will illuminate if the string is valid.  If it's not valid, check
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
    - Navigate to `Assets/Firebase/Sample/Messaging` in the **Project** window.
    - Double click on the `MainScene` file to open it.
  - Import the Firebase Cloud Messaging plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseMessaging.unitypackage`.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Messaging` folder in the
      **Project** window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Optional: Update the Project Bundle Identifier.
    - If you did not use `com.google.FirebaseUnityMessagingTestApp.dev`
      as the Android package name when you created your app in the Firebase
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
  - Please ensure that you are requesting for `BIND_JOB_SERVICE` permission on
    messaging services in `AndroidManifest.xml`. Example,
  ```
    <service android:name="com.google.firebase.messaging.MessageForwardingService"
             android:permission="android.permission.BIND_JOB_SERVICE"
             android:exported="false" >
    </service>
  ```
  - See the **Using the Sample** section below.

## Notes

### Usage on tvOS

This testapp was designed for use on iOS and Android targets, and when
running in the Unity editor. While the code will also execute on tvOS,
the buttons will be unresponsive as there isn't an easy way to provide
the app with the click / touch events to orchestrate the UI elements on
that platform.

## Using the Sample

Before running the sample on iOS you must create a
[APNs certificate](https://firebase.google.com/docs/cloud-messaging/ios/certs)
if you haven't already, then associate it with your sample project in the
[Firebase console](https://firebase.google.com/console/):

  - Inside your project in the Firebase console, select the gear icon,
    select  **Project Settings**, and then select the **Cloud Messaging** tab.
  - Select the **Upload Certificate** button for your development certificate,
    your production certificate, or both. At least one is required.
  - For each certificate, select the `.p12` file, and provide the password,
    if any. Make sure the bundle ID for this certificate matches the
    bundle ID of your app. Select **Save**.

Failure to associate the sample with an APNs certificate will result in the
iOS or tvOS application being unable to receive messages.

  - When you run the app, it will print:
    `Received Registration Token: <registration_token>`
    this token can be used to send a notification to a single device.
    - When running the app on iOS or tvOS, the token can be accessed
      via Xcode's console output.
    - When running the app on Android, the token can be accessed using the
      ADB command line with the `adb logcat` command.

  - To send messages from your own server or the command line you will need the
     `Server Key`.
    - Open your project in the
      [Firebase Console](https://firebase.google.com/console/)
    - Click the gear icon then **Project settings** in the menu on the left
    - Select the **Cloud Messaging** tab.
    - Copy the **Server Key**.

  - You can [send a notification to a single device](https://firebase.google.com/docs/cloud-messaging/unity/device-group)
    or group of devices with this token.
    - Using the [Firebase Console](https://firebase.google.com/console/):
      - Open the [Firebase Console](https://firebase.google.com/console/).
      - Select **Notifications** in the left menu.
      - Change **Target** to **Single Device** and paste in the
        **Registration Token** from the device.
      - Fill out the rest of the field and press **Send Message** to send a
        notification.
    - Using the command line:
      - Replace `<Server Key>` and `<Registration Token>` in this command and
        run it from the command line:
```
curl --header "Authorization: key=<Server Key>" --header "Content-Type: application/json" https://android.googleapis.com/gcm/send -d '{"notification":{"title":"Hi","body":"Hello from the Cloud"},"data":{"score":"lots"},"to":"<Registration Token>"}'
```

  - You can [send a notification to a topic](https://firebase.google.com/docs/cloud-messaging/unity/topic-messaging)
    (e.g "TestTopic") which notifies all devices subscribed to the topic.
    - Using the [Firebase Console](https://firebase.google.com/console/):
      - Open the [Firebase Console](https://firebase.google.com/console/).
      - Select **Notifications** in the left menu.
      - Change **Target** to **Topic** and select the topic (this can take a
        few hours to appear after devices have subscribed).
      - Fill out the rest of the field and press **Send Message** to send a
        notification.
    - Using the command line:
      - Replace `<Server Key>` and `<Topic>` in this command and
        run it from the command line:
```
curl --header "Authorization: key=<Server Key>" --header "Content-Type: application/json" https://android.googleapis.com/gcm/send -d '{"notification":{"title":"Hi","body":"Hello from the Cloud"},"data":{"score":"lots"},"to":"/topics/<Topic>"}'
```


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

