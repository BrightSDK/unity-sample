using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Unity.SharpZipLib.Tar;
using Unity.SharpZipLib.GZip;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEditor;

interface BrightSDKExtractor
{
    public void Extract(string sourceFile);
}

class AndroidBrightSDKExtractor : BrightSDKExtractor
{
    private string sdkDir;

    public AndroidBrightSDKExtractor()
    {
        sdkDir = BrightSDKDirectory.PluginsDir("Android");
    }

    public void Extract(string sourceFile)
    {
        RemoveObsoleteFiles();
        ExtractBrightSdk(sourceFile);
    }

    private void RemoveObsoleteFiles()
    {
        // Remove obsolete bright_sdk*.aar files
        Debug.Log("AndroidBrightSDKExtractor: Removing obsolete bright_sdk*.aar files");
        string[] obsoleteAarFiles = Directory.GetFiles(sdkDir, "bright_sdk*.aar", SearchOption.TopDirectoryOnly);
        foreach (string file in obsoleteAarFiles)
        {
            Debug.Log($"AndroidBrightSDKExtractor: Deleting obsolete AAR file {file}");
            File.Delete(file);
        }
    }

    private void ExtractBrightSdk(string sourceFile)
    {
        Debug.Log("AndroidBrightSDKExtractor: Extracting Bright SDK");
        string extractDir = Path.Combine(BrightSDKDirectory.CacheDir, "extracted/Android");
        if (Directory.Exists(extractDir))
            Directory.Delete(extractDir, true);
        Directory.CreateDirectory(extractDir);

        unzip(sourceFile, extractDir);

        // Find the .aar file recursively
        string aarFile = Directory.GetFiles(extractDir, "*.aar", SearchOption.AllDirectories).FirstOrDefault();
        string destAarFile = Path.Combine(sdkDir, "bright_sdk.aar");

        if (File.Exists(destAarFile))
            File.Delete(destAarFile);

        if (aarFile != null && File.Exists(aarFile))
        {
            File.Copy(aarFile, destAarFile);
            AssetDatabase.Refresh();
            Debug.Log($"AndroidBrightSDKExtractor: AAR file found and copied from {aarFile} to {destAarFile}");
        }
        else
            Debug.LogError($"AndroidBrightSDKExtractor: AAR file not found in {extractDir}");
    }

