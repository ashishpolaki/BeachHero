#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using BeachHero;
using GooglePlayGames;

    /// <summary>
    /// CLI build script providing TestApk and GooglePlayBundleApk methods for automated and editor menu builds.
    /// Can be called via Unity CLI:
    ///   -executeMethod BuildScript.TestApk
    ///   -executeMethod BuildScript.GooglePlayBundleApk
    /// </summary>
    public static class BuildScript
    {
        private const string TEST_APK_BUILD_DIR = @"C:\Users\hunte\Downloads\BeachHero\BEachHeroAPK";
        private const string GOOGLE_BUNDLE_AAB_BUILD_DIR = @"C:\Users\hunte\Downloads\BeachHero\BeachHero AAB";
        private const string KEYSTORE_PASSWORD = "@Sharath1999";
        private const string TEST_ANDROID_CONFIGURATION_CLIENT_ID_OR_CERTIFICATE = "665392816801-pl09fd4dhhpumpgur5a2udtrd8fdia9n.apps.googleusercontent.com";
        private const string GOOGLE_PLAY_CONFIGURATION_CLIENT_ID = "665392816801-c88l7fhhsb79vs1505008fkqmuuk24r3.apps.googleusercontent.com";
        private const string INIT_SCENE_PATH = "Assets/Scenes/Init.unity";
        private const string CHEAT_CODE_CONSOLE_NAME = "CheatCode Console";

        [MenuItem("Beach Hero/Build/Android/Test APK", false, 1)]
        public static void TestApk()
        {
            ConfigureTestApk();
            BuildAndroid(isBundle: false);
        }

        [MenuItem("Beach Hero/Build/Android/Google Play Bundle (AAB)", false, 2)]
        public static void GooglePlayBundleApk()
        {
            ConfigureGooglePlayBundleAab();
            BuildAndroid(isBundle: true);
        }

        private static void ConfigureTestApk()
        {
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            EditorUserBuildSettings.buildAppBundle = false;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = KEYSTORE_PASSWORD;
            PlayerSettings.Android.keyaliasPass = KEYSTORE_PASSWORD;

            EnsureAndroidDefine("CHEAT_CODE");
            EnsureAndroidDefine("ENABLE_DEBUG");
            EnsureCheatCodeConsoleIsActive();

            DebugUtils.Log($"[BuildScript] Test APK settings applied. Android configuration client ID/certificate: '{TEST_ANDROID_CONFIGURATION_CLIENT_ID_OR_CERTIFICATE}'.");
        }

        private static void ConfigureGooglePlayBundleAab()
        {
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            EditorUserBuildSettings.buildAppBundle = true;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = KEYSTORE_PASSWORD;
            PlayerSettings.Android.keyaliasPass = KEYSTORE_PASSWORD;

            RemoveAndroidDefine("CHEAT_CODE");
            RemoveAndroidDefine("ENABLE_DEBUG");
            EnsureCheatCodeConsoleIsInactive();

            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.bundleVersion = IncrementPatchVersion(PlayerSettings.bundleVersion);

            DebugUtils.Log($"[BuildScript] Bundle version: {PlayerSettings.bundleVersion}; version code: {PlayerSettings.Android.bundleVersionCode}.");
        }

        private static void ConfigureGooglePlayGamesAndroid(string configurationClientId)
        {
            if (string.IsNullOrWhiteSpace(configurationClientId))
            {
                DebugUtils.LogWarning("[BuildScript] Google Play Games Android configuration skipped because the selected client ID is empty.");
                return;
            }

            PlayGamesSettings settings = PlayGamesSettings.LoadInstance();
            if (settings == null)
            {
                DebugUtils.LogError("[BuildScript] Google Play Games settings asset could not be loaded.");
                return;
            }

            Type setupType = Type.GetType("GooglePlayGames.Editor.GPGSAndroidSetupUI, Google.Play.Games.Editor");
            MethodInfo setupMethod = setupType?.GetMethod(
                "PerformSetup",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string), typeof(string) },
                null);

            if (setupMethod == null)
            {
                DebugUtils.LogError("[BuildScript] Google Play Games Android setup API could not be found.");
                return;
            }

            bool setupSucceeded;
            try
            {
                setupSucceeded = (bool)setupMethod.Invoke(null, new object[]
                {
                    configurationClientId,
                    settings.AppId,
                    settings.NearbyServiceId
                });
            }
            catch (Exception exception)
            {
                DebugUtils.LogError($"[BuildScript] Google Play Games Android configuration threw an exception: {exception}");
                return;
            }

            if (!setupSucceeded)
            {
                DebugUtils.LogError("[BuildScript] Google Play Games Android configuration failed.");
            }
            else
            {
                DebugUtils.Log("[BuildScript] Google Play Games Android configuration completed.");
            }
        }

        private static void EnsureAndroidDefine(string symbol)
        {
            NamedBuildTarget androidBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
            PlayerSettings.GetScriptingDefineSymbols(androidBuildTarget, out string[] symbols);

            if (!symbols.Contains(symbol))
            {
                ArrayUtility.Add(ref symbols, symbol);
                PlayerSettings.SetScriptingDefineSymbols(androidBuildTarget, symbols);
            }
        }

        private static void RemoveAndroidDefine(string symbol)
        {
            NamedBuildTarget androidBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
            PlayerSettings.GetScriptingDefineSymbols(androidBuildTarget, out string[] symbols);

            if (symbols.Contains(symbol))
            {
                ArrayUtility.Remove(ref symbols, symbol);
                PlayerSettings.SetScriptingDefineSymbols(androidBuildTarget, symbols);
            }
        }

        private static void EnsureCheatCodeConsoleIsActive()
        {
            if (!File.Exists(INIT_SCENE_PATH))
            {
                DebugUtils.LogWarning($"[BuildScript] Init scene not found at '{INIT_SCENE_PATH}'.");
                return;
            }

            Scene initScene = SceneManager.GetSceneByPath(INIT_SCENE_PATH);
            bool openedForBuild = !initScene.IsValid() || !initScene.isLoaded;
            if (openedForBuild)
            {
                initScene = EditorSceneManager.OpenScene(INIT_SCENE_PATH, OpenSceneMode.Additive);
            }

            GameObject cheatConsole = initScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(transform => transform.gameObject)
                .FirstOrDefault(gameObject => gameObject.name == CHEAT_CODE_CONSOLE_NAME);

            if (cheatConsole == null)
            {
                DebugUtils.LogWarning($"[BuildScript] '{CHEAT_CODE_CONSOLE_NAME}' was not found in '{INIT_SCENE_PATH}'.");
                if (openedForBuild)
                {
                    EditorSceneManager.CloseScene(initScene, true);
                }
                return;
            }

            if (!cheatConsole.activeSelf)
            {
                cheatConsole.SetActive(true);
                EditorSceneManager.MarkSceneDirty(initScene);
                EditorSceneManager.SaveScene(initScene);
                DebugUtils.Log($"[BuildScript] Enabled '{CHEAT_CODE_CONSOLE_NAME}' in '{INIT_SCENE_PATH}'.");
            }

            if (openedForBuild)
            {
                EditorSceneManager.CloseScene(initScene, true);
            }
        }

        private static void EnsureCheatCodeConsoleIsInactive()
        {
            if (!File.Exists(INIT_SCENE_PATH))
            {
                DebugUtils.LogWarning($"[BuildScript] Init scene not found at '{INIT_SCENE_PATH}'.");
                return;
            }

            Scene initScene = SceneManager.GetSceneByPath(INIT_SCENE_PATH);
            bool openedForBuild = !initScene.IsValid() || !initScene.isLoaded;
            if (openedForBuild)
            {
                initScene = EditorSceneManager.OpenScene(INIT_SCENE_PATH, OpenSceneMode.Additive);
            }

            GameObject cheatConsole = initScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(transform => transform.gameObject)
                .FirstOrDefault(gameObject => gameObject.name == CHEAT_CODE_CONSOLE_NAME);

            if (cheatConsole == null)
            {
                DebugUtils.LogWarning($"[BuildScript] '{CHEAT_CODE_CONSOLE_NAME}' was not found in '{INIT_SCENE_PATH}'.");
            }
            else if (cheatConsole.activeSelf)
            {
                cheatConsole.SetActive(false);
                EditorSceneManager.MarkSceneDirty(initScene);
                EditorSceneManager.SaveScene(initScene);
                DebugUtils.Log($"[BuildScript] Disabled '{CHEAT_CODE_CONSOLE_NAME}' in '{INIT_SCENE_PATH}'.");
            }

            if (openedForBuild)
            {
                EditorSceneManager.CloseScene(initScene, true);
            }
        }

        private static string IncrementPatchVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "0.0.1";
            }

            string[] parts = version.Split('.');
            int lastIndex = parts.Length - 1;
            if (int.TryParse(parts[lastIndex], out int patch))
            {
                parts[lastIndex] = (patch + 1).ToString();
                return string.Join(".", parts);
            }

            return $"{version}.1";
        }

        private static void BuildAndroid(bool isBundle)
        {
            string buildType = isBundle ? "Google Play Bundle (AAB)" : "Test APK";
            DebugUtils.Log($"[BuildScript] ================= Starting {buildType} Build =================");

            try
            {
                // 1. Configure Android Build Settings
                EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                EditorUserBuildSettings.buildAppBundle = isBundle;

                // 2. Configure Keystore
                ConfigureKeystore();

                // 3. Apply optional CLI version overrides
                ApplyVersionOverrides();

                // 4. Configure Google Play Games with the build-specific client ID.
                string configurationClientId = isBundle
                    ? GOOGLE_PLAY_CONFIGURATION_CLIENT_ID
                    : TEST_ANDROID_CONFIGURATION_CLIENT_ID_OR_CERTIFICATE;
                ConfigureGooglePlayGamesAndroid(configurationClientId);

                // 5. Resolve Output Path
                string extension = isBundle ? "aab" : "apk";
                string defaultFileName = $"BeachHero.{extension}";
                string buildDirectory = isBundle ? GOOGLE_BUNDLE_AAB_BUILD_DIR : TEST_APK_BUILD_DIR;
                string defaultPath = Path.Combine(buildDirectory, defaultFileName);

                string outputPath = GetArg("-buildPath") ?? GetArg("-output") ?? defaultPath;
                outputPath = outputPath.Replace('\\', '/');

                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!isBundle && File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                    DebugUtils.Log($"[BuildScript] Existing test APK overwritten: {outputPath}");
                }

                // 6. Gather Scenes
                string[] scenes = GetEnabledScenes();
                if (scenes == null || scenes.Length == 0)
                {
                    DebugUtils.LogError("[BuildScript] No enabled scenes found for build!");
                    ExitIfBatchMode(1);
                    return;
                }

                DebugUtils.Log($"[BuildScript] Target: Android | AppBundle: {isBundle}");
                DebugUtils.Log($"[BuildScript] Output Path: {outputPath}");
                DebugUtils.Log($"[BuildScript] Scenes included ({scenes.Length}):\n - " + string.Join("\n - ", scenes));

                // 7. Build Player Options
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                };

                // 8. Perform Build
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                // 9. Result Handling
                if (summary.result == BuildResult.Succeeded)
                {
                    float sizeMb = summary.totalSize / (1024f * 1024f);
                    DebugUtils.Log($"[BuildScript] Build SUCCEEDED!");
                    DebugUtils.Log($"[BuildScript] File: {summary.outputPath} ({sizeMb:F2} MB)");
                    DebugUtils.Log($"[BuildScript] Duration: {summary.totalTime.TotalSeconds:F1}s");

                    if (Application.isBatchMode)
                    {
                        ExitIfBatchMode(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(summary.outputPath);
                    }
                }
                else if (summary.result == BuildResult.Failed)
                {
                    DebugUtils.LogError($"[BuildScript] Build FAILED! Total errors: {summary.totalErrors}");
                    ExitIfBatchMode(1);
                }
                else if (summary.result == BuildResult.Cancelled)
                {
                    DebugUtils.LogWarning("[BuildScript] Build was CANCELLED.");
                    ExitIfBatchMode(2);
                }
                else
                {
                    DebugUtils.LogWarning($"[BuildScript] Build finished with status: {summary.result}");
                    ExitIfBatchMode(1);
                }
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"[BuildScript] Unexpected error during build: {ex}");
                ExitIfBatchMode(1);
            }
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrEmpty(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length > 0)
            {
                return scenes;
            }

            // Fallback to essential project scenes
            string[] fallback = { "Assets/Scenes/Init.unity", "Assets/Scenes/Game.unity" };
            return fallback.Where(File.Exists).ToArray();
        }

        private static void ConfigureKeystore()
        {
            if (HasArg("-useDebugKeystore"))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                DebugUtils.Log("[BuildScript] -useDebugKeystore passed. Using default Android debug keystore.");
                return;
            }

            string keystoreName = GetArg("-keystoreName") ?? Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_NAME");
            if (!string.IsNullOrEmpty(keystoreName))
            {
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.useCustomKeystore = true;
            }

            string keyaliasName = GetArg("-keyaliasName") ?? Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_NAME");
            if (!string.IsNullOrEmpty(keyaliasName))
            {
                PlayerSettings.Android.keyaliasName = keyaliasName;
            }

            string keystorePass = GetArg("-keystorePass")
                ?? Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS")
                ?? Environment.GetEnvironmentVariable("KEYSTORE_PASS");
            if (!string.IsNullOrEmpty(keystorePass))
            {
                PlayerSettings.Android.keystorePass = keystorePass;
            }

            string keyaliasPass = GetArg("-keyaliasPass")
                ?? Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_PASS")
                ?? Environment.GetEnvironmentVariable("KEYALIAS_PASS");
            if (!string.IsNullOrEmpty(keyaliasPass))
            {
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }

            if (PlayerSettings.Android.useCustomKeystore)
            {
                DebugUtils.Log($"[BuildScript] Keystore configured: '{PlayerSettings.Android.keystoreName}' (Alias: '{PlayerSettings.Android.keyaliasName}')");
            }
        }

        private static void ApplyVersionOverrides()
        {
            string version = GetArg("-bundleVersion") ?? GetArg("-version");
            if (!string.IsNullOrEmpty(version))
            {
                PlayerSettings.bundleVersion = version;
                DebugUtils.Log($"[BuildScript] Overrode bundleVersion to {version}");
            }

            string versionCodeStr = GetArg("-bundleVersionCode") ?? GetArg("-versionCode");
            if (int.TryParse(versionCodeStr, out int versionCode))
            {
                PlayerSettings.Android.bundleVersionCode = versionCode;
                DebugUtils.Log($"[BuildScript] Overrode bundleVersionCode to {versionCode}");
            }
        }

        private static string GetArg(string name, string defaultValue = null)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return defaultValue;
        }

        private static bool HasArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            return Array.Exists(args, arg => arg.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static void ExitIfBatchMode(int exitCode)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }
    }

#endif
