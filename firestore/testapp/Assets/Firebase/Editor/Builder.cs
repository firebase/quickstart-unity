using UnityEditor;
using UnityEngine;

// Allows doing builds from the command line.
//
// For more information, see:
// https://docs.unity3d.com/Manual/CommandLineArguments.html
// https://docs.unity3d.com/ScriptReference/BuildOptions.html
// https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
public class Builder {

  public static void BuildIos() {
    var options = new BuildPlayerOptions();

    options.scenes = new string[] { "Assets/Firebase/Sample/Firestore/MainSceneAutomated.unity" };
    options.locationPathName = "ios-build";
    options.target = BuildTarget.iOS;
    // AcceptExternalModificationsToPlayer corresponds to "Append" in the Unity
    // UI -- it allows doing incremental iOS build.
    options.options = BuildOptions.AcceptExternalModificationsToPlayer;
    // Firebase Unity plugins don't seem to work on a simulator.
    PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

    BuildPipeline.BuildPlayer(options);
  }

}
