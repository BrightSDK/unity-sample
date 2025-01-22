using System;
using UnityEngine;

public class BrightSdkHelper
{
    private ChoiceListener choiceListener;

    private AndroidJavaObject brightApi;
    private AndroidJavaObject currentActivity;

    public string benefit = "To unlock premium features";
    public string agreeBtn = "Yes, sure!";
    public string disagreeBtn = "No, thanks!";
    public bool skipConsent = false;

    public BrightSdkHelper(BrightSdkController parent)
    {
        choiceListener = new ChoiceListener(parent.UpdateStatus);
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        brightApi = new AndroidJavaObject("com.android.eapx.BrightApi");
        AndroidJavaObject settings = new AndroidJavaObject("com.android.eapx.Settings", currentActivity);
        settings.Call("setBenefit", benefit);
        settings.Call("setAgreeBtn", agreeBtn);
        settings.Call("setDisagreeBtn", disagreeBtn);
        settings.Call("setSkipConsent", skipConsent);
        settings.Call("setOnStatusChange", choiceListener);
        brightApi.CallStatic("init", currentActivity, settings);
    }

    public void ShowConsent()
    {
        brightApi.CallStatic("showConsent", currentActivity);
    }

    public void OptOut()
    {
        brightApi.CallStatic("optOut", currentActivity);
    }

    public bool IsEnabled()
    {
        int choice = brightApi.CallStatic<int>("getChoice", currentActivity);
        return choiceListener.isEnabled(choice);
    }

    private class ChoiceListener : AndroidJavaProxy
    {
        public Action<bool> onChangeCallback;

        public ChoiceListener(Action<bool> onChangeCallback) : base("com.android.eapx.Settings$OnStatusChange")
        {
            this.onChangeCallback = onChangeCallback;
        }

        public void onChange(int choice)
        {
            Debug.Log($"Bright SDK consent choice changed: {choice}");
            onChangeCallback(isEnabled(choice));
        }

        public bool isEnabled(int choice)
        {
            return choice == 1;
        }
    }

}

