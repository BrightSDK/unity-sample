using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

public class CopyWinBrightSDKPostscript: IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        bool _isPureBuild = isPureBuild(report);
        string[] archs = getSupportedTargets(report.summary.platform, _isPureBuild);
        string _joined = string.Join(", ", archs);

        Debug.Log($"CopyWinBrightSDKPostscript: Started for [{_joined}]");
        foreach (string archString in archs)
        {
            Debug.Log($"CopyWinBrightSDKPostscript: Started copying for arch {archString}");
            string sdkDestinationDirectoryPath = findOutputSdkDll(archString, report);
            sdkDestinationDirectoryPath = Path.GetDirectoryName(sdkDestinationDirectoryPath);
            copyFilesToDestination(sdkDestinationDirectoryPath, archString);
            Debug.Log($"CopyWinBrightSDKPostscript: Finished copying for arch {archString}");
        }
        Debug.Log($"CopyWinBrightSDKPostscript: Finished for [{_joined}]");
    }

    private bool isPureBuild(BuildReport report)
    {
        foreach (BuildFile file in report.GetFiles()) {
            var isProject = file.path.EndsWith($"sln", System.StringComparison.OrdinalIgnoreCase)
            || file.path.EndsWith($"slnx", System.StringComparison.OrdinalIgnoreCase)
            || file.path.EndsWith($"csproj", System.StringComparison.OrdinalIgnoreCase)
            || file.path.EndsWith($"vcxproj", System.StringComparison.OrdinalIgnoreCase);
            if (isProject) return false;
        }
        return true;
    }

    private string[] getSupportedTargets(BuildTarget platform, bool isExeBuild)
    {
        if (isExeBuild)
        {
            switch (platform)
            {
            case BuildTarget.StandaloneWindows: return new string[] {"32"};
            case BuildTarget.StandaloneWindows64: return new string[] {"64"};
            default: return new string[] {};
            }
        }
        else
        {
            switch (platform)
            {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return new string[] {"32", "64"};
            default: return new string[] {};
            }
        }
    }

    private string findOutputSdkDll(string platform, BuildReport report)
    {
        foreach (BuildFile file in report.GetFiles()) {
            if (file.path.EndsWith($"lum_sdk{platform}.dll"))
                return file.path;
        }
        throw new InvalidOperationException("No lum_sdk in output folder");
    }

    private void copyFilesToDestination(string destinationPath, string archString)
    {
        string netUpdaterName = $"net_updater{archString}.exe";
        string netUpdaterPath = findFileInAssets(netUpdaterName);
        if (netUpdaterPath == null)
            throw new DirectoryNotFoundException($"File {netUpdaterName} not found in Assets");

        string brdConfigName = "win_brd_config.json";
        string brdConfigPath = findFileInAssets(brdConfigName);
        if (brdConfigPath == null)
        {
            brdConfigName = "brd_config.json";
            brdConfigPath = findFileInAssets(brdConfigName);
        }
        if (brdConfigPath == null)
            throw new DirectoryNotFoundException($"brd_config file not found in Assets");
        Debug.Log($"CopyWinBrightSDKPostscript: Copy {netUpdaterPath} to destination {destinationPath}");
        File.Copy(netUpdaterPath, Path.Combine(destinationPath, netUpdaterName));
        Debug.Log($"CopyWinBrightSDKPostscript: Copy {brdConfigPath} to destination {destinationPath}");
        File.Copy(brdConfigPath, Path.Combine(destinationPath, "brd_config.json"));
    }

    private string findFileInAssets(string searchFileName)
    {
        string searchName = Path.GetFileNameWithoutExtension(searchFileName);
        string[] guids = AssetDatabase.FindAssets(searchName);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) == searchFileName)
            {
                return path;
            }
        }
        return null;
    }
}
