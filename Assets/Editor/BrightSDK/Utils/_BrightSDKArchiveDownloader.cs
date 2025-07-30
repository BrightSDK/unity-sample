using System;
using System.IO;
using System.Net;
using UnityEngine;

class BrightSDKArchiveDownloader
{
    private const string sdkUrl = "https://cdn.bright-sdk.com/static/";

    // null for latest
    public virtual string PredefinedVersion => null;

    public string Download(string lastVersion)
    {
        string remoteName = MakeRemoteFileName(lastVersion);
        if (remoteName == null)
        {
            Debug.LogError("SDKArchiveDownloader: Unknown sdk remote file name.");
            return null;
        }
        string downloadURL = sdkUrl + remoteName;
        string targetFile = Path.Combine(BrightSDKDirectory.CacheDir, remoteName);
        downloadFile(downloadURL, targetFile);
        return targetFile;
    }

    public virtual string MakeRemoteFileName(string lastVersion)
    {
        return null;
    }

    private void downloadFile(string url, string targetFile)
    {
        if (!File.Exists(targetFile))
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, targetFile);
            }
        }
    }
}

class AndroidSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    // null for latest
    public override string PredefinedVersion => null;
    public override string MakeRemoteFileName(string lastVersion)
    {
        string version = PredefinedVersion ?? lastVersion;
        return "bright_sdk_android-" + version + ".tar.gz";
    }
}

class AppleSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    // null for latest
    public override string PredefinedVersion => null;
    public override string MakeRemoteFileName(string lastVersion)
    {
        string version = PredefinedVersion ?? lastVersion;
        return "bright_sdk_ios-" + version + ".zip";
    }
}