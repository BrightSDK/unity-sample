# Bright SDK Unity Sample

A sample Unity project demonstrating integration with the Bright SDK for Android, iOS, Windows, and other platforms.

## Overview

This project showcases how to integrate and use the Bright SDK in a Unity application. It includes a simple UI with consent management, settings, and SDK status monitoring across multiple platforms.

## Features

- **Multi-platform support**: Android, iOS, tvOS, macOS, Windows
- **Consent management**: Show and manage user consent dialogs
- **Settings interface**: Configure SDK behavior
- **Status monitoring**: Real-time SDK status display
- **Error handling**: Global exception handling with user-friendly error screens

## Prerequisites

- Unity 2021.3 or later (recommended)
- For Android builds: Android SDK and NDK
- For iOS/macOS builds: Xcode
- For Windows builds: Visual Studio

## Installation

1. Clone this repository:
   ```bash
   git clone git@github.com:BrightSDK/unity-sample.git
   cd unity-sample
   ```

2. Install the Bright SDK:
   ```bash
   ./install_bright_sdk.sh
   ```

3. Open the project in Unity:
   - Launch Unity Hub
   - Click "Add" and select the cloned project directory
   - Open the project

## Project Structure

```
Assets/
├── Editor/              # Editor scripts
├── Plugins/             # Native plugins for different platforms
├── Scenes/              # Unity scenes
│   └── SampleScene.unity
├── Scripts/
│   ├── BrightSDK/       # Bright SDK integration scripts
│   ├── BrightSdkController.cs    # Main SDK controller
│   └── UnityMainThreadDispatcher.cs
└── TextMesh Pro/        # UI text rendering
```

## Usage

### Running the Sample

1. Open `Assets/Scenes/SampleScene.unity`
2. Click Play in Unity Editor or build for your target platform
3. The app displays:
   - **Home Screen**: Main interface with platform information
   - **Settings Screen**: SDK configuration options
   - **Status Widget**: Real-time SDK status

### Key Components

#### BrightSdkController

The main controller managing SDK interactions:

```csharp
// Show consent dialog
public void showBrightSdkConsent()

// Toggle between screens
public void toggleScreen(GameObject screen)

// Update SDK status
private void RequestAndUpdateStatus()
```

#### Platform-Specific Helpers

- `AndroidBrightSDKHelper`: Android implementation
- `AppleBrightSDKHelper`: iOS/macOS/tvOS implementation
- `WinBrightSDKHelper`: Windows implementation

## Building for Different Platforms

### Android

1. File → Build Settings → Android
2. Click "Switch Platform"
3. Configure Player Settings (package name, version, etc.)
4. Click "Build" or "Build and Run"

### iOS

1. File → Build Settings → iOS
2. Click "Switch Platform"
3. Configure Player Settings (bundle identifier, version, etc.)
4. Click "Build"
5. Open the generated Xcode project and build

### Windows

1. File → Build Settings → Windows
2. Click "Switch Platform"
3. Click "Build" or "Build and Run"

## Configuration

The Bright SDK can be configured through the settings screen in the app. Key settings include:

- SDK enable/disable toggle
- Status monitoring
- Platform-specific configurations

## Troubleshooting

### SDK Not Loading

- Ensure `install_bright_sdk.sh` ran successfully
- Check Unity Console for error messages
- Verify platform-specific plugins are present in `Assets/Plugins/`

### Build Errors

- Ensure all required SDKs are installed for your target platform
- Check minimum API levels for Android
- Verify signing configurations for iOS/Android

## API Documentation

For detailed API documentation, visit the [Bright SDK Documentation](https://brightsdk.github.io/).

## Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/BrightSDK/unity-sample/issues)
<!-- - **Documentation**: [Bright SDK Docs](https://brightsdk.github.io/) -->
- **Contact**: support@brightsdk.com

## License

This sample project is provided as-is for demonstration purposes. Check the Bright SDK license for SDK usage terms.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for improvements.
