# Tracked Keyboard text input sample

This sample showcases the tracked keyboard feature, providing a comprehensive panel for text input. In addition, the panel offers 2D and 3D visualization options for the keyboard. It also includes toggles for desk and passthrough modes, as well as tracking status. With these features, this sample provides a solid foundation for building mixed reality applications with tracked keyboards.

## Licenses
The [MIT LICENSE](./LICENSE.txt) applies to the majority of the project, however files from [Text Mesh Pro](https://unity.com/legal/licenses/unity-companion-license) are licensed under their respective licensing terms.

## Prerequisites
* **Unity 2022.3+ LTS**.
* **Pair bluetooth keyboard** : On your Quest device, go to **Settings** > **Devices** > **Keyboard**. Enable keyboard tracking and pair your keyboard.

## Getting started

1. Clone this repo using either the green "**Code**" button above or this command in the terminal.
    ```sh
    git clone https://github.com/oculus-samples/Unity-TrackedKeyboard.git
    ```

    The repo is cloned to your computer. This may take a couple of minutes depending on your internet speed.

2. In Unity Hub, click **Add** > **Add Project from Disk** and select the **Unity-TrackedKeyboard** folder on your machine.
3. Open the project in **Unity 2022.3+ LTS**.
4. Open **Window** > **Package Manager** and locate the **Meta Tracked Keyboard package** (com.meta.xr.trackedkeyboardsample).
5. Open the **Samples** tab and click **Import** Samples. Then, open the **scene** located at **Assets/Samples/Meta XR Tracked Keyboard/[version]/TrackedKeyboard.unity.**
6. (**Optional**) If keyboard tracking is disabled, click the '**Connect**' button on the bottom of the panel to enable it directly from the application.
7. You should now see a white outline around your tracked keyboard, and it should become visible when you bring your hands close to it.

## Known Limitations

* Bad lighting conditions may affect keyboard tracking.
* Desk height is estimated based on the tracked keyboard's position and may not be accurate for all keyboard types.
