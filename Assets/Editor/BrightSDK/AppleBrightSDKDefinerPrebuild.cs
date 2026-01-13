using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AppleBrightSDKDefiner: AssetPostprocessor
{
    const string Define = "APPLE_BRIGHT_SDK";

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        Apply();
    }

    public static void Apply()
    {
        Debug.Log($"AppleBrightSDKDefiner: Started");
        bool mobileExists = mobileFrameworkExists();
        Debug.Log($"AppleBrightSDKDefiner: Mobile framework exists {mobileExists}");
        updateForPlatform(BuildTargetGroup.iOS, mobileExists);
        updateForPlatform(BuildTargetGroup.tvOS, mobileExists);

        bool macExists = macOSFrameworkExists();
        Debug.Log($"AppleBrightSDKDefiner: MacOS framework exists {macExists}");
        updateForPlatform(BuildTargetGroup.Standalone, macExists);
        Debug.Log($"AppleBrightSDKDefiner: Finished");
    }

    private static void updateForPlatform(BuildTargetGroup group, bool pluginExists)
    {
        Debug.Log($"AppleBrightSDKDefiner: Updating for target {group}");
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        bool defineExists = defines.Contains(Define);
        Debug.Log($"AppleBrightSDKDefiner: Definition for {group} exists {defineExists}");

        if (pluginExists && !defineExists)
        {
            Debug.Log($"AppleBrightSDKDefiner: Setting definition for {group}");
            defines += ";" + Define;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
        else if (!pluginExists && defineExists)
        {
            Debug.Log($"AppleBrightSDKDefiner: Removing definition for {group}");
            defines = defines.Replace(Define, "").Replace(";;", ";").Trim(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }

    private static bool mobileFrameworkExists()
    {
        string path = "Assets/Plugins/Apple/BrightSDK/brdsdk.xcframework";
        if (Directory.Exists(path))
            return true;
        path = "Assets/Plugins/Apple/BrightSDK/brdsdk.framework";
        if (Directory.Exists(path))
            return true;
        return false;
    }

    private static bool macOSFrameworkExists()
    {
        string searchName = Path.GetFileNameWithoutExtension("net_updater.zip");
        string[] guids = AssetDatabase.FindAssets(searchName);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) == "net_updater.zip")
            {
                return true;
            }
        }
        return false;
    }
}