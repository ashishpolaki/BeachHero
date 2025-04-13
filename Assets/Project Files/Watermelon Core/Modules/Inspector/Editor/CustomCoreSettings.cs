using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class CustomCoreSettings
    {
        private const string DEFAULT_INIT_SCENE_NAME = "Init";

        // Define your preference values
        public static bool UseCustomInspector { get; private set; } = true;
        public static bool UseHierarchyIcons { get; private set; } = true;

        public static bool AutoLoadInitializer { get; private set; } = true;
        public static string InitSceneName { get; private set; } = DEFAULT_INIT_SCENE_NAME;

        public static Color AdsDummyBackgroundColor { get; private set; } = new Color(0.1f, 0.2f, 0.35f, 1.0f);
        public static Color AdsDummyMainColor { get; private set; } = new Color(0.15f, 0.37f, 0.6f, 1.0f);

        public static bool ShowWatermelonPromotions { get; private set; } = true;

        // Define a static constructor to load saved preferences when the script is loaded
        static CustomCoreSettings()
        {
            LoadPreferences();
        }

        // Create the SettingsProvider and register it to the Preferences window
        [SettingsProvider]
        public static SettingsProvider CustomPreferencesMenu()
        {
            // Create a new SettingsProvider with a path in the Preferences window
            var provider = new SettingsProvider("Preferences/Watermelon Core", SettingsScope.User)
            {
                // Label of the preferences page
                label = "Watermelon Core",

                // This method is called to draw the GUI
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    GUILayout.Space(12);

                    UseCustomInspector = EditorGUILayout.Toggle("Custom Inspector", UseCustomInspector);
                    UseHierarchyIcons = EditorGUILayout.Toggle("Hierarchy Icons", UseHierarchyIcons);

                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Initializer", EditorStyles.boldLabel);
                    AutoLoadInitializer = EditorGUILayout.Toggle("Auto Load Initializer", AutoLoadInitializer);
                    if(AutoLoadInitializer)
                    {
                        InitSceneName = EditorGUILayout.TextField("Initializer Scene Name", InitSceneName);
                    }

                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Monetization", EditorStyles.boldLabel);
                    AdsDummyBackgroundColor = EditorGUILayout.ColorField("Dummy Background Color", AdsDummyBackgroundColor);
                    AdsDummyMainColor = EditorGUILayout.ColorField("Dummy Main Color", AdsDummyMainColor);

                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Promotions", EditorStyles.boldLabel);
                    ShowWatermelonPromotions = EditorGUILayout.Toggle("Show Watermelon Promotions", ShowWatermelonPromotions);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    if (GUI.changed)
                    {
                        SavePreferences();
                    }
                },

                // Define the keywords to be used in the search bar
                keywords = new string[] { "Custom", "Preferences", "Watermelon Core", "Core" }
            };

            return provider;
        }

        // Save the preferences to EditorPrefs
        private static void SavePreferences()
        {
            EditorPrefs.SetBool("UseCustomInspector", UseCustomInspector);
            EditorPrefs.SetBool("UseHierarchyIcons", UseHierarchyIcons);

            EditorPrefs.SetBool("AutoLoadInitializer", AutoLoadInitializer);
            EditorPrefs.SetString("InitSceneName", InitSceneName);

            EditorPrefs.SetString("AdsDummyBackgroundColor", AdsDummyBackgroundColor.ToHex());
            EditorPrefs.SetString("AdsDummyMainColor", AdsDummyMainColor.ToHex());

            EditorPrefs.SetBool("ShowWatermelonPromotions", ShowWatermelonPromotions);

            if (!UseHierarchyIcons)
                EditorCustomHierarchy.Disable();
        }

        // Load the preferences from EditorPrefs
        private static void LoadPreferences()
        {
            UseCustomInspector = EditorPrefs.GetBool("UseCustomInspector", true);
            UseHierarchyIcons = EditorPrefs.GetBool("UseHierarchyIcons", true);

            AutoLoadInitializer = EditorPrefs.GetBool("AutoLoadInitializer", true);
            InitSceneName = EditorPrefs.GetString("InitSceneName", DEFAULT_INIT_SCENE_NAME);

            if(ColorUtility.TryParseHtmlString(EditorPrefs.GetString("AdsDummyBackgroundColor"), out Color backgroundColor))
            {
                AdsDummyBackgroundColor = backgroundColor;
            }

            if (ColorUtility.TryParseHtmlString(EditorPrefs.GetString("AdsDummyMainColor"), out Color mainColor))
            {
                AdsDummyMainColor = mainColor;
            }

            ShowWatermelonPromotions = EditorPrefs.GetBool("ShowWatermelonPromotions", true);
        }
    }
}