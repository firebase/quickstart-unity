# Crashlytics for Firebase Quickstart

The Crashlytics for Firebase Unity Sample demonstrates the
[Firebase SDK for Crashlytics](https://firebase.google.com/docs/crashlytics/)
with the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)
inside the Unity Editor.

## Requirements

* [Unity](http://unity3d.com/) 5.3 or higher
* [Xcode](https://developer.apple.com/xcode/) 9.4.1 or higher
  (when developing for iOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Running the Sample inside the Editor

  - Download the
    [Firebase Unity SDK](https://firebase.google.com/download/unity)
    and unzip it somewhere convenient.
  - Open this sample project in the Unity editor.
    - If you are not on the Unity splash screen and have an Unity project opened, select the `File > Open Project` menu item.
    - Click `Open`.
    - Navigate to this sample project directory `testapp` in the file dialog and click `Open`.
    - If you see the "Opening Project in Non-Matching Editor Installation" dialog, you can ignore it. You are running a different version of Unity than what this test app was originally created with and that is most likely fine.
  - Open the scene `MainScene`.
    - Navigate to `Assets/Firebase/Sample/Crashlytics` in the `Project` window.
    - Double click on `MainScene` file to open.
  - Import the `Firebase Crashlytics` plugin.
    - Select the **Assets > Import Package > Custom Package** menu item.
    - From the [Firebase Unity SDK](https://firebase.google.com/download/unity)
      downloaded previously, import `FirebaseCrashlytics.unitypackage` from the
      directory that matches the version of Unity you use:
       - If your project is configured to use .NET 3.x, import the `dotnet3/FirebaseCrashlytics.unitypackage` package.
       - If your project is configured to use .NET 4.x, import the
         `dotnet4/FirebaseCrashlytics.unitypackage` package.
    - When the **Import Unity Package** window appears, click the **Import**
      button.

Once you have done this, you can run the Unity Editor and test the application.

## Building the Sample for Devices

### iOS

  - [Create a new Firebase project and Unity iOS app](https://firebase.google.com/docs/unity/setup).
    - By default, the testapp is configured with com.google.firebase.unity.crashlytics.testapp as the package name. You should use this package name when creating your project, or update the testapp bundle identifier as described in `Optional: Update the Project Bundle Identifier` below.
    - Download the `GoogleService-Info.plist` file associated with your
      Firebase project from the console.
      - For further details please refer to the
        [general instructions](https://firebase.google.com/docs/ios/setup)
        which describes how to configure a Firebase application for iOS.
  - Add the `GoogleService-Info.plist` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Crashlytics` folder in the `Project`
      window.
    - Drag the `GoogleService-Info.plist` downloaded from the Firebase console
      into the folder.
      - NOTE: `GoogleService-Info.plist` can be placed anywhere under the
        `Assets` folder.
  - Set up Crashlytics
    - In the Firebase console -> Select your project -> Select Crashlytics ->
      Setup Crashlytics -> Select app is new.
      (You do not need to download the SDK again as you have already downloaded 
      the Unity plugin)
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.crashlytics.testapp`
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

### Android

  - [Create a new Firebase project and Unity Android app](https://firebase.google.com/docs/unity/setup).
    - By default, the testapp is configured with com.google.firebase.unity.crashlytics.testapp as the package name. You should use this package name when creating your project, or update the testapp bundle identifier as described in `Optional: Update the Project Bundle Identifier` below.
    - Download the `google-services.json` file associated with your
        Firebase project from the console.
      - For further details please refer to the
        [general instructions](https://firebase.google.com/docs/android/setup)
        which describes how to configure a Firebase application for Android.
  - Add the `google-services.json` file to the project.
    - Navigate to the `Assets/Firebase/Sample/Crashlytics` folder in the `Project`
      window.
    - Drag the `google-services.json` downloaded from the Firebase console
      into the folder.
      - NOTE: `google-services.json` can be placed anywhere under the `Assets`
        folder.
  - Set up Crashlytics
    - In the Firebase console -> Select your project -> Select Crashlytics ->
      Setup Crashlytics -> Select app is new.
      (You do not need to download the SDK again as you have already downloaded 
      the Unity plugin)
  - Optional: Update the Project Bundle Identifier
    - If you did not use `com.google.firebase.unity.crashlytics.testapp`
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


## Testing and Validation

  - Once the Unity test app is running (detach debugger if attached), click the `Perform All Actions` button
  - Trigger upload by completely closing and relaunching the test app
  - To view the reported issues, go to the [Firebase Crashlytics console](https://firebase.google.com/console/) for the test app:
    - Add a time filter to more easily identify the errors if needed

## Support

[https://firebase.google.com/support/](https://firebase.google.com/support/)


## License

Copyright 2018 Google, Inc.

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

