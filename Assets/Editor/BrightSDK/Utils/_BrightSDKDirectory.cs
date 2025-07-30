using System;
using System.IO;

static class BrightSDKDirectory
{
    public static string CacheDir => AnyDir("Library/BrightSdkCache");

    public static string PluginsDir(string subfolder)
    {
        return AnyDir("Assets/Plugins", subfolder);
    }

    public static string AnyDir(string source, string subfolder = null)
    {
        string dir = source;
        if (subfolder != null)
            dir = Path.Combine(dir, subfolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }
        if (recursive)
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
    }
}