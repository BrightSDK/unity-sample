using System;
using UnityEngine;
using UnityEngine.Events;

public class BrightSDKHelper : MonoBehaviour
{
    [Serializable]
    public class StatusChangeEvent : UnityEvent<bool> { }

    public StatusChangeEvent onStatusChangeCallback;
    public string benefit = "To unlock premium features";
    public string agreeBtn = "Yes, sure!";
    public string disagreeBtn = "No, thanks!";
    public bool skipConsent = false;

    public virtual void ShowConsent()
    {
    }

    public virtual void OptOut()
    {
    }

    public virtual bool IsEnabled()
    {
        return false;
    }
}