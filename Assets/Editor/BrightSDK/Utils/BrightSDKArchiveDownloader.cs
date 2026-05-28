using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;

class BrightSDKArchiveDownloader
{
    private const string IntegrationConfigUrl =
        "https://bright-sdk.com/sdk_api/sdk/integration/config";

    // Fallback base URL used when the integration config API is unavailable
    public virtual string sdkUrl => "https://cdn.bright-sdk.com/static/";

    public virtual string VersionsPlatformKey => null;

    // Platform key as returned by the integration config API (may differ from VersionsPlatformKey)
    public virtual string IntegrationPlatformKey => null;

    public string Download(string lastVersion)
    {
        string apiKey = System.Environment.GetEnvironmentVariable("SDK_API_KEY");
        string resolvedUrl = null;
        if (!string.IsNullOrEmpty(apiKey) && IntegrationPlatformKey != null)
            resolvedUrl = resolveUrlFromConfig(apiKey, IntegrationPlatformKey, lastVersion);

        string configVersion = getConfigVersion();
        string remoteName = MakeRemoteFileName(configVersion, lastVersion);
        if (remoteName == null && resolvedUrl == null)
        {
            Debug.LogError("SDKArchiveDownloader: Unknown sdk remote file name.");
            return null;
        }
        string downloadURL = resolvedUrl ?? (sdkUrl + remoteName);
        string targetFile = Path.Combine(BrightSDKDirectory.CacheDir,
            remoteName ?? Path.GetFileName(downloadURL));
        downloadFile(downloadURL, targetFile);
        return targetFile;
    }

    public virtual string MakeRemoteFileName(string configVersion, string lastVersion)
    {
        return null;
    }

    private string resolveUrlFromConfig(string apiKey, string platformKey, string fallbackVersion)
    {
        try
        {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(IntegrationConfigUrl);
            req.Headers.Add("api-key", apiKey);
            req.Timeout = 10000;
            using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                // Minimal JSON extraction without a full JSON parser
                string ver = extractJsonString(json, "\"" + platformKey + "\".*?\"last_version\":\\s*\"([^\"]+)\"");
                string urlTpl = extractJsonString(json, "\"" + platformKey + "\".*?\"url_tpl\":\\s*\"([^\"]+)\"");
                string baseTpl = extractJsonString(json, "\"base\":\\s*\"([^\"]+)\"");
                string commonTpl = extractJsonString(json, "\"common\":\\s*\"([^\"]+)\"");
                string tvTpl = extractJsonString(json, "\"tv\":\\s*\"([^\"]+)\"");
                if (string.IsNullOrEmpty(urlTpl) || string.IsNullOrEmpty(baseTpl)) return null;
                string version = string.IsNullOrEmpty(ver) ? fallbackVersion : ver;
                string url = urlTpl;
                if (!string.IsNullOrEmpty(commonTpl))
                    url = url.Replace("{{common}}", commonTpl);
                if (!string.IsNullOrEmpty(tvTpl))
                    url = url.Replace("{{tv}}", tvTpl);
                url = url.Replace("{{base}}", baseTpl)
                         .Replace("{{platform}}", platformKey)
                         .Replace("{{version}}", version);
                Debug.Log($"SDKArchiveDownloader: resolved URL from integration config: {url}");
                return url;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SDKArchiveDownloader: integration config fetch failed: {e.Message}. Falling back to static URL.");
            return null;
        }
    }

    private string extractJsonString(string json, string pattern)
    {
        var match = System.Text.RegularExpressions.Regex.Match(json, pattern,
            System.Text.RegularExpressions.RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : null;
    }

    private void downloadFile(string url, string targetFile)
    {
        if (!File.Exists(targetFile))
        {
            Debug.Log($"SDKArchiveDownloader: {targetFile} not exists, downloading");
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, targetFile);
            }
        } else {
            Debug.Log($"SDKArchiveDownloader: Reusing downloaded file {targetFile}");
        }
    }

    private string loadConfigVersionPath()
    {
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) == "BrightSDK.json")
                return path;
        }
        return null;
    }

    private string getConfigVersion()
    {
        string path = loadConfigVersionPath();
        if (string.IsNullOrWhiteSpace(path)) return null;
        string json = File.ReadAllText(path);
        BrightSDKConfig config = JsonUtility.FromJson<BrightSDKConfig>(json);
        string version = config.versions[VersionsPlatformKey];
        if (string.IsNullOrWhiteSpace(version)) return null;
        return version;
    }
}

class AndroidSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    public override string VersionsPlatformKey => "android";
    public override string IntegrationPlatformKey => "android";
    public override string MakeRemoteFileName(string configVersion, string lastVersion)
    {
        string version = configVersion ?? lastVersion;
        return "bright_sdk_android-" + version + ".tar.gz";
    }
}

class AppleMobileSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    public override string VersionsPlatformKey => "apple_mobile";
    public override string IntegrationPlatformKey => "ios";
    public override string MakeRemoteFileName(string configVersion, string lastVersion)
    {
        string version = configVersion ?? lastVersion;
        return "bright_sdk_ios-" + version + ".zip";
    }
}

class AppleDesktopSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    public override string VersionsPlatformKey => "apple_desktop";
    public override string IntegrationPlatformKey => "macos";
    public override string MakeRemoteFileName(string configVersion, string lastVersion)
    {
        string version = configVersion ?? lastVersion;
        return "bright_sdk_macos_unity-" + version + ".zip";
    }
}

class WindowsSDKArchiveDownloader : BrightSDKArchiveDownloader
{
    public override string VersionsPlatformKey => "windows";
    public override string IntegrationPlatformKey => "win";
    public override string MakeRemoteFileName(string configVersion, string lastVersion)
    {
        string version = configVersion ?? lastVersion;
        return "bright_sdk_win-" + version + ".zip";
    }
}