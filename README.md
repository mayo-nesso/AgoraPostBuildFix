# Unity Agora XCode Build Fix

## Context

This is a simple project aimed at automating the fix for the error that occurs in Xcode when attempting to build a project exported from Unity for Mac/OSX that includes AgoraRTCKit.
The error in question is: `Command CodeSign failed with a nonzero exit code`, and according to the documentation, it occurs because when building in Unity for Mac/OSX, the symbolic link structure in the Agora framework breaks.

From Agora `Tools/prep_codesign.sh` scripts:
    `When an Agora Unity Project gets built, the framework library symbolic link structure gets lost...`

## Preliminary Solution

Agora provides us with two scripts in the `Tools` folder: `prep_codesign.sh` and `signcode.sh`.
These scripts accept the location of the application exported from Xcode as a parameter. That is the directory where our `Application.app` is located.

The first script restores the symlink structure within the AgoraRTCKit framework. Additionally, this script creates an entitlement file named `App.entitlements`, which will be used in the next script.
The second script signs the plugin's structure.

Currently, there are two problems with this approach. Firstly, as of February 2024, Xcode 15.2 fails to build, meaning the files won't find `Application.app` and will fail. Secondly, these steps can be tedious if frequent build creations are necessary or if we want to minimize human involvement in this extra step.

## Proposed Solution

For the scripts to work, the path where the Agora framework is located within the exported folder must be fixed. Then, the two scripts must be executed consecutively.

To achieve this, we can utilize the tools provided by Unity, specifically `IPostprocessBuildWithReport`, to execute these steps every time a build is exported to the Mac platform.

This results in primarily two elements:

- `AgoraPostBuildFix/Editor/AgoraPostBuildConfiguration.cs`: A script that performs the actions of a. modifying the path in the scripts if necessary and b. invoking `prep_codesign.sh` and then `signcode.sh`.
- `PostBuildConfiguration.asset`: A Scriptable Object containing the necessary configuration for the correct execution of the fix. This includes a. our *Apple Developer Identity* (in the form: `my apple developer identity (1A23BC4DE5)`) and b. the path to the `Tools` folder within the Agora plugin folder.
