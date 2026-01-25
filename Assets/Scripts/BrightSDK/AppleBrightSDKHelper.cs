using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using AOT;

#if APPLE_BRIGHT_SDK && (UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX)
using Brdsdk;
#endif
public class AppleBrightSDKHelper : BrightSDKHelper
{
#if APPLE_BRIGHT_SDK && (UNITY_IOS || UNITY_TVOS)
    void Awake()
    {
        BrdsdkBridge.set_on_choice_change_callback(choiceChanged);
        BrdsdkBridge.tryInit(benefit, agreeBtn, disagreeBtn, null, null, skipConsent);
    }

    public override void ExternalOptIn()
    {
        BrdsdkBridge.external_opt_in(ChoiceTriggerType.Manual);
    }

    public override void NotifyConsentShown()
    {
        BrdsdkBridge.notify_consent_shown();
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
        bool isEnabled = choice == Brdsdk.Choice.Peer;
        NotifyChoiceChangeListeners(isEnabled);
    }

#elif APPLE_BRIGHT_SDK && UNITY_STANDALONE_OSX
    private BrdsdkBridgeMacOS sdkBridge;

    void Awake()
    {
        sdkBridge = BrdsdkBridgeMacOS.Create(null, null, null, agreeBtn, disagreeBtn, benefit);
        if (sdkBridge == null)
            return;
        sdkBridge.SetChoiceChangeCallback(choiceChanged);
    }

    public override void ExternalOptIn()
    {
        if (sdkBridge == null)
            return;
        sdkBridge.ExternalOptIn();
    }

    public override void NotifyConsentShown()
    {
        if (sdkBridge == null)
            return;
        sdkBridge.NotifyConsentShown();
    }

    public override void ShowConsent()
    {
        if (sdkBridge == null)
            return;
        sdkBridge.ShowConsent(true, emptyChoiceChanged);
    }

    public override void OptOut()
    {
        if (sdkBridge == null)
            return;
        sdkBridge.OptOut();
    }

    public override bool IsEnabled()
    {
        if (sdkBridge == null)
            return false;
        return sdkBridge.CurrentChoice == BrdsdkBridgeMacOS.Choice.Peer;
    }

    private void choiceChanged(BrdsdkBridgeMacOS.Choice choice)
    {
        bool enabled = choice == BrdsdkBridgeMacOS.Choice.Peer;
        NotifyChoiceChangeListeners(enabled);
    }

    private void emptyChoiceChanged(BrdsdkBridgeMacOS.Choice choice)
    {
    }
#endif
}