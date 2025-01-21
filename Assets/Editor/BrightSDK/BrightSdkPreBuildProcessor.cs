using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Unity.SharpZipLib.Tar;
using Unity.SharpZipLib.GZip;
using System.Text;

public class BrightSdkPreBuildProcessor : IPreprocessBuildWithReport
{
    private string sdkVersion = "latest";
    private readonly string sdkDir = "Assets/Plugins/Android";
    private readonly string cacheDir = "Library/BrightSdkCache";
    private readonly string sdkUrl = "https://cdn.bright-sdk.com/static/";
    private readonly string sdkVersionsUrl = "https://bright-sdk.com/sdk_api/sdk/versions";

    private readonly Dictionary<string, string> sdkVersions = new Dictionary<string, string>();

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("BrightSdkPreBuildProcessor: OnPreprocessBuild called");

        if (report.summary.platform == BuildTarget.Android)
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform is Android, updating Bright SDK");
            UpdateBrightSdk();
        }
        else
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform is not Android, skipping Bright SDK update");
        }
    }

    private void UpdateBrightSdk()
    {
        Debug.Log("BrightSdkPreBuildProcessor: Starting Bright SDK update");
        ParseBrightSdkArgs();
        FetchBrightSdkVersions();
        PopulateBrightSdkVersions();
        AdjustBrightSdkVersion();
        DownloadBrightSdk();
        RemoveObsoleteAarFiles();
        ExtractBrightSdk();
        Debug.Log("BrightSdkPreBuildProcessor: Bright SDK updated successfully");
    }

    private void ParseBrightSdkArgs()
    {
        // Parse arguments for the SDK
        Debug.Log("BrightSdkPreBuildProcessor: Parsing Bright SDK arguments");
    }

    private void FetchBrightSdkVersions()
    {
        // Fetch the latest SDK versions
        Debug.Log("BrightSdkPreBuildProcessor: Fetching Bright SDK versions");
        string sdkVersionsFile = Path.Combine(cacheDir, "sdk_versions.json");

        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        else if (File.Exists(sdkVersionsFile))
        {
            FileInfo fileInfo = new FileInfo(sdkVersionsFile);
            if (fileInfo.LastWriteTime < System.DateTime.Now.AddDays(-1))
            {
                File.Delete(sdkVersionsFile);
            }
        }

        if (!File.Exists(sdkVersionsFile))
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(sdkVersionsUrl, sdkVersionsFile);
            }
        }

        string jsonContent = File.ReadAllText(sdkVersionsFile);
        Debug.Log("SDK versions json content: " + jsonContent);

        SdkVersions sdkVersionsData = JsonUtility.FromJson<SdkVersions>(jsonContent);
        sdkVersions["android"] = sdkVersionsData.android;

        Debug.Log("SDK versions: " + string.Join(", ", sdkVersions.Select(kv => kv.Key + "=" + kv.Value)));
    }

    private void PopulateBrightSdkVersions()
    {
        // Populate the SDK versions
        Debug.Log("BrightSdkPreBuildProcessor: Populating Bright SDK versions");
    }

    private void AdjustBrightSdkVersion()
    {
        // Adjust the SDK version
        Debug.Log("BrightSdkPreBuildProcessor: Adjusting Bright SDK version");
        if (sdkVersion == "latest" && sdkVersions.ContainsKey("android"))
        {
            sdkVersion = sdkVersions["android"];
        }
    }

    private void DownloadBrightSdk()
    {
        // Download the SDK
        Debug.Log("BrightSdkPreBuildProcessor: Downloading Bright SDK");
        string targzName = "bright_sdk_android-" + sdkVersion + ".tar.gz";
        string targzFile = Path.Combine(cacheDir, targzName);

        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        else if (!File.Exists(targzFile))
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(sdkUrl + targzName, targzFile);
            }
        }
    }

    private void RemoveObsoleteAarFiles()
    {
        // Remove obsolete bright_sdk*.aar files
        Debug.Log("BrightSdkPreBuildProcessor: Removing obsolete bright_sdk*.aar files");
        string[] obsoleteAarFiles = Directory.GetFiles(sdkDir, "bright_sdk*.aar", SearchOption.TopDirectoryOnly);
        foreach (string file in obsoleteAarFiles)
        {
            Debug.Log($"Deleting obsolete AAR file: {file}");
            File.Delete(file);
        }
    }

    private void ExtractBrightSdk()
    {
        // Ensure necessary directories exist
        if (!Directory.Exists(sdkDir))
        {
            Directory.CreateDirectory(sdkDir);
        }

        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        // Extract the SDK
        Debug.Log("BrightSdkPreBuildProcessor: Extracting Bright SDK");
        string targzName = "bright_sdk_android-" + sdkVersion + ".tar.gz";
        string targzFile = Path.Combine(cacheDir, targzName);
        string extractDir = Path.Combine(cacheDir, "extracted");

        if (Directory.Exists(extractDir))
        {
            Directory.Delete(extractDir, true);
        }

        Directory.CreateDirectory(extractDir);

        using (FileStream fs = new FileStream(targzFile, FileMode.Open, FileAccess.Read))
        using (GZipInputStream gzipStream = new GZipInputStream(fs))
        using (TarInputStream tarStream = new TarInputStream(gzipStream, Encoding.UTF8))
        {
            TarEntry entry;
            while ((entry = tarStream.GetNextEntry()) != null)
            {
                string name = entry.Name;
                string outputPath = Path.Combine(extractDir, name);

                Debug.Log($"Extracting {name} to {outputPath}");

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(outputPath);
                }
                else
                {
                    string directoryPath = Path.GetDirectoryName(outputPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        tarStream.CopyEntryContents(outputStream);
                    }
                }
            }
        }

        // Log the contents of the extracted directory
        LogDirectoryContents(extractDir);

        // Find the .aar file recursively
        string aarFile = Directory.GetFiles(extractDir, "*.aar", SearchOption.AllDirectories).FirstOrDefault();
        string destAarFile = Path.Combine(sdkDir, "bright_sdk-" + sdkVersion + ".aar");

        if (File.Exists(destAarFile))
        {
            File.Delete(destAarFile);
        }

        if (aarFile != null && File.Exists(aarFile))
        {
            File.Copy(aarFile, destAarFile);
            Debug.Log($"AAR file found and copied from {aarFile} to {destAarFile}");
        }
        else
        {
            Debug.LogError($"AAR file not found in {extractDir}");
        }
    }

    private void LogDirectoryContents(string path)
    {
        Debug.Log($"Contents of {path}:");
        foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
        {
            Debug.Log($"Directory: {dir}");
        }
        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            Debug.Log($"File: {file}");
        }
    }
}

[Serializable]
public class SdkVersions
{
    public string android;
    // Add other fields if necessary
}