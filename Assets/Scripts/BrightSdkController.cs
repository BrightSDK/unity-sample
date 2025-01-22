using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrightSdkController : MonoBehaviour
{


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

    private BrightSdkHelper brightSdkHelper;

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
        Debug.Log($"Setting platform text: {Application.platform.ToString()}");
        platformText.text = $"Platform: {Application.platform.ToString()}";
        if (Application.platform == RuntimePlatform.Android)
        {
            brightSdkHelper = new BrightSdkHelper(this);
        }
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

    public void showBrightSdkConsent()
    {
        if (brightSdkHelper != null)
        {
            brightSdkHelper.ShowConsent();
        }
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
        if (brightSdkHelper != null)
        {
            bool isEnabled = brightSdkHelper.IsEnabled();
            UpdateStatus(isEnabled);
        }
    }

    public void UpdateStatus(bool isEnabled)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            Debug.Log($"Bright SDK enabled: {isEnabled}");
            string status = isEnabled ? "Status: Premium" : "Status: Free";
            Debug.Log($"Setting statusText: {status}");
            statusText.text = status;
            Debug.Log($"Updated statusText: {statusText.text}");
            Debug.Log($"Updating statusToggle: {statusToggle.isOn}");
            isProgrammaticChange = true;
            statusToggle.isOn = isEnabled;
            isProgrammaticChange = false;
            Debug.Log($"Updating premiumButton: {premiumButton.activeSelf}");
            premiumButton.SetActive(!isEnabled);
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
        if (brightSdkHelper == null)
        {
            return;
        }
        if (isOn)
        {
            brightSdkHelper.ShowConsent();
        }
        else
        {
            brightSdkHelper.OptOut();
        }
    }

}