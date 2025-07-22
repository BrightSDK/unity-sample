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
    private readonly string cacheDir = "Library/BrightSdkCache";
    private readonly string sdkUrl = "https://cdn.bright-sdk.com/static/";
    private readonly string sdkVersionsUrl = "https://bright-sdk.com/sdk_api/sdk/versions";

    private readonly Dictionary<BuildTarget, string> sdkVersions = new Dictionary<BuildTarget, string>();
    private readonly Dictionary<BuildTarget, BrightSDKDowloader> downloaders = new Dictionary<BuildTarget, BrightSDKDowloader>();

    public int callbackOrder => 0;

    public BrightSdkPreBuildProcessor()
    {
        downloaders[BuildTarget.Android] = new AndroidBrightSDKDowloader(cacheDir, "Assets/Plugins", sdkUrl);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("BrightSdkPreBuildProcessor: OnPreprocessBuild called");

        BuildTarget platform = report.summary.platform;
        if (downloaders.ContainsKey(platform))
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform is " + platform + ", updating Bright SDK");
            UpdateBrightSdk(platform);
        }
        else
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform " + platform + " is not supported, skipping Bright SDK update");
        }
    }

    private void UpdateBrightSdk(BuildTarget platform)
    {
        Debug.Log("BrightSdkPreBuildProcessor: Starting Bright SDK update");
        ParseBrightSdkArgs();
        FetchBrightSdkVersions();
        PopulateBrightSdkVersions();

        if (downloaders.ContainsKey(platform) && sdkVersions.ContainsKey(platform))
        {
            string version = sdkVersions[platform];
            downloaders[platform].Download(version);
        }

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
        // TODO: is it necessary?
        sdkVersions[BuildTarget.Android] = sdkVersionsData.android;
        sdkVersions[BuildTarget.iOS] = sdkVersionsData.ios;

        Debug.Log("SDK versions: " + string.Join(", ", sdkVersions.Select(kv => kv.Key + "=" + kv.Value)));
    }

    private void PopulateBrightSdkVersions()
    {
        // Populate the SDK versions
        Debug.Log("BrightSdkPreBuildProcessor: Populating Bright SDK versions");
    }
}

[Serializable]
public class SdkVersions
{
    public string android;
    public string ios;
    // Add other fields if necessary
}

interface BrightSDKDowloader
{
    public void Download(string publicVersion);
}

class AndroidBrightSDKDowloader: BrightSDKDowloader
{
    // null for latest
    private string sdkVersion = "1.543.127";
    private string cacheDir;
    private string sdkDir;
    private string sdkUrl;

    public AndroidBrightSDKDowloader(string _cacheDir, string pluginsDir, string _sdkUrl)
    {
        cacheDir = _cacheDir;
        sdkDir = Path.Combine(pluginsDir, "Android");
        sdkUrl = _sdkUrl;

        if (!Directory.Exists(sdkDir))
        {
            Directory.CreateDirectory(sdkDir);
        }
    }

    public void Download(string publicVersion)
    {
        if (sdkVersion == null)
            sdkVersion = publicVersion;
        DownloadBrightSdk();
        RemoveObsoleteAarFiles();
        ExtractBrightSdk();
    }

    private void DownloadBrightSdk()
    {
        // Download the SDK
        Debug.Log("AndroidBrightSDKDowloader: Downloading Bright SDK " + sdkVersion);
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
        Debug.Log("AndroidBrightSDKDowloader: Removing obsolete bright_sdk*.aar files");
        string[] obsoleteAarFiles = Directory.GetFiles(sdkDir, "bright_sdk*.aar", SearchOption.TopDirectoryOnly);
        foreach (string file in obsoleteAarFiles)
        {
            Debug.Log($"AndroidBrightSDKDowloader: Deleting obsolete AAR file {file}");
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
        Debug.Log("AndroidBrightSDKDowloader: Extracting Bright SDK");
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
        // LogDirectoryContents(extractDir);

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
            Debug.Log($"AndroidBrightSDKDowloader: AAR file found and copied from {aarFile} to {destAarFile}");
        }
        else
        {
            Debug.LogError($"AndroidBrightSDKDowloader: AAR file not found in {extractDir}");
        }
    }

    private void LogDirectoryContents(string path)
    {
        Debug.Log($"AndroidBrightSDKDowloader: Contents of {path}:");
        foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
        {
            Debug.Log($"AndroidBrightSDKDowloader: Directory {dir}");
        }
        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            Debug.Log($"AndroidBrightSDKDowloader: File {file}");
        }
    }
}
