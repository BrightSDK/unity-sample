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

    private bool? lastChoice = null;
    private readonly object lastChoiceLock = new object();

    public virtual void ShowConsent()
    {
    }

    public virtual void ExternalOptIn()
    {
    }

    public virtual void NotifyConsentShown()
    {
    }

    public virtual void OptOut()
    {
    }

    public virtual bool IsEnabled()
    {
        return false;
    }

    public void NotifyChoiceChangeListeners(bool isEnabled)
    {
        lock (lastChoiceLock)
        {
            lastChoice = isEnabled;
        }
    }

    private void Update()
    {
        if (onStatusChangeCallback == null) return;
        bool value;

        lock (lastChoiceLock)
        {
            if (lastChoice == null) return;
            value = lastChoice.Value;
            lastChoice = null;
        }
        onStatusChangeCallback.Invoke(value);
    }
}