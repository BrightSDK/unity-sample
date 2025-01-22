using System;
using UnityEngine;
using UnityEngine.Events;

public class BrightSdkHelper : MonoBehaviour
{
    private ChoiceListener choiceListener;

    private AndroidJavaObject brightApi;
    private AndroidJavaObject currentActivity;

    public string benefit = "To unlock premium features";
    public string agreeBtn = "Yes, sure!";
    public string disagreeBtn = "No, thanks!";
    public bool skipConsent = false;

    [Serializable]
    public class StatusChangeEvent : UnityEvent<bool> { }
    public StatusChangeEvent onStatusChangeCallback;

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

