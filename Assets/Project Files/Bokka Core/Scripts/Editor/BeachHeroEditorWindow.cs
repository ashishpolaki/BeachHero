using Bokka.BeachRescue;
using UnityEditor;
using UnityEngine;

public class BeachHeroEditorWindow : EditorWindow
{
    private static BeachHeroEditorWindow instance;
    private static EditorWindow window;

    #region Editor Window 
    private static readonly int DEFAULT_WINDOW_MIN_SIZE = 600;
    private string assetPath = "Assets/Project Files/Data/Level System/Levels Database.asset";
    #endregion

    #region Tabs
    private SerializedObject levelsDatabaseSerializedObject;
    private int selectedTab = 0;
    private string[] tabTitles = { "Levels", "Items" };
    private GUIStyle tabStyle;
    private GUIStyle boxStyle;
    #endregion

    #region LevelsTab
    private float levelsTab_DragAreaWidth = 5f; // Width of the drag area
    private const string LEVELS_PROPERTY_NAME = "levelsList";
    private SerializedProperty levelsList_SerializedProperty;
    private int selectedLevelIndex = 0;
    private float levelsTab_LeftPanelWidth = 240f;
    private bool levelsTab_isResizing = false;
    private Vector2 levelsTab_LeftPanelscrollPos;
    #endregion

    #region ItemsTab
    private SerializedProperty items_SerializedProperty;
    private const string ITEMS_PROPERTY_NAME = "items";
    #endregion


    [MenuItem("Tools/BeachHero LevelEditor Window")]
    private static void ShowWindow()
    {
        window = EditorWindow.GetWindow<BeachHeroEditorWindow>("Beach Hero Level Window");
        window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
        window.Show();
    }

    #region Unity methods
    protected virtual void OnEnable()
    {
        instance = this;
        LevelsDatabase data = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(assetPath);

        if (data != null)
        {
            levelsDatabaseSerializedObject = new SerializedObject(data);
            ReadLevelDatabaseFields();
        }
    }
    private void OnGUI()
    {
        DrawTabs();
    }
    #endregion

    #region Tab Methods
    private void DrawTabs()
    {
        // Initialize styles
        tabStyle = new GUIStyle(EditorStyles.miniButtonMid); // Better than toolbarButton for height control
        tabStyle.fontStyle = FontStyle.Bold;
        tabStyle.fontSize = 14;
        tabStyle.fixedHeight = 30;
        tabStyle.alignment = TextAnchor.MiddleCenter;
        tabStyle.margin = new RectOffset(2, 2, 5, 5);

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.margin = new RectOffset(10, 10, 10, 10);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);

        GUILayout.Space(10);

        // Styled horizontal toolbar
        EditorGUILayout.BeginHorizontal();
        GUI.changed = false;
        selectedTab = GUILayout.Toolbar(selectedTab, tabTitles, tabStyle);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Content box with padding
        EditorGUILayout.BeginVertical(boxStyle);

        switch (selectedTab)
        {
            case 0:
                DrawLevelsTab();
                break;
            case 1:
                DrawItemsTab();
                break;
        }

        EditorGUILayout.EndVertical();
    }
    private void ReadLevelDatabaseFields()
    {
        levelsList_SerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
        items_SerializedProperty = levelsDatabaseSerializedObject.FindProperty(ITEMS_PROPERTY_NAME);
    }
    #endregion

    #region Level Tab
    private void DrawLevelsTab()
    {
        GUILayout.BeginHorizontal();

        #region Left Panel
        // LEFT PANEL
        GUILayout.BeginVertical(GUILayout.Width(levelsTab_LeftPanelWidth));
        GUILayout.Label("Levels", EditorStyles.boldLabel);

        levelsTab_LeftPanelscrollPos = GUILayout.BeginScrollView(levelsTab_LeftPanelscrollPos, false, true);
        for (int i = 1; i <= levelsList_SerializedProperty.arraySize; i++)
        {
            // Define the button style
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.fontSize = 25;
            buttonStyle.fixedHeight = 30;

            // Highlight the selected button
            if (i - 1 == selectedLevelIndex)
            {
                buttonStyle.normal.textColor = Color.green;  // Change text color (or change background if you prefer)
                buttonStyle.normal.background = MakeTex(2, 2, Color.blue); // Set a gray background (as an example)
            }
            else
            {
                // buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                buttonStyle.normal.textColor = Color.white;
            }

            // Show the button
            if (GUILayout.Button("Level " + i, buttonStyle))
            {
                selectedLevelIndex = i - 1;  // Update selected index (0-based)
                Debug.Log("Clicked Level " + i);
            }
            GUILayout.Space(5); // Add some space between buttons
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Calculate drag handler area height to match left panel height
        Rect lastRect = GUILayoutUtility.GetLastRect();
        float dragHeight = lastRect.yMax; // End of the left panel
        Rect dragArea = new Rect(lastRect.xMax, lastRect.yMin, levelsTab_DragAreaWidth, dragHeight - lastRect.yMin);
        EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.ResizeHorizontal);
        EditorGUI.DrawRect(dragArea, Color.black); // Dark gray (Optional)

        // Handle drag resizing
        if (Event.current.type == EventType.MouseDown && dragArea.Contains(Event.current.mousePosition))
        {
            levelsTab_isResizing = true;
            Event.current.Use(); // Use event only if clicked inside drag area
        }

        if (levelsTab_isResizing)
        {
            if (Event.current.type == EventType.MouseDrag)
            {
                levelsTab_LeftPanelWidth = Mathf.Clamp(Event.current.mousePosition.x, 100f, position.width - 100f);
                Repaint();
                Event.current.Use(); // Only use when resizing
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                levelsTab_isResizing = false;
                Event.current.Use(); // End resizing and consume event
            }
        }
        #endregion

        #region Right Panel
        //// RIGHT PANEL
        GUILayout.BeginVertical("box");
        float previewSize = 100f;
        float spacing = 10f;
        float rightPanelContentWidth = EditorGUIUtility.currentViewWidth - levelsTab_LeftPanelWidth - levelsTab_DragAreaWidth; // Account for padding
        int itemsPerRow = Mathf.Max(1, Mathf.FloorToInt((rightPanelContentWidth + spacing) / (previewSize + spacing)));

        int count = items_SerializedProperty.arraySize;

        for (int i = 0; i < count; i += itemsPerRow)
        {
            GUILayout.BeginHorizontal();

            for (int j = 0; j < itemsPerRow && (i + j) < count; j++)
            {
                var property = items_SerializedProperty.GetArrayElementAtIndex(i + j);
                SerializedProperty prefabProp = property.FindPropertyRelative("prefab");
                UnityEngine.Object prefab = prefabProp.objectReferenceValue;

                if (prefab == null) continue;

                Texture2D previewTex = AssetPreview.GetAssetPreview(prefab);
                if (previewTex == null)
                    previewTex = AssetPreview.GetMiniThumbnail(prefab);

                GUIContent content = new GUIContent(previewTex);

                GUILayout.BeginVertical(GUILayout.Width(previewSize));
                if (GUILayout.Button(content, GUILayout.Width(previewSize), GUILayout.Height(previewSize)))
                {
                    Debug.Log($"Clicked: {prefab.name}");
                    Selection.activeObject = prefab;
                }

                GUILayout.Label(prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        #endregion

        GUILayout.EndHorizontal();
    }
    // Helper method to create a texture for the button background
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    #endregion

    #region Item Tab
    private void DrawItemsTab()
    {

    }
    #endregion
}
