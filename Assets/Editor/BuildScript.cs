using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    public static void ConfigureAndroidSettings()
    {
        string editorPath = "/home/anni/Unity/Hub/Editor/2022.3.62f3/Editor/Data/PlaybackEngines/AndroidPlayer";
        string sdkPath = Path.Combine(editorPath, "SDK");
        string ndkPath = Path.Combine(editorPath, "NDK");
        string jdkPath = Path.Combine(editorPath, "OpenJDK");

        if (Directory.Exists(sdkPath)) EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        if (Directory.Exists(ndkPath)) EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        if (Directory.Exists(jdkPath)) EditorPrefs.SetString("JdkPath", jdkPath);

        PlayerSettings.productName = "Furnish AR";
        PlayerSettings.companyName = "FurnishAR";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.furnishar.app");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;


        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;
    }

    public static void BuildAndroid()
    {
        ConfigureAndroidSettings();

        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        string outputDir = "Builds/Android";
        string outputPath = Path.Combine(outputDir, "FurnishAR.apk");

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("Starting Android APK Build (API 34 - Android 14) to: " + outputPath);
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Android Build SUCCEEDED: " + summary.totalSize + " bytes at " + outputPath);
        }
        else
        {
            Debug.LogError("Android Build FAILED with result: " + summary.result + ", errors: " + summary.totalErrors);
            EditorApplication.Exit(1);
        }
    }
}
