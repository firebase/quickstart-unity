# Firebase Remote Config Quickstart

The Firebase Remote Config Test Application (testapp) demonstrates fetching of
various types of data through the Firebase Remote Config API.  The application
has a basic set of buttons that trigger retrieval and display of data through
Firebase Remote Config.


## Requrements

The testapp requires version 5.3 of Unity or higher.


## Introduction

[Read more about Firebase Remote Config](https://firebase.google.com/docs/remote-config/)


## Building the testapp

### Android

  - Register your Android app with Firebase.
    - Create a new app on the [Firebase
      console](https://firebase.google.com/console/), and attach your Android
      app to it.
      - You can use "com.google.firebase.unity.remoteconfig.testapp" as the Package Name
        while you're testing.
      - To [generate a SHA1](https://developers.google.com/android/guides/client-auth)
        run this command on Mac and Linux,
        ```
        keytool -exportcert -list -v -alias androiddebugkey -keystore ~/.android/debug.keystore
        ```
        or this command on Windows,
        ```
        keytool -exportcert -list -v -alias androiddebugkey -keystore %USERPROFILE%\.android\debug.keystore
        ```
      - If keytool reports that you do not have a debug.keystore, you can
        [create one with](http://developer.android.com/tools/publishing/app-signing.html#signing-manually),
        ```
        keytool -genkey -v -keystore ~/.android/debug.keystore -storepass android -alias androiddebugkey -keypass android -dname "CN=Android Debug,O=Android,C=US"
        ```
    - Download a `google-services.json` file from the Firebase console.
      This file identifies your Android app to the Firebase backend, and will
      need to be included in the testapp later.
    - For further details please refer to the [general
      instructions for setting up an Android app with
      Firebase](https://firebase.google.com/docs/android/setup).
  - Download the [Firebase Unity SDK](https://dev-partners.googlesource.com/unity-firebase/+archive/zip.tar.gz)
    and unzip it somewhere convenient.
  - Open the testapp project in the Unity editor.
  - Open the scene MainScene
  - Import the custom package FirebaseApp.unitypackage from the Firebase
    Unity SDK, downloaded previously.  (From the menu, select
    Assets > Import Package > Custom Package)
  - Import the custom package FirebaseRemoteConfig.unitypackage in the same way.
  - From the menu, select Assets > Google Play Services > Resolve Client Jars.
    Note that this option will only appear on the menu if you have both
    imported the firebase packages, and set the build target to Android.
  - Take the `google-services.json` that was downloaded earlier, and copy it
    into your the testapp's `Assets` directory.
    (Note: This has to be done after the packages are loaded, so that the plugin
    notices the file and generates the appropriate resource files.)
  - Build the Unity project and run it on your Android device.


### iOS

  - Register your iOS app with Firebase.
    - Create a new app on the [Firebase
      console](https://firebase.google.com/console/), and attach your Android
      app to it.
      - You can use "com.google.firebase.unity.remoteconfig.testapp" as the Package Name
        while you're testing.
    - Download a `GoogleService-Info.plist` file from the Firebase console.
      This file identifies your iOS app to the Firebase backend, and will
      need to be included in the testapp later.
    - For further details please refer to the [general instructions for setting
      up an iOS app with Firebase](https://firebase.google.com/docs/ios/setup).
  - Download the [Firebase Unity SDK](https://dev-partners.googlesource.com/unity-firebase/+archive/zip.tar.gz)
    and unzip it somewhere convenient.
  - Open the testapp project in the Unity editor.
  - Open the scene MainScene
  - Import the custom package FirebaseApp.unitypackage from the Firebase
    Unity SDK, downloaded previously.  (From the menu, select
    Assets > Import Package > Custom Package)
  - Import the custom package FirebaseRemoteConfig.unitypackage in the same way.
  - Build your project for iOS.
  - Navigate to the xcode project that was created by Unity, and create a new
    file called Podfile, with the following contents:
    ```
    source 'https://github.com/CocoaPods/Specs.git'
    platform :ios, '8.0'

    target 'Unity-iPhone' do
      pod 'Firebase/RemoteConfig'
    end
    ```
  - From the terminal, navigate to the project directory, and run `pod install`.
  - From the terminal, run `open Unity-iPhone.xcworkspace` to open xcode.
    (Important: Launch it from `Unity-iPhone.xcworkspace` and not
    `Unity-iPhone.xcproject`.  Launching from `Unity-iPhone.xcproject` will
    cause problems.)
  - Under 'Build Settings' of the project, add `$(inherited)` to "Other Linker
    Flags".
  - Drag the `GoogleServices-Info.plist` file from finder into the root of the
    project in xcode.
  - Build the Unity project and run it on your Android device.


## Using the testapp

Before running, you should add some data for it to fetch.  Navigate to the
[Firebase Console](https://console.firebase.google.com), select your app, and
click on "Remote Config" on the sidebar.  Add some new parameters.  (By default
the testapp will try to fetch a string named "config_test_string", an integer
named "config_test_int", a floating point value named "config_test_float", and
a boolean named "config_test_bool")  Add those fields, and give them whatever
values you want.  Click "Publish" in the upper right corner.

When you run the testapp, it will provide a simple interface with two buttons -
one button that displays whatever data is currently downloaded (or the defaults,
if you haven't ever fetched anything yet) and one that fetches data from the
server.

Pressing the "display data" button should show the defaults.  After pressing
"fetch data", the "display data" button will instead display the newly
downloaded data.  You can update and publish new data through the firebase
console and it will be reflected in your app.


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

