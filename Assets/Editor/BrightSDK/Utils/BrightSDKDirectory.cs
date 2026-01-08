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
}