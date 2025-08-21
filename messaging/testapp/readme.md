# Firebase Messaging Quickstart

The Firebase Messaging Unity Sample demonstrates receiving messages from
[Firebase Cloud Messaging](https://firebase.google.com/docs/cloud-messaging/)
using the
[Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).


## Requirements

* [Unity](http://unity3d.com/) The quickstart project requires 2019 or higher.
* [Xcode](https://developer.apple.com/xcode/) 13.3.1 or higher
  (when developing for iOS or tvOS).
* [Android SDK](https://developer.android.com/studio/index.html#downloads)
  (when developing for Android).

## Set up, build and run the sample

1. Open the sample project in the Unity editor.
    - Select the **File > Open Project** menu item.
    - Click **Open**.
    - Navigate to the sample directory `testapp` in the file dialog and click
      **Open**.
        - You might be prompted to upgrade the project to your version of Unity.
        Click **Confirm** to upgrade the project and continue.
    - Open the scene `MainScene`.
      - Navigate to `Assets/Firebase/Sample/Messaging` in the **Project** window.
      - Double click on `MainScene` file to open it.
1. Complete [Add Firebase to your Unity project](https://firebase.google.com/docs/unity/setup) AND [Set up a Firebase Cloud Messaging client app with Unity](https://firebase.google.com/docs/cloud-messaging/unity/client) but do not implement the code sections as this codebase already has those functionalities.
    1. If at any point you run into issues with building and installing the sample, complete [Debugging the Game Build, Install and Run Process](https://firebase.google.com/docs/unity/build-debug-guide) and follow the instructions therein.

## Using the Sample

- When you run the app, it will print:
  `Received Registration Token: <registration_token>`
  this token can be used to send a notification to a single device.
  - When running the app on iOS or tvOS, the token can be accessed
    via Xcode's console output.
  - When running the app on Android, the token can be accessed using the
    ADB command line with the `adb logcat` command.


> **_tvOS NOTE:_**  This testapp was designed for use on iOS and Android targets, and when running in the Unity editor. While the code will also execute on tvOS, the buttons will be unresponsive as there isn't an easy way to provide the app with the click / touch events to orchestrate the UI elements on that platform.


### Send a message from your server environment

> **_NOTE:_** Dispatching messages from your server is a more extensible way of sending notifications or data messages to clients. The following is an easy way to demo this functionality. If you are interested in learning more about how to send messages in production from the command line or an Admin SDK read more about it starting with [Your server environment and FCM](https://firebase.google.com/docs/cloud-messaging/server).

1. Navigate to the FCM REST API Docs for [Method: projects.messages.send](https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages/send)
1. Look for the "Try this method" panel
    1. This might require you to expand your browser window.
1. Follow the API doc to understand what IDs and fields to fill in  for your particular use case.
    1. Your project number is available from [Project settings](https://console.firebase.google.com/project/_/settings/general)
1. Fill in the `Request body` referencing the [FCM REST API docs](https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages).
1. Finally, execute.

Reminder, while this process is currently done from our website, it uses the FCM v1 Send API directly and provides the simplest starting point to switch to using the Send API or Admin SDKs in production.

### Send a notification from the console

> **_NOTE:_** This is a simple but less flexible way of sending notifications to clients. It is recommended that in production you [Send a message from your server environment](#send-a-message-from-your-server-environment) instead.

- You can [send a notification to a single device](https://firebase.google.com/docs/cloud-messaging/unity/device-group)
  or group of devices with this token.
  - Using the [Firebase Console](https://firebase.google.com/console/):
    - Select **Notifications** in the left menu.
    - Change **Target** to **Single Device** and paste in the
      **Registration Token** from the device.
    - Fill out the rest of the field and press **Send Message** to send a
      notification.

- You can [send a notification to a topic](https://firebase.google.com/docs/cloud-messaging/unity/topic-messaging)
  (e.g "TestTopic") which notifies all devices subscribed to the topic.
  - Using the [Firebase Console](https://firebase.google.com/console/):
    - Select **Notifications** in the left menu.
    - Change **Target** to **Topic** and select the topic (this can take a
      few hours to appear after devices have subscribed).
    - Fill out the rest of the field and press **Send Message** to send a
      notification.


## Troubleshooting
- Please see the
  [Known Issues](https://firebase.google.com/docs/unity/setup#known-issues)
  section of the
  [Unity Setup Guide](https://firebase.google.com/docs/unity/setup) for other
  troubleshooting topics.
- When running the app, if all that you see is a blue horizon, then please
  ensure that you followed the steps to **Open the scene `MainScene`**
  above.
- Again, if at any point you run into issues with building and installing the sample, complete [Debugging the Game Build, Install and Run Process](https://firebase.google.com/docs/unity/build-debug-guide) and follow the instructions therein.


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

