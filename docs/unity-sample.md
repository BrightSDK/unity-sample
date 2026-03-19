# unity-sample – Full Documentation

## Overview

unity-sample is a Unity sample application that demonstrates Bright SDK Android integration inside a Unity scene and automates Bright SDK AAR refresh before Android builds.

Repository: https://github.com/BrightSDK/unity-sample
Primary branch in this checkout: 2021.3
Unity editor version in project settings: 2021.3.45f2

---

## Project Purpose

The sample provides:

- A scene-level UX for free/premium consent state
- A Bright SDK Android helper wrapper in C#
- A prebuild processor that downloads and injects latest Android AAR
- UI controls to show consent and opt out

This repository is Android-focused (no platform-specific runtime wrappers for iOS/macOS/Windows in sample scripts).

---

## Build-Time Bright SDK Update

Source: Assets/Editor/BrightSDK/BrightSdkPreBuildProcessor.cs

Implements `IPreprocessBuildWithReport` and runs only for Android builds.

Pipeline:

1. Detect target platform in `OnPreprocessBuild`
2. Fetch versions JSON from:
   - https://bright-sdk.com/sdk_api/sdk/versions
3. Cache versions in:
   - Library/BrightSdkCache/sdk_versions.json
4. Resolve Android version (`latest` -> remote `android` field)
5. Download archive from:
   - https://cdn.bright-sdk.com/static/bright_sdk_android-{version}.tar.gz
6. Remove obsolete `bright_sdk*.aar` from:
   - Assets/Plugins/Android
7. Extract `.tar.gz` (GZip + Tar via Unity SharpZipLib)
8. Locate `.aar` recursively and copy to:
   - Assets/Plugins/Android/bright_sdk-{version}.aar

Key notes:

- Cache invalidated after one day.
- The extraction stage logs full extracted directory contents for debugging.
- Prebuild is no-op for non-Android targets.

---

## Runtime Bright SDK Wrapper

Source: Assets/Scripts/BrightSDK/BrightSdkHelper.cs

`BrightSdkHelper` is a MonoBehaviour that bridges Unity to Android Java APIs:

- Uses `com.android.eapx.BrightApi`
- Builds and configures `com.android.eapx.Settings`
- Registers `Settings$OnStatusChange` callback through AndroidJavaProxy

Configurable fields exposed in Inspector:

- benefit (default: To unlock premium features)
- agreeBtn (default: Yes, sure!)
- disagreeBtn (default: No, thanks!)
- skipConsent (default: false)

Public methods:

- ShowConsent()
- OptOut()
- IsEnabled() -> maps consent choice to enabled state

Consent mapping:

- choice == 1 is treated as enabled/premium

Callback flow:

- Java onChange(int choice) -> C# ChoiceListener -> onStatusChangeCallback (UnityEvent<bool>)

---

## Sample App Controller

Source: Assets/Scripts/SampleController.cs

Responsibilities:

- Detects Android platform and attaches BrightSdkHelper usage
- Controls three screens:
  - homeScreen
  - settingsScreen
  - errorScreen
- Shows current status:
  - Premium (enabled)
  - Free (disabled)
- Syncs a settings toggle with Bright SDK state
- Shows consent when user toggles ON
- Calls opt-out when user toggles OFF
- Handles global unhandled exceptions and routes to error screen

UI behavior details:

- Uses `UnityMainThreadDispatcher.Enqueue(...)` for thread-safe UI updates
- Maintains `isProgrammaticChange` guard to avoid toggle feedback loops
- Activates `premiumButton` only when user is not enabled

---

## Main-Thread Dispatcher

Source: Assets/Scripts/UnityMainThreadDispatcher.cs

Simple static action queue:

- `Enqueue(Action)` adds work to queue
- `Update()` on dispatcher object executes queued actions on Unity main thread

Used by `SampleController` for safe UI mutations from callback paths.

---

## Android Gradle Template

Source: Assets/Plugins/Android/mainTemplate.gradle

Highlights:

- android library plugin template for Unity export
- Includes Bright SDK support dependencies used by sample integration:
  - androidx.appcompat:1.1.0
  - androidx.core:1.9.0
  - androidx.constraintlayout:2.1.4
  - androidx.multidex:2.0.1

Also present in Assets/Plugins/Android:

- release.keystore
- gradleTemplate.properties
- bundled bright_sdk-1.549.794.aar (sample artifact)

Prebuild processor can replace/update this AAR on Android builds.

---

## Installation Script

Source: install_bright_sdk.sh

Bootstraps latest unity-plugin scripts directly from BrightSDK/unity-plugin main:

1. Downloads and runs install_dependencies.sh
2. Downloads and runs install.sh
3. Removes temporary installer script after each step

This script is a shortcut to sync sample integration logic with the standalone unity-plugin repository.

---

## Unity Packages

From Packages/manifest.json:

- com.unity.sharp-zip-lib: 1.3.9 (required by prebuild tar/gzip extraction)
- TextMeshPro and standard Unity modules

The sharp-zip-lib dependency is critical for archive extraction in `BrightSdkPreBuildProcessor`.

---

## Scene and UX Flow

Main scene: Assets/Scenes/SampleScene.unity

Typical flow:

1. App starts in landscape.
2. Status is queried from Bright SDK (`IsEnabled`).
3. UI displays Free/Premium and toggle state.
4. User can open consent from button/toggle.
5. Choice callback updates UI state through UnityEvent and dispatcher.

---

## Limitations

- Integration code is Android-only in this sample.
- Prebuild updater executes only for Android target.
- No dedicated test suite in repository for sample logic.

---

## Source Map

Key files:

- Assets/Editor/BrightSDK/BrightSdkPreBuildProcessor.cs
- Assets/Scripts/BrightSDK/BrightSdkHelper.cs
- Assets/Scripts/SampleController.cs
- Assets/Scripts/UnityMainThreadDispatcher.cs
- Assets/Plugins/Android/mainTemplate.gradle
- install_bright_sdk.sh
- Packages/manifest.json
- ProjectSettings/ProjectVersion.txt
