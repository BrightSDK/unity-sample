using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using Brdsdk;

public class AppleBrightSDKHelper : BrightSDKHelper
{
    void Awake()
    {
        BrdsdkBridge.set_on_choice_change_callback(choiceChanged);
        BrdsdkBridge.tryInit(benefit, agreeBtn, disagreeBtn, null, null, skipConsent);
    }

    public override void ShowConsent()
    {
        BrdsdkBridge.show_consent();
    }

    public override void OptOut()
    {
        BrdsdkBridge.opt_out(ChoiceTriggerType.Manual);
    }

    public override bool IsEnabled()
    {
        return BrdsdkBridge.current_choice() == Brdsdk.Choice.Peer;
    }

    private void choiceChanged(Choice choice)
    {
        bool enabled = choice == Brdsdk.Choice.Peer;
        if (onStatusChangeCallback != null)
            onStatusChangeCallback.Invoke(enabled);
    }
}