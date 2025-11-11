using System;

[System.Serializable]
public class BrightSDKConfig
{
    [System.Serializable]
    public class Versions
    {
        public string android;
        public string appleMobile;
        public string appleDesktop;

        public string this[string key]
        {
            get
            {
                switch (key)
                {
                    case "android": return android;
                    case "apple_mobile": return appleMobile;
                    case "apple_desktop": return appleDesktop;
                    default: return null;
                }
            }
        }
    }

    public Versions versions;
}