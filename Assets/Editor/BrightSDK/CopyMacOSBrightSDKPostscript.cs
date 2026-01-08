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

public class CopyMacOSBrightSDKPostscript: IPostprocessBuildWithReport
{
    public int callbackOrder => 0;
    private const string netUpdaterZipName = "net_updater.zip";
    private const string netUpdaterAppName = "net_updater.app";
    private const string frameworkName = "brdsdk.framework";
    private const string entitlementsName = "net_updater.entitlements";
    private const string resignScriptName = "resign_net_updater.sh";
    private const string sdkDestinationContainerName = "BrightSDK";

    public void OnPostprocessBuild(BuildReport report)
    {
        BuildTarget platform = report.summary.platform;
        if (platform != BuildTarget.StandaloneOSX) return;

        Debug.Log($"CopyMacOSBrightSDKPostscript: Started");
        string outputPath = report.summary.outputPath;
        validateOutputType(outputPath);
        string sdkSourceContainerPath = findValidSDKContainerPath();
        string sdkDestinationContainerPath = createAndGetSDKFilesPath(outputPath);
        copySDKFilesToDestination(sdkSourceContainerPath, sdkDestinationContainerPath);
        prepareXcodeProject(outputPath, sdkDestinationContainerPath);
        Debug.Log($"CopyMacOSBrightSDKPostscript: Finished");
    }

    private string findScript()
    {
        string fileName = "CopyMacOSBrightSDKPostscript.cs";
        string searchName = Path.GetFileNameWithoutExtension(fileName);
        string[] guids = AssetDatabase.FindAssets(searchName);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) == fileName)
                return path;
        }
        return null;
    }

    private void validateOutputType(string outputPath)
    {
        if (outputPath.EndsWith(".app"))
            throw new InvalidOperationException("Embedding into ready build bundle is not supported");
    }

    private string findValidSDKContainerPath()
    {
        string searchName = Path.GetFileNameWithoutExtension(netUpdaterZipName);
        string[] guids = AssetDatabase.FindAssets(searchName);
        string foundPath = null;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) == netUpdaterZipName)
            {
                foundPath = path;
                break;
            }
        }
        if (foundPath == null)
            throw new DirectoryNotFoundException("SDK container not found");
        var containerPath = Directory.GetParent(foundPath);
        if (containerPath == null)
            throw new InvalidOperationException($"Incorrect source path for SDK container - {foundPath}");
        return containerPath.FullName;
    }

    private string createAndGetSDKFilesPath(string outputPath)
    {
        string path = Path.Combine(outputPath, sdkDestinationContainerName);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);
        return path;
    }

    private void copySDKFilesToDestination(string sdkSourcePath, string sdkDestinationPath)
    {
        Debug.Log($"CopyMacOSBrightSDKPostscript: Copy files from {sdkSourcePath} to {sdkDestinationPath}");
        dittoExtractZip(Path.Combine(sdkSourcePath, netUpdaterZipName), sdkDestinationPath);
        dittoCopy(Path.Combine(sdkSourcePath, frameworkName), Path.Combine(sdkDestinationPath, frameworkName));
        dittoCopy(Path.Combine(sdkSourcePath, entitlementsName), Path.Combine(sdkDestinationPath, entitlementsName));
        dittoCopy(Path.Combine(sdkSourcePath, resignScriptName), Path.Combine(sdkDestinationPath, resignScriptName));
    }

    private void prepareXcodeProject(string outputPath, string sdkContainerPath)
    {
        Debug.Log($"CopyMacOSBrightSDKPostscript: Start preparing Xcode project");
        string xcodeProjectPath = getXcodeProjectFile(outputPath);
        Debug.Log($"CopyMacOSBrightSDKPostscript: Xcode project path {xcodeProjectPath}");
        string projectPath = Path.Combine(xcodeProjectPath, "project.pbxproj");
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        string target = project.GetUnityMainTargetGuid();

        string frameworkGuid = addFileToProject(project, outputPath, Path.Combine(sdkContainerPath, frameworkName));
        string netUpdaterGuid = addFileToProject(project, outputPath, Path.Combine(sdkContainerPath, netUpdaterAppName));
        string entitlementsGuid = addFileToProject(project, outputPath, Path.Combine(sdkContainerPath, entitlementsName));
        string resignScriptGuid = addFileToProject(project, outputPath, Path.Combine(sdkContainerPath, resignScriptName));

        project.AddFileToBuild(target, frameworkGuid);
        UnityEditor.iOS.Xcode.Extensions.PBXProjectExtensions.AddFileToEmbedFrameworks(project, target, frameworkGuid);

        addScriptPhases(project, projectPath, target, netUpdaterGuid);

        addBuildSetting(project, target, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/../Frameworks");
        addBuildSetting(project, target, "FRAMEWORK_SEARCH_PATHS", sdkDestinationContainerName);
        addBuildSetting(project, target, "NET_UPDATER_ENTITLEMENTS", $"{sdkDestinationContainerName}/{entitlementsName}");

        project.WriteToFile(projectPath);
        Debug.Log($"CopyMacOSBrightSDKPostscript: Preparing Xcode project finished");
    }

    private string getXcodeProjectFile(string searchRoot)
    {
        string[] projects = Directory.GetDirectories(searchRoot, "*.xcodeproj", SearchOption.TopDirectoryOnly);
        if (projects.Length == 0)
            throw new DirectoryNotFoundException("Any xcode project in " + searchRoot);
        return projects[0];
    }

    private string addFileToProject(PBXProject project, string projectFolderPath, string filePath)
    {
        string relativePath = Path.GetRelativePath(projectFolderPath, filePath);
        string fileName = Path.GetFileName(filePath);
        string guid = project.AddFile(relativePath, $"{sdkDestinationContainerName}/{fileName}", PBXSourceTree.Source);
        return guid;
    }

    private void addBuildSetting(PBXProject project, string target, string name, string value)
    {
        string currentValue = project.GetBuildPropertyForAnyConfig(target, name) ?? "";
        if (!currentValue.Contains(value))
            project.AddBuildProperty(target, name, value);
    }

    private void addScriptPhases(PBXProject project, string projectPath, string target, string netUpdaterGuid)
    {
        string netUpdaterCopyPhaseGuid = project.GetCopyFilesBuildPhaseByTarget(target, "Copy net_updater.app", "Contents/Library/LoginItems", "1");
        if (netUpdaterCopyPhaseGuid == null) 
        {
            netUpdaterCopyPhaseGuid = project.AddCopyFilesBuildPhase(target, "Copy net_updater.app", "Contents/Library/LoginItems", "1");
            project.AddFileToBuildSection(target, netUpdaterCopyPhaseGuid, netUpdaterGuid);
        }

        var projectText = File.ReadAllText(projectPath);
        if (!projectText.Contains("Resign net_updater.app"))
        {
            string scriptPath = $"${{PROJECT_DIR}}/{sdkDestinationContainerName}/{resignScriptName}";
            var netUpdaterResignPhaseGuid = project.AddShellScriptBuildPhase(target, "Resign net_updater.app", $"/bin/sh {scriptPath}", "");
        }
    }

    private void dittoCopy(string src, string dst)
    {
        if (Directory.Exists(dst))
            Directory.Delete(dst, true);
        runBash("/usr/bin/ditto", $"\"{src}\" \"{dst}\"");
    }

    private void dittoExtractZip(string src, string destinationRootPath)
    {
        if (!Directory.Exists(destinationRootPath))
            Directory.CreateDirectory(destinationRootPath);
        runBash("/usr/bin/ditto", $"-x -k \"{src}\" \"{destinationRootPath}\"");
    }

    private void runBash(string file, string args)
    {
        Process p = new Process();
        p.StartInfo.FileName = file;
        p.StartInfo.Arguments = args;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0)
            throw new Exception($"{file} {args}\n{p.StandardError.ReadToEnd()}");
    }
}
