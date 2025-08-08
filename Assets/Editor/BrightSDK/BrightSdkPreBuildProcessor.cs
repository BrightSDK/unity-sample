using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Collections.Generic;

public class BrightSdkPreBuildProcessor : IPreprocessBuildWithReport
{
    private readonly Dictionary<BuildTarget, BrightSDKExtractor> extractors = new Dictionary<BuildTarget, BrightSDKExtractor>();
    private readonly Dictionary<BuildTarget, BrightSDKArchiveDownloader> archiveDownloaders = new Dictionary<BuildTarget, BrightSDKArchiveDownloader>();
    private BrightSDKVersions sdkVersions;

    public int callbackOrder => 0;

    public BrightSdkPreBuildProcessor()
    {
        sdkVersions = new BrightSDKVersions();

        extractors[BuildTarget.Android] = new AndroidBrightSDKDownloader();
        extractors[BuildTarget.iOS] = extractors[BuildTarget.tvOS] = new AppleBrightSDKDownloader();

        archiveDownloaders[BuildTarget.Android] = new AndroidSDKArchiveDownloader();
        archiveDownloaders[BuildTarget.iOS] = archiveDownloaders[BuildTarget.tvOS] = new AppleSDKArchiveDownloader();
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("BrightSdkPreBuildProcessor: OnPreprocessBuild called");
        BuildTarget platform = report.summary.platform;
        if (isPlatformSupported(platform))
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform is " + platform + ", updating Bright SDK");
            UpdateBrightSdk(platform);
        }
        else
        {
            Debug.Log("BrightSdkPreBuildProcessor: Platform " + platform + " is not supported, skipping Bright SDK update");
        }
    }

    private bool isPlatformSupported(BuildTarget platform)
    {
        return archiveDownloaders.ContainsKey(platform) && extractors.ContainsKey(platform);
    }

    private void UpdateBrightSdk(BuildTarget platform)
    {
        Debug.Log("BrightSdkPreBuildProcessor: Starting Bright SDK update");
        sdkVersions.load();

        if (isPlatformSupported(platform) && sdkVersions.LastVersion(platform) != null)
        {
            string lastVersion = sdkVersions.LastVersion(platform);
            string archiveFile = archiveDownloaders[platform].Download(lastVersion);
            extractors[platform].Extract(archiveFile);
        }

        Debug.Log("BrightSdkPreBuildProcessor: Bright SDK updated successfully");
    }
}