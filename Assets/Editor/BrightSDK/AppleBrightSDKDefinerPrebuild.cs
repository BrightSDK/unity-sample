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
        updateForPlatform(BuildTargetGroup.iOS);
        updateForPlatform(BuildTargetGroup.tvOS);
        if (macOSFrameworkExists())
        {
            updateForPlatform(BuildTargetGroup.Standalone);
        }
    }

    private static void updateForPlatform(BuildTargetGroup group)
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        bool pluginExists = mobileFrameworkExists();
        bool defineExists = defines.Contains(Define);

        if (pluginExists && !defineExists)
        {
            defines += ";" + Define;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
        else if (!pluginExists && defineExists)
        {
            defines = defines.Replace(Define, "").Replace(";;", ";").Trim(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }

    private static bool mobileFrameworkExists()
    {
        string path = "Assets/Plugins/Apple/BrightDataSDK/brdsdk.xcframework";
        if (Directory.Exists(path))
            return true;
        path = "Assets/Plugins/Apple/BrightDataSDK/brdsdk.framework";
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