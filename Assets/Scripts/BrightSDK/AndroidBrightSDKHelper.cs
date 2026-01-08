using System;
using UnityEngine;
using UnityEngine.Events;

public class AndroidBrightSDKHelper : BrightSDKHelper
{
    private ChoiceListener choiceListener;
    private AndroidJavaObject brightApi;
    private AndroidJavaObject currentActivity;

    void Awake()
    {
        choiceListener = new ChoiceListener(OnStatusChange);
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

    public override void ShowConsent()
    {
        brightApi.CallStatic("showConsent", currentActivity);
    }

    public override void OptOut()
    {
        brightApi.CallStatic("optOut", currentActivity);
    }

    public override bool IsEnabled()
    {
        int choice = brightApi.CallStatic<int>("getChoice", currentActivity);
        return choiceListener.isEnabled(choice);
    }

    private void OnStatusChange(bool isEnabled)
    {
        if (onStatusChangeCallback != null)
            onStatusChangeCallback.Invoke(isEnabled);
    }

    private class ChoiceListener : AndroidJavaProxy
    {
        private Action<bool> onChangeCallback;

        public ChoiceListener(Action<bool> onChangeCallback) : base("com.android.eapx.Settings$OnStatusChange")
        {
            this.onChangeCallback = onChangeCallback;
        }

        public void onChange(int choice)
        {
            Debug.Log($"Bright SDK consent choice changed: {choice}");
            if (onChangeCallback != null)
                onChangeCallback(isEnabled(choice));
        }

        public bool isEnabled(int choice)
        {
            return choice == 1;
        }
    }

}

