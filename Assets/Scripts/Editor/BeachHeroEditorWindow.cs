#if UNITY_EDITOR
using BeachHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BeachHeroEditorWindow : EditorWindow
{
    private static BeachHeroEditorWindow instance;
    private static EditorWindow window;

    #region Property Representation
    public class LevelDatabaseRepresentation
    {
        public SerializedObject serializedLevelDatabaseObject;
        public SerializedProperty levelsListProperty;
        public SerializedProperty spawnItemRepresentation;

        public LevelDatabaseRepresentation(Object levelDatabaseObject)
        {
            serializedLevelDatabaseObject = new SerializedObject(levelDatabaseObject);
            ReadFields();
        }

        private void ReadFields()
        {
            levelsListProperty = serializedLevelDatabaseObject.FindProperty("levelsList");
            spawnItemRepresentation = serializedLevelDatabaseObject.FindProperty("spawnItemsList");
        }
    }

    public class LevelRepresentation
    {
        public SerializedObject serializedLevelObject;
        public SerializedProperty levelTimeProperty;
        public SerializedProperty boatSpeedProperty;
        public SerializedProperty startPointProperty;
        public SerializedProperty moveObstaclesProperty;
        public SerializedProperty staticObstaclesProperty;
        public SerializedProperty whirlpoolObstaclesProperty;
        public SerializedProperty savedCharactersProperty;
        public SerializedProperty collectablesProperty;
        public SerializedProperty medalRequirementsProperty;

        public LevelRepresentation(Object levelObject)
        {
            serializedLevelObject = new SerializedObject(levelObject);
            ReadFields();
        }

        public void ApplyModifiedProperties()
        {
            serializedLevelObject.ApplyModifiedProperties();
        }

        private void ReadFields()
        {
            levelTimeProperty = serializedLevelObject.FindProperty("levelTime");
            boatSpeedProperty = serializedLevelObject.FindProperty("BoatSpeed");
            startPointProperty = serializedLevelObject.FindProperty("startPoint");
            moveObstaclesProperty = serializedLevelObject.FindProperty("obstacles").FindPropertyRelative("movingObstacles");
            staticObstaclesProperty = serializedLevelObject.FindProperty("obstacles").FindPropertyRelative("staticObstacles");
            whirlpoolObstaclesProperty = serializedLevelObject.FindProperty("obstacles").FindPropertyRelative("whirlpoolObstacles");
            savedCharactersProperty = serializedLevelObject.FindProperty("drownCharacters");
            collectablesProperty = serializedLevelObject.FindProperty("collectables");
            medalRequirementsProperty = serializedLevelObject.FindProperty("medalRequirements");
        }
    }

    public class MoveObstacleRepresentation
    {
        public class KeyFrameRepresentation
        {
            public SerializedProperty positionProperty;
            public SerializedProperty inTangentLocalProperty;
            public SerializedProperty outTangentLocalProperty;

            public KeyFrameRepresentation(SerializedProperty keyframeProperty)
            {
                positionProperty = keyframeProperty.FindPropertyRelative("position");
                inTangentLocalProperty = keyframeProperty.FindPropertyRelative("inTangentLocal");
                outTangentLocalProperty = keyframeProperty.FindPropertyRelative("outTangentLocal");
            }
        }

        public SerializedProperty obstacleTypeProperty;
        public SerializedProperty keyFrameProperty;
        public SerializedProperty resolutionProperty;
        public SerializedProperty movementSpeedProperty;
        public SerializedProperty rotationSpeedProperty;
        public SerializedProperty isLoopMovementProperty;
        public SerializedProperty isInverseDirectionProperty;

        public MoveObstacleRepresentation(SerializedProperty movingObstacleProperty)
        {
            obstacleTypeProperty = movingObstacleProperty.FindPropertyRelative("type");
            keyFrameProperty = movingObstacleProperty.FindPropertyRelative("bezierKeyframes");
            resolutionProperty = movingObstacleProperty.FindPropertyRelative("resolution");
            movementSpeedProperty = movingObstacleProperty.FindPropertyRelative("movementSpeed");
            rotationSpeedProperty = movingObstacleProperty.FindPropertyRelative("rotationSpeedMultiplier");
            isLoopMovementProperty = movingObstacleProperty.FindPropertyRelative("loopedMovement");
            isInverseDirectionProperty = movingObstacleProperty.FindPropertyRelative("inverseDirection");
        }
    }

    public class StaticObstacleRepresentation
    {
        public SerializedProperty typeProperty;
        public SerializedProperty positionProperty;
        public SerializedProperty rotationProperty;
        public SerializedProperty scaleProperty;

        public StaticObstacleRepresentation(SerializedProperty staticObstacleProperty)
        {
            typeProperty = staticObstacleProperty.FindPropertyRelative("type");
            positionProperty = staticObstacleProperty.FindPropertyRelative("position");
            rotationProperty = staticObstacleProperty.FindPropertyRelative("rotation");
            scaleProperty = staticObstacleProperty.FindPropertyRelative("scale");
        }
    }

    public class SavedCharacterRepresentation
    {
        public SerializedProperty positionProperty;
        public SerializedProperty waitTimePercentageProperty;

        public SavedCharacterRepresentation(SerializedProperty savedCharacterProperty)
        {
            positionProperty = savedCharacterProperty.FindPropertyRelative("position");
            waitTimePercentageProperty = savedCharacterProperty.FindPropertyRelative("waitTimePercentage");
        }
    }
    #endregion

    #region Window Variables
    private static readonly int DEFAULT_WINDOW_MIN_SIZE = 600;
    private string ASSETPATH = "Assets/ScriptableObjects/Levels/LevelsDatabase.asset";
    private string EDITOR_SCENE_NAME = "GameEditorScene";
    private string EDITOR_SCENE_PATH = "Assets/Scenes/GameEditorScene.unity";
    //  private string GAME_SCENE_PATH = "Assets/Scenes/Game.unity";
    private string INIT_SCENE_PATH = "Assets/Scenes/Init.unity";
    private string TEST_SCENE_PATH = "Assets/Scenes/Test.unity";
    private string FILE_STRING = "file :";
    private string LEVELS_PATH = "Assets/ScriptableObjects/Levels/Level_";
    private int selectedTab = 0;
    private string[] tabTitles = { "Levels", "Items" };
    private GUIStyle tabStyle;
    private GUIStyle boxStyle;
    #endregion

    #region Serialize Fields
    private LevelDatabaseRepresentation levelDatabaseRepresentation;
    private LevelRepresentation levelRepresentation;
    #endregion

    #region LevelsTab
    private int previousSelectedLevelIndex = -1;
    private int selectedLevelIndex = 0;
    private float levelsTab_DragAreaWidth = 5f; // Width of the drag area
    private float levelsTab_LeftPanelWidth = 240f;
    private bool levelsTab_isResizing = false;
    private Vector2 levelsTab_LeftPanelscrollPos;
    private Vector2 levelsTab_rightPanelScrollPos;
    #endregion

    #region ItemsTab
    private const string ITEMS_PROPERTY_NAME = "items";
    #endregion

    [MenuItem("Beach Hero/Level Editor Window")]
    private static void ShowWindow()
    {
        window = EditorWindow.GetWindow<BeachHeroEditorWindow>("Beach Hero Level Window");
        window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
        window.Show();
    }

    #region Unity methods
    void OnEnable()
    {
        instance = this;
        LevelDatabaseSO data = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(ASSETPATH);
        if (data != null)
        {
            levelDatabaseRepresentation = new LevelDatabaseRepresentation(data);
        }
        selectedLevelIndex = 0;
        previousSelectedLevelIndex = -1;
    }

    private void OnDestroy()
    {
        if (EditorSceneController.Instance)
        {
            EditorSceneController.Instance.Clear();
        }
    }

    private void OnGUI()
    {
        DrawContent();
    }
    #endregion

    private void DrawContent()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
        {
            DrawOpenEditorScene();
            return;
        }
        DrawTabs();
    }

    private void DrawOpenEditorScene()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

        if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
        {
            EditorSceneManager.OpenScene(EDITOR_SCENE_PATH);
        }

        EditorGUILayout.EndVertical();
    }

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
    #endregion

    #region Level Tab
    private void DrawLevelsTab()
    {
        GUILayout.BeginHorizontal();

        #region Left Panel
        float bottomHeight = 170f; // fixed
        float topHeight = position.height - bottomHeight;

        // LEFT PANEL
        GUILayout.BeginVertical(GUILayout.Width(levelsTab_LeftPanelWidth));

        //Top Section
        GUILayout.BeginVertical(GUILayout.Height(topHeight));
        GUILayout.Label("Levels", EditorStyles.boldLabel);

        levelsTab_LeftPanelscrollPos = GUILayout.BeginScrollView(levelsTab_LeftPanelscrollPos, false, true);
        for (int i = 1; i <= levelDatabaseRepresentation.levelsListProperty.arraySize; i++)
        {
            // Define the button style
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.fontSize = 25;
            buttonStyle.fixedHeight = 30;

            // Highlight the selected button
            if (i - 1 == selectedLevelIndex)
            {
                buttonStyle.normal.textColor = Color.green; // Change text color (or change background if you prefer)
                buttonStyle.normal.background = MakeTex(2, 2, Color.blue); // Set a gray background (as an example)

                if (previousSelectedLevelIndex != selectedLevelIndex)
                {
                    OpenLevel();
                    previousSelectedLevelIndex = selectedLevelIndex;
                }
            }
            else
            {
                // buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                buttonStyle.normal.textColor = Color.white;
            }

            // Show the button
            if (GUILayout.Button("Level " + i, buttonStyle))
            {
                selectedLevelIndex = i - 1;
            }
            GUILayout.Space(5); // Add some space between buttons
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        //End of top section


        //Bottom Section
        GUILayout.BeginVertical("box", GUILayout.Height(bottomHeight));

        if (GUILayout.Button("Add New Level"))
        {
            levelDatabaseRepresentation.levelsListProperty.arraySize++;
            //create a new scriptable object asset with previous level data
            LevelSO level = levelDatabaseRepresentation.levelsListProperty.GetArrayElementAtIndex(levelDatabaseRepresentation.levelsListProperty.arraySize - 2).objectReferenceValue as LevelSO;
            var newLevel = Object.Instantiate(level);
            string newLevelPath = $"{LEVELS_PATH}{levelDatabaseRepresentation.levelsListProperty.arraySize}.asset";
            AssetDatabase.CreateAsset(newLevel, newLevelPath);
            AssetDatabase.SaveAssets();
            levelDatabaseRepresentation.levelsListProperty.GetArrayElementAtIndex(levelDatabaseRepresentation.levelsListProperty.arraySize - 1).objectReferenceValue = newLevel;
            levelDatabaseRepresentation.serializedLevelDatabaseObject.ApplyModifiedProperties();
            selectedLevelIndex = levelDatabaseRepresentation.levelsListProperty.arraySize - 1;
            previousSelectedLevelIndex = -1; // Force refresh
            OpenLevel();
        }
        if (GUILayout.Button("Test Level"))
        {
            TestLevel();
        }
        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }
        //Go to the Init Scene
        // if (GUILayout.Button("Init Scene", EditorStyles.miniButton, GUILayout.Width(EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing)))
        if (GUILayout.Button("Open Init Scene"))
        {
            EditorSceneManager.OpenScene(INIT_SCENE_PATH);
        }
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        //end of bottom section
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
        levelsTab_rightPanelScrollPos = GUILayout.BeginScrollView(levelsTab_rightPanelScrollPos, false, true);

        float previewSize = 100f;
        float spacing = 10f;
        float rightPanelContentWidth = EditorGUIUtility.currentViewWidth - levelsTab_LeftPanelWidth - levelsTab_DragAreaWidth; // Account for padding
        int itemsPerRow = Mathf.Max(1, Mathf.FloorToInt((rightPanelContentWidth + spacing) / (previewSize + spacing)));

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(levelDatabaseRepresentation.levelsListProperty.GetArrayElementAtIndex(selectedLevelIndex), new GUIContent(FILE_STRING));
        EditorGUILayout.PropertyField(levelRepresentation.levelTimeProperty);
        EditorGUILayout.PropertyField(levelRepresentation.boatSpeedProperty);
        DrawMedalRequirements(levelRepresentation.medalRequirementsProperty);

        //SpawnItems Header
        GUIStyle customStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField(" Spawn Items ", customStyle, GUILayout.Height(20 + EditorGUIUtility.singleLineHeight));

        //Preview Prefab Thumbnails
        SpawnPrefabItems(levelDatabaseRepresentation.spawnItemRepresentation, itemsPerRow, previewSize);

        //Save if levelTime is changed
        if (EditorGUI.EndChangeCheck())
        {
            levelRepresentation.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        #endregion

        GUILayout.EndHorizontal();

        //mark scene as dirty
        if (GUI.changed)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    private void DrawMedalRequirements(SerializedProperty medalProperty)
    {
        EditorGUILayout.Space(10);

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("Medal Requirements (On Collecting GameCurrency)", headerStyle);
        SerializedProperty threeMedal = medalProperty.FindPropertyRelative("requiredCurrencyForThreeMedals");
        SerializedProperty twoMedal = medalProperty.FindPropertyRelative("requiredCurrencyForTwoMedals");
        SerializedProperty oneMedal = medalProperty.FindPropertyRelative("requiredCurrencyForOneMedal");

        int maxCurrency = GetTotalGameCurrency(levelRepresentation.collectablesProperty);
        EditorGUI.BeginChangeCheck();
        int newThree = EditorGUILayout.IntSlider("3 Medals", threeMedal.intValue, 0, maxCurrency);
        int newTwo = EditorGUILayout.IntSlider("2 Medals", twoMedal.intValue, 0, maxCurrency);

        GUI.enabled = false;
        int oneMin = 0;
        int oneMax = Mathf.Max(0, newTwo - 1);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("1 Medal", GUILayout.Width(80));
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"{oneMin} -> {oneMax}");
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;

        if (EditorGUI.EndChangeCheck())
        {
            // apply clamp ONLY when changed
            newTwo = Mathf.Clamp(newTwo, 0, newThree);
            newThree = maxCurrency;

            twoMedal.intValue = newTwo;
            threeMedal.intValue = newThree;
        }
    }
    private int GetTotalGameCurrency(SerializedProperty collectablesProperty)
    {
        int total = 0;

        for (int i = 0; i < collectablesProperty.arraySize; i++)
        {
            SerializedProperty element = collectablesProperty.GetArrayElementAtIndex(i);

            SerializedProperty typeProp = element.FindPropertyRelative("type");

            //  IMPORTANT: enum comparison
            if ((CollectableType)typeProp.enumValueIndex == CollectableType.GameCurrency)
            {
                total++;
            }
        }

        return total;
    }
    private void SpawnPrefabItems(SerializedProperty spawnItemsProperty, int itemsPerRow, float previewSize)
    {
        for (int i = 0; i < spawnItemsProperty.arraySize; i++)
        {
            SpawnItemType spawnItemType = (SpawnItemType)spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("SpawnItemType").enumValueIndex;
            switch (spawnItemType)
            {
                case SpawnItemType.StaticObstacle:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(" Static Obstacles ", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 }, GUILayout.Height(10 + EditorGUIUtility.singleLineHeight));
                    SpawnPrefabItem(spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab"), itemsPerRow, previewSize, spawnItemType);
                    break;
                case SpawnItemType.MovingObstacle:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(" Moving Obstacles ", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 }, GUILayout.Height(10 + EditorGUIUtility.singleLineHeight));
                    SpawnPrefabItem(spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab"), itemsPerRow, previewSize, spawnItemType);
                    break;
                //case SpawnItemType.WhirlpoolObstacle:
                //    EditorGUILayout.Space(5);
                //    EditorGUILayout.LabelField(" Cyclone Obstacle ", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 }, GUILayout.Height(10 + EditorGUIUtility.singleLineHeight));
                //    SpawnPrefabItem(spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab"), itemsPerRow, previewSize, spawnItemType);
                //    break;
                case SpawnItemType.Collectable:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(" Collectables ", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 }, GUILayout.Height(10 + EditorGUIUtility.singleLineHeight));
                    SpawnPrefabItem(spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab"), itemsPerRow, previewSize, spawnItemType);
                    break;
                case SpawnItemType.DrownCharacter:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(" Characters ", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 }, GUILayout.Height(10 + EditorGUIUtility.singleLineHeight));
                    SpawnPrefabItem(spawnItemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab"), itemsPerRow, previewSize, spawnItemType);
                    break;
            }
        }
    }

    private void SpawnPrefabItem(SerializedProperty serializedProperty, int itemsPerRow, float previewSize, SpawnItemType spawnItemType)
    {
        int count = serializedProperty.arraySize;
        if (count > 0)
        {
            for (int i = 0; i < count; i += itemsPerRow)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < itemsPerRow && (i + j) < count; j++)
                {
                    var property = serializedProperty.GetArrayElementAtIndex(i + j);
                    UnityEngine.Object prefab = property.objectReferenceValue;

                    if (prefab == null) continue;

                    Texture2D previewTex = AssetPreview.GetAssetPreview(prefab);
                    if (previewTex == null)
                        previewTex = AssetPreview.GetMiniThumbnail(prefab);

                    GUIContent content = new GUIContent(previewTex);

                    GUILayout.BeginVertical(GUILayout.Width(previewSize));
                    if (GUILayout.Button(content, GUILayout.Width(previewSize), GUILayout.Height(previewSize)))
                    {
                        //  Selection.activeObject = prefab;
                        EditorSceneController.Instance.SpawnPrefabItem(spawnItemType, prefab);
                    }

                    GUILayout.Label(prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }
        }
    }

    private void OpenLevel()
    {
        levelRepresentation = new LevelRepresentation(levelDatabaseRepresentation.levelsListProperty.GetArrayElementAtIndex(selectedLevelIndex).objectReferenceValue);
        EditorSceneController.Instance.Clear();
        EditorSceneController.Instance.SpawnLevelData(levelRepresentation.serializedLevelObject.targetObject as LevelSO);
    }

    private void TestLevel()
    {
        SaveLevel();
        SaveSystem.SaveInt(StringUtils.HIGHEST_COMPLETED_LEVEL, selectedLevelIndex + 1);
        EditorSceneManager.OpenScene(TEST_SCENE_PATH);
        EditorApplication.isPlaying = true;
        //  window.Close();
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

    #region Save
    private void SaveLevel()
    {
        SaveStartPoint();
        SaveMovingObstacle();
        SaveStaticObstacles();
        SaveWhirlpoolObstacle();
        SaveCollectables();
        SaveSavedCharacters();
        levelRepresentation.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    private void SaveWhirlpoolObstacle()
    {
        WhirlpoolEditTool[] whirlpoolEditComponents = EditorSceneController.Instance.GetWhirlpoolEditData();
        levelRepresentation.whirlpoolObstaclesProperty.arraySize = whirlpoolEditComponents.Length;

        for (int i = 0; i < whirlpoolEditComponents.Length; i++)
        {
            SerializedProperty whirlPoolProperty = levelRepresentation.whirlpoolObstaclesProperty.GetArrayElementAtIndex(i);
            whirlPoolProperty.FindPropertyRelative("position").vector3Value = whirlpoolEditComponents[i].transform.position;
            whirlPoolProperty.FindPropertyRelative("scale").floatValue = whirlpoolEditComponents[i].cycloneRadius;
            whirlPoolProperty.FindPropertyRelative("shaderPosition").vector2Value = whirlpoolEditComponents[i].shaderPosition;
        }
    }

    private void SaveStaticObstacles()
    {
        StaticObstacle[] staticObstacles = EditorSceneController.Instance.GetStaticObstacleEditData();
        levelRepresentation.staticObstaclesProperty.arraySize = staticObstacles.Length;

        for (int i = 0; i < staticObstacles.Length; i++)
        {
            SerializedProperty staticObstacleProperty = levelRepresentation.staticObstaclesProperty.GetArrayElementAtIndex(i);
            StaticObstacleRepresentation staticObstacleRepresentation = new StaticObstacleRepresentation(staticObstacleProperty);
            // Set properties for the static obstacle
            staticObstacleRepresentation.typeProperty.enumValueIndex = (int)staticObstacles[i].ObstacleType;
            staticObstacleRepresentation.positionProperty.vector3Value = staticObstacles[i].transform.position;
            staticObstacleRepresentation.rotationProperty.vector3Value = staticObstacles[i].transform.eulerAngles;
            if (staticObstacles[i].transform.localScale == Vector3.zero)
            {
                staticObstacles[i].transform.localScale = Vector3.one;
            }
            staticObstacleRepresentation.scaleProperty.vector3Value = staticObstacles[i].transform.localScale;
        }
    }

    private void SaveCollectables()
    {
        Collectable[] collectables = EditorSceneController.Instance.GetCollectableEditData();
        levelRepresentation.collectablesProperty.arraySize = collectables.Length;

        for (int i = 0; i < collectables.Length; i++)
        {
            SerializedProperty collectableProperty = levelRepresentation.collectablesProperty.GetArrayElementAtIndex(i);
            collectableProperty.FindPropertyRelative("type").enumValueIndex = (int)collectables[i].CollectableType;
            collectableProperty.FindPropertyRelative("position").vector3Value = collectables[i].transform.position;
            collectableProperty.FindPropertyRelative("rotation").vector3Value = collectables[i].transform.eulerAngles;
        }
    }

    private void SaveSavedCharacters()
    {
        DrownCharacterEditTool[] savedCharacters = EditorSceneController.Instance.GetSavedCharacterEditData();
        levelRepresentation.savedCharactersProperty.arraySize = savedCharacters.Length;

        for (int i = 0; i < savedCharacters.Length; i++)
        {
            SerializedProperty savedCharacterProperty = levelRepresentation.savedCharactersProperty.GetArrayElementAtIndex(i);
            SavedCharacterRepresentation savedCharacterRepresentation = new SavedCharacterRepresentation(savedCharacterProperty);
            // Set properties for the static obstacle
            savedCharacterRepresentation.positionProperty.vector3Value = savedCharacters[i].transform.position;
            savedCharacterRepresentation.waitTimePercentageProperty.floatValue = savedCharacters[i].waitTimePercentage;
        }
    }

    private void SaveStartPoint()
    {
        var spawnPointData = EditorSceneController.Instance.GetSpawnPointEditData();
        levelRepresentation.startPointProperty.FindPropertyRelative("Position").vector3Value = spawnPointData.Item1;
        levelRepresentation.startPointProperty.FindPropertyRelative("Rotation").vector3Value = spawnPointData.Item2;
    }

    private void SaveMovingObstacle()
    {
        MovingObstacleEditTool[] movingObstacleEditComponents = EditorSceneController.Instance.GetMovingObstacleEditData();
        levelRepresentation.moveObstaclesProperty.arraySize = movingObstacleEditComponents.Length;

        //  moveObstales_LevelEditProperty.ClearArray();
        for (int i = 0; i < movingObstacleEditComponents.Length; i++)
        {
            SerializedProperty movingObstacleProperty = levelRepresentation.moveObstaclesProperty.GetArrayElementAtIndex(i);
            MoveObstacleRepresentation moveObstacleRepresentation = new MoveObstacleRepresentation(movingObstacleProperty);
            moveObstacleRepresentation.obstacleTypeProperty.enumValueIndex = (int)movingObstacleEditComponents[i].obstacleType;
            //keyframes
            moveObstacleRepresentation.keyFrameProperty.arraySize = movingObstacleEditComponents[i].Keyframes.Length;
            for (int j = 0; j < movingObstacleEditComponents[i].Keyframes.Length; j++)
            {
                SerializedProperty keyframeProperty = moveObstacleRepresentation.keyFrameProperty.GetArrayElementAtIndex(j);
                MoveObstacleRepresentation.KeyFrameRepresentation keyFrameRepresentation = new MoveObstacleRepresentation.KeyFrameRepresentation(keyframeProperty);
                keyFrameRepresentation.positionProperty.vector3Value = movingObstacleEditComponents[i].Keyframes[j].position;
                keyFrameRepresentation.inTangentLocalProperty.vector3Value = movingObstacleEditComponents[i].Keyframes[j].inTangentLocal;
                keyFrameRepresentation.outTangentLocalProperty.vector3Value = movingObstacleEditComponents[i].Keyframes[j].outTangentLocal;
            }

            moveObstacleRepresentation.resolutionProperty.floatValue = movingObstacleEditComponents[i].resolution;
            moveObstacleRepresentation.movementSpeedProperty.floatValue = movingObstacleEditComponents[i].movementSpeed;
            moveObstacleRepresentation.rotationSpeedProperty.floatValue = movingObstacleEditComponents[i].rotationSpeedMultiplier;
            moveObstacleRepresentation.isLoopMovementProperty.boolValue = movingObstacleEditComponents[i].loopedMovement;
            moveObstacleRepresentation.isInverseDirectionProperty.boolValue = movingObstacleEditComponents[i].inverseDirection;
        }
    }
    #endregion

    #region Item Tab
    private void DrawItemsTab()
    {
    }
    #endregion
}
#endif