    private void unzip(string sourceFile, string extractDir)
    {
        using (FileStream fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
        using (GZipInputStream gzipStream = new GZipInputStream(fs))
        using (TarInputStream tarStream = new TarInputStream(gzipStream, Encoding.UTF8))
        {
            TarEntry entry;
            while ((entry = tarStream.GetNextEntry()) != null)
            {
                string name = entry.Name;
                string outputPath = Path.Combine(extractDir, name);

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
    }
}

// --- Apple platforms ---

class AppleBrightSDKExtractor : BrightSDKExtractor
{
    private string relativeSdkPath;
    private string sdkDir;

    public AppleBrightSDKExtractor(string _relativeSdkPath)
    {
        relativeSdkPath = _relativeSdkPath;//"Apple/BrightDataSDK";
        sdkDir = BrightSDKDirectory.PluginsDir(relativeSdkPath);
    }

    public void Extract(string sourceFile)
    {
        RemoveObsoleteFiles();
        ExtractBrightSdk(sourceFile);
    }

    public virtual string ConstructSourcePath(string extractDir)
    {
        return extractDir;
    }

    public virtual void DidUnzipToTempDir(string srcDir)
    {
    }

    public virtual void DidCopyFilesToDestination(string destDir)
    {
    }

    private void RemoveObsoleteFiles()
    {
        Debug.Log("AppleBrightSDKExtractor: Removing obsolete SDK's files");
        foreach (string file in Directory.GetFiles(sdkDir, "*.*"))
        {
            Debug.Log($"AppleBrightSDKExtractor: Deleting file {file}");
            File.Delete(file);
        }
        foreach (string dir in Directory.GetDirectories(sdkDir))
        {
            Debug.Log($"AppleBrightSDKExtractor: Deleting folder {dir}");
            Directory.Delete(dir, true);
        }
    }

    private void ExtractBrightSdk(string sourceFile)
    {
        Debug.Log("AppleBrightSDKExtractor: Extracting Bright SDK");
        string extractDir = Path.Combine(BrightSDKDirectory.CacheDir, "extracted", relativeSdkPath);
        if (Directory.Exists(extractDir))
            Directory.Delete(extractDir, true);
        Directory.CreateDirectory(extractDir);

        dittoExtractZip(sourceFile, extractDir);

        string destDir = sdkDir;
        if (Directory.Exists(destDir))
            Directory.Delete(destDir, true);

        string srcDir = ConstructSourcePath(extractDir);
        DidUnzipToTempDir(srcDir);

        dittoCopy(srcDir, destDir);
        DidCopyFilesToDestination(destDir);

        AssetDatabase.Refresh();
        Debug.Log("AppleBrightSDKExtractor: Bright SDK files copied");
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

class AppleMobileBrightSDKExtractor: AppleBrightSDKExtractor
{
    public AppleMobileBrightSDKExtractor() : base("Apple/BrightDataSDK")
    {
    }

    public override string ConstructSourcePath(string extractDir)
    {
        string srcDir = Path.Combine(extractDir, "unity_plugin/BrightDataSDK");
        if (!Directory.Exists(srcDir))
            srcDir = Path.Combine(extractDir, "unity_editor_sample_app/Assets/BrightDataSDK");
        return srcDir;
    }

    public override void DidUnzipToTempDir(string srcDir)
    {
        string asmDefFile = Path.Combine(srcDir, "AppleBrightSDK.asmdef");
        if (File.Exists(asmDefFile))
            File.Delete(asmDefFile);
        asmDefFile = Path.Combine(srcDir, "AppleBrightSDK.asmdef.meta");
        if (File.Exists(asmDefFile))
            File.Delete(asmDefFile);
    }

    public override void DidCopyFilesToDestination(string destDir)
    {
        setSettingsOfFramework(destDir);
    }

    private void setSettingsOfFramework(string frameworkRoot)
    {
        Debug.Log("AppleMobileBrightSDKExtractor: Set settings for framework");
        string frameworkPath = Path.Combine(frameworkRoot, "brdsdk.framework");
        PluginImporter plugin = AssetImporter.GetAtPath(frameworkPath) as PluginImporter;
        if (plugin == null)
            return;
        plugin.SetCompatibleWithAnyPlatform(false);
        plugin.SetCompatibleWithEditor(false);
        plugin.SetCompatibleWithPlatform(BuildTarget.iOS, true);
        plugin.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
        plugin.SetCompatibleWithPlatform(BuildTarget.Android, false);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
        plugin.SaveAndReimport();
    }
}

class AppleDesktopBrightSDKExtractor: AppleBrightSDKExtractor
{
    public AppleDesktopBrightSDKExtractor() : base("Apple/BrightDataSDK-macOS")
    {
    }

    public override void DidUnzipToTempDir(string srcDir)
    {
        string editorPath = Path.Combine(srcDir, "Editor");
        if (Directory.Exists(editorPath))
            Directory.Delete(editorPath, true);
    }

    public override void DidCopyFilesToDestination(string destDir)
    {
        setSettingsOfFramework(destDir);
    }

    private void setSettingsOfFramework(string frameworkRoot)
    {
        Debug.Log("AppleDesktopBrightSDKExtractor: Set settings for framework");
        string frameworkPath = Path.Combine(frameworkRoot, "brdsdk.framework");
        PluginImporter plugin = AssetImporter.GetAtPath(frameworkPath) as PluginImporter;
        if (plugin == null)
            return;
        plugin.SetCompatibleWithAnyPlatform(false);
        plugin.SetCompatibleWithEditor(false);
        plugin.SetCompatibleWithPlatform(BuildTarget.iOS, false);
        plugin.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
        plugin.SetCompatibleWithPlatform(BuildTarget.Android, false);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, true);
        plugin.SaveAndReimport();
    }
}