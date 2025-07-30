using System;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEditor;

[Serializable]
class SdkVersions
{
    public string android;
    public string ios;
    // Add other fields if necessary
}

class BrightSDKVersions
{
    private readonly string sdkVersionsUrl = "https://bright-sdk.com/sdk_api/sdk/versions";
    private SdkVersions lastVersions;

    public void load()
    {
        Debug.Log("Fetching Bright SDK versions");
        string jsonContent = getVersionsContent();
        lastVersions = JsonUtility.FromJson<SdkVersions>(jsonContent);
        Debug.Log("Loaded SDK versions: " + lastVersions);
    }

    public string LastVersion(BuildTarget platform)
    {
        if (platform == BuildTarget.iOS || platform == BuildTarget.tvOS)
            return lastVersions?.ios;
        else if (platform == BuildTarget.Android)
            return lastVersions?.android;

        return null;
    }

    private string getVersionsContent()
    {
        string sdkVersionsFile = Path.Combine(BrightSDKDirectory.CacheDir, "sdk_versions.json");

        if (File.Exists(sdkVersionsFile))
        {
            FileInfo fileInfo = new FileInfo(sdkVersionsFile);
            if (fileInfo.LastWriteTime < System.DateTime.Now.AddDays(-1))
                File.Delete(sdkVersionsFile);
        }

        if (!File.Exists(sdkVersionsFile))
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(sdkVersionsUrl, sdkVersionsFile);
            }
        }

        return File.ReadAllText(sdkVersionsFile);
    }
}
