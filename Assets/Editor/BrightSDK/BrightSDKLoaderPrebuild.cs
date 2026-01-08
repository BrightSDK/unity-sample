using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Collections.Generic;

public class BrightSDKLoaderPrebuild : IPreprocessBuildWithReport
{
    private readonly Dictionary<BuildTarget, BrightSDKExtractor> extractors = new Dictionary<BuildTarget, BrightSDKExtractor>();
    private readonly Dictionary<BuildTarget, BrightSDKArchiveDownloader> archiveDownloaders = new Dictionary<BuildTarget, BrightSDKArchiveDownloader>();
    private BrightSDKVersions sdkVersions;

    public int callbackOrder => 0;

    public BrightSDKLoaderPrebuild()
    {
        sdkVersions = new BrightSDKVersions();

        extractors[BuildTarget.Android] = new AndroidBrightSDKExtractor();
        extractors[BuildTarget.iOS] = extractors[BuildTarget.tvOS] = new AppleMobileBrightSDKExtractor();
        extractors[BuildTarget.StandaloneOSX] = new AppleDesktopBrightSDKExtractor();

        archiveDownloaders[BuildTarget.Android] = new AndroidSDKArchiveDownloader();
        archiveDownloaders[BuildTarget.iOS] = archiveDownloaders[BuildTarget.tvOS] = new AppleMobileSDKArchiveDownloader();
        archiveDownloaders[BuildTarget.StandaloneOSX] = new AppleDesktopSDKArchiveDownloader();
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("BrightSDKLoaderPrebuild: OnPreprocessBuild called");
        BuildTarget platform = report.summary.platform;
        if (isPlatformSupported(platform))
        {
            Debug.Log("BrightSDKLoaderPrebuild: Platform is " + platform + ", updating Bright SDK");
            UpdateBrightSdk(platform);
        }
        else
        {
            Debug.Log("BrightSDKLoaderPrebuild: Platform " + platform + " is not supported, skipping Bright SDK update");
        }
    }

    private bool isPlatformSupported(BuildTarget platform)
    {
        return archiveDownloaders.ContainsKey(platform) && extractors.ContainsKey(platform);
    }

    private void UpdateBrightSdk(BuildTarget platform)
    {
        Debug.Log("BrightSDKLoaderPrebuild: Starting Bright SDK update");
        sdkVersions.load();

        if (isPlatformSupported(platform) && sdkVersions.LastVersion(platform) != null)
        {
            string lastVersion = sdkVersions.LastVersion(platform);
            string archiveFile = archiveDownloaders[platform].Download(lastVersion);
            extractors[platform].Extract(archiveFile);
        }

        Debug.Log("BrightSDKLoaderPrebuild: Bright SDK updated successfully");
    }
}