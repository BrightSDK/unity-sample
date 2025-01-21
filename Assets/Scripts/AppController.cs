using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppController : MonoBehaviour
{
    AndroidJavaObject brightApi;
    AndroidJavaObject currentActivity;

    public GameObject errorScreen;
    public GameObject homeScreen;
    public GameObject settingsScreen;
    public GameObject statusWidget;
    public GameObject platformLabel;
    public Toggle statusToggle;
    private bool isProgrammaticChange = false;

    public GameObject premiumButton;

    private TextMeshProUGUI platformText;
    private TextMeshProUGUI statusText;

    public ChoiceListener choiceListener;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        statusText = statusWidget.GetComponent<TextMeshProUGUI>();
        platformText = platformLabel.GetComponent<TextMeshProUGUI>();

        // Register the global exception handler
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        toggleScreen(homeScreen);
        initBrightSdk();
        Debug.Log($"Setting platform text: {Application.platform.ToString()}");
        platformText.text = $"Platform: {Application.platform.ToString()}";
        RequestAndUpdateStatus();
        statusToggle.onValueChanged.AddListener(OnSettingsToggleChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void toggleScreen(GameObject screen)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            foreach (GameObject s in new List<GameObject> {
                errorScreen,
                homeScreen,
                settingsScreen,
            })
            {
                s.SetActive(false);
            }
            screen.SetActive(true);
        });
    }

    private void initBrightSdk()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        brightApi = new AndroidJavaObject("com.android.eapx.BrightApi");

        AndroidJavaObject settings = new AndroidJavaObject("com.android.eapx.Settings", currentActivity);
        settings.Call("setBenefit", "To unlock premium features");
        settings.Call("setAgreeBtn", "Yes, sure!");
        settings.Call("setDisagreeBtn", "No, thanks!");
        // settings.Call("setSkipConsent", true);
        choiceListener = new ChoiceListener(this);
        settings.Call("setOnStatusChange", choiceListener);
        brightApi.CallStatic("init", currentActivity, settings);

    }

    public void showBrightSdkConsent()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        brightApi.CallStatic("showConsent", currentActivity);
    }

    private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        toggleScreen(errorScreen);
        UnityMainThreadDispatcher.Enqueue(() => {
            Exception exception = (Exception)e.ExceptionObject;
            Debug.LogError($"Unhandled exception: {exception.Message}");
            TextMeshProUGUI text = errorScreen.GetComponent<TextMeshProUGUI>();
            Debug.Log($"Setting error text: {exception.Message}");
            text.text = $"Error: {exception.Message}";
        });
    }

    public void RequestAndUpdateStatus()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        int choice = brightApi.CallStatic<int>("getChoice", currentActivity);
        UpdateStatus(choice);
    }

    public void UpdateStatus(int choice)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            Debug.Log($"Bright SDK consent choice: {choice}");
            string status = choice == 1 ? "Status: Premium" : "Status: Free";
            Debug.Log($"Setting statusText: {status}");
            statusText.text = status;
            Debug.Log($"Updated statusText: {statusText.text}");
            Debug.Log($"Updating statusToggle: {statusToggle.isOn}");
            isProgrammaticChange = true;
            statusToggle.isOn = choice == 1;
            isProgrammaticChange = false;
            Debug.Log($"Updating premiumButton: {premiumButton.activeSelf}");
            premiumButton.SetActive(choice != 1);
        });
    }

    private void OnSettingsToggleChanged(bool isOn)
    {
        if (isProgrammaticChange)
        {
            Debug.Log("Toggle change ignored (programmatic)");
            return;
        }
        Debug.Log($"Settings Toggle changed: {isOn}");
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        if (isOn)
        {
            // Call some function when the toggle is checked
            showBrightSdkConsent();
        }
        else
        {
            brightApi.CallStatic("optOut", currentActivity);
        }
    }

    public class ChoiceListener : AndroidJavaProxy
    {
        private AppController parent;

        public ChoiceListener(AppController parent) : base("com.android.eapx.Settings$OnStatusChange")
        {
            this.parent = parent;
        }

        public void onChange(int choice)
        {
            Debug.Log($"Bright SDK consent choice changed: {choice}");
            parent.UpdateStatus(choice);
        }
    }
}