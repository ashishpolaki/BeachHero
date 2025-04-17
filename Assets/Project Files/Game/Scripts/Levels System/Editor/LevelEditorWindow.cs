#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Bokka.BeachRescue
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //Path variables need to be changed ----------------------------------------
        private const string ENUM_FILE_PATH = "Assets/Project Files/Game/Scripts/Levels System/Items.cs";
        private const string GAME_SCENE_PATH = "Assets/Project Files/Game/Scenes/Game.unity";
        private const string EDITOR_SCENE_PATH = "Assets/Project Files/Game/Scenes/Level Editor.unity";
        private static string EDITOR_SCENE_NAME = "Level Editor";

        //Window configuration
        private const string TITLE = "Level Editor";
        private const float WINDOW_MIN_WIDTH = 600;
        private const float WINDOW_MIN_HEIGHT = 560;
        private const float WINDOW_MAX_WIDTH = 800;
        private const float WINDOW_MAX_HEIGHT = 700;

        //Level database fields
        private const string LEVELS_PROPERTY_NAME = "levelsList";
        private const string ITEMS_PROPERTY_NAME = "items";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty itemsSerializedProperty;

        //EnumObjectsList 
        private const string TYPE_PROPERTY_PATH = "item";
        private const string PREFAB_PROPERTY_PATH = "prefab";
        private const string CATEGORY_PROPERTY_PATH = "category";
        private const string SPAWN_POSITION_PROPERTY_PATH = "spawnPosition";
        private bool enumCompiling;
        private EnumObjectsList enumObjectsList;

        //TabHandler
        private TabHandler tabHandler;
        private const string LEVELS_TAB_NAME = "Levels";
        private const string ITEMS_TAB_NAME = "Items";

        //sidebar
        private LevelsHandler levelsHandler;
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 240;
        private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";
        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";

        private const string OPEN_GAME_SCENE_WARNING = "Please make sure you saved changes before swiching scene. Are you ready to proceed?";
        private const string REMOVE_SELECTION = "Remove selection";

        //ItemSave
        private const string ITEM_SAVE_TYPE_PROPERTY_PATH = "type";
        private const string POSITION_PROPERTY_PATH = "position";
        private const string ROTATION_PROPERTY_PATH = "rotation";
        private const string SCALE_PROPERTY_PATH = "scale";

        //General
        private const string YES = "Yes";
        private const string CANCEL = "Cancel";
        private const string WARNING_TITLE = "Warning";
        private SerializedProperty tempProperty;
        private string tempPropertyLabel;

        //rest of levels tab
        private const string ITEMS_LABEL = "Spawn items:";
        private const string FILE = "file:";
        private const string COMPILING = "Compiling...";
        private const string ITEM_UNASSIGNED_ERROR = "Please assign prefab to this item in \"Items\"  tab.";
        private const string SELECT_LEVEL = "Playtest level";

        private const float ITEMS_BUTTON_SPACE = 8;
        private const float ITEMS_BUTTON_WIDTH = 80;
        private const float ITEMS_BUTTON_HEIGHT = 80;

        private bool prefabAssigned;
        private GUIContent itemContent;
        private SerializedProperty currentLevelItemProperty;
        private Vector2 levelItemsScrollVector;
        private float itemPosX;
        private float itemPosY;
        private Rect itemsRect;
        private Rect itemRect;
        private int itemsPerRow;
        private int rowCount;
        private List<Item> itemEnumValues;
        private int currentSideBarWidth;
        private bool lastActiveLevelOpened;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private Rect itemsListWidthRect;
        private float currentItemListWidth;
        private GameObject tempGameobject;

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            return builder.Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelsDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(Level);
        }

        protected override void ReadLevelDatabaseFields()
        {
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            itemsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(ITEMS_PROPERTY_NAME);
        }

        protected override void InitialiseVariables()
        {
            Serializer.Init();
            enumCompiling = false;
            enumObjectsList = new EnumObjectsList(itemsSerializedProperty, TYPE_PROPERTY_PATH, PREFAB_PROPERTY_PATH, ENUM_FILE_PATH, OnBeforeEnumFileupdateCallback);
            enumObjectsList.EnableCategories(CATEGORY_PROPERTY_PATH);
            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab(LEVELS_TAB_NAME, DisplayLevelsTab));
            tabHandler.AddTab(new TabHandler.Tab(ITEMS_TAB_NAME, enumObjectsList.DisplayTab));
            itemEnumValues = new List<Item>();
            Item[] items = (Item[])Enum.GetValues(typeof(Item));

            for (int i = 0; i < items.Length; i++)
            {
                itemEnumValues.Add(items[i]);
            }

            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return;
            }

            if (change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (levelsHandler.SelectedLevelIndex == -1)
            {
                OpenScene(GAME_SCENE_PATH);
            }
            else
            {
                TestLevel();
            }
        }

        private void OpenLastActiveLevel()
        {
            if (!lastActiveLevelOpened)
            {
                if ((levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
                {
                    int levelIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0, levelsSerializedProperty.arraySize - 1);
                    levelsHandler.CustomList.SelectedIndex = levelIndex;
                    levelsHandler.OpenLevel(levelIndex);
                }

                lastActiveLevelOpened = true;
            }
        }

        private void OnBeforeEnumFileupdateCallback()
        {
            enumCompiling = true;
        }

        protected override void Styles()
        {
            if (levelsDatabase != null)
            {
                levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
            }

            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            SaveLevelIfPosssible();
            PlayerPrefs.SetInt(PREFS_LEVEL, index);
            PlayerPrefs.Save();
            selectedLevelRepresentation = new LevelRepresentation(levelObject);
            levelsHandler.UpdateCurrentLevelLabel(GetLevelLabel(levelObject, index));
            LoadLevel();
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            return levelRepresentation.GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            levelRepresentation.Clear();
        }

        protected override void DrawContent()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                DrawOpenEditorScene();
                return;
            }

            if (enumCompiling)
            {
                EditorGUILayout.LabelField(COMPILING, EditorCustomStyles.labelLargeBold);
                return;
            }

            tabHandler.DisplayTab();
        }

        private void DrawOpenEditorScene()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

            if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
            {
                OpenScene(EDITOR_SCENE_PATH);
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayLevelsTab()
        {
            OpenLastActiveLevel();
            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(currentSideBarWidth));
            levelsHandler.DisplayReordableList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            HandleChangingSideBar();

            //level content
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MinWidth(8), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    levelsHandler.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    levelsHandler.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void DisplaySidebarButtons()
        {
            if (GUILayout.Button("Rename Levels", EditorCustomStyles.button))
            {
                SaveLevelIfPosssible();
                levelsHandler.RenameLevels();
            }

            if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorCustomStyles.button))
            {
                if (EditorUtility.DisplayDialog(WARNING_TITLE, OPEN_GAME_SCENE_WARNING, YES, CANCEL))
                {
                    SaveLevelIfPosssible();
                    OpenScene(GAME_SCENE_PATH);
                }
            }

            if (GUILayout.Button(REMOVE_SELECTION, EditorCustomStyles.button))
            {
                SaveLevelIfPosssible();
                selectedLevelRepresentation = null;
                levelsHandler.ClearSelection();
                ClearScene();
            }
        }

        private static void ClearScene()
        {
            EditorSceneController.Instance.Clear();
        }

        private void SetAsCurrentLevel()
        {
            EditorSetLevel(levelsHandler.SelectedLevelIndex);
        }

        public static void EditorSetLevel(int levelIndex)
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();

            SimpleIntSave levelIndexSave = tempSave.GetSaveObject<SimpleIntSave>("Level Number");
            levelIndexSave.Value = levelIndex;

            tempSave.Flush(false);

            SaveController.SaveCustom(tempSave);
        }

        private void DisplaySelectedLevel()
        {
            if (levelsHandler.SelectedLevelIndex == -1)
            {
                return;
            }

            //handle level file field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(levelsHandler.SelectedLevelProperty, new GUIContent(FILE));

            if (EditorGUI.EndChangeCheck())
            {
                levelsHandler.ReopenLevel();
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            EditorGUILayout.PropertyField(selectedLevelRepresentation.maxWaitingDurationProperty);
            EditorGUILayout.Space();

            if (GUILayout.Button(SELECT_LEVEL, EditorCustomStyles.button, GUILayout.Width(EditorGUIUtility.labelWidth)))
            {
                TestLevel();
            }

            DisplayItemsListSection();
            EditorGUILayout.Space();
        }

        private void TestLevel()
        {
            SaveLevel();
            SetAsCurrentLevel();
            OpenScene(GAME_SCENE_PATH);
            EditorApplication.isPlaying = true;
            window.Close();
        }

        private void DisplayItemsListSection()
        {
            EditorGUILayout.LabelField(ITEMS_LABEL);
            itemsListWidthRect = GUILayoutUtility.GetRect(1, Screen.width, 0, 0, GUILayout.ExpandWidth(true));

            if (itemsListWidthRect.width > 1)
            {
                currentItemListWidth = itemsListWidthRect.width;
            }

            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (itemsSerializedProperty.arraySize != 0)
            {
                itemsPerRow = Mathf.FloorToInt((currentItemListWidth - 16) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH)); // 16- space for vertical scroll
                rowCount = Mathf.CeilToInt(itemsSerializedProperty.arraySize * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < itemsSerializedProperty.arraySize; i++)
            {
                tempProperty = itemsSerializedProperty.GetArrayElementAtIndex(i);
                tempPropertyLabel = tempProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).enumDisplayNames[tempProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).enumValueIndex];
                prefabAssigned = tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue != null;
                LevelItem.Category category = (LevelItem.Category)tempProperty.FindPropertyRelative(CATEGORY_PROPERTY_PATH).enumValueIndex;
                tempGameobject = tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;

                if (category == LevelItem.Category.StartPoint)
                {
                    continue; // We aren`t displaying start points
                }

                if (prefabAssigned)
                {
                    if (AssetPreview.GetAssetPreview(tempGameobject) == null)
                    {
                        if (AssetPreview.IsLoadingAssetPreview(tempGameobject.GetInstanceID()))
                        {
                            itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempGameobject), tempPropertyLabel);
                        }
                        else
                        {
                            itemContent = new GUIContent(AssetPreview.GetMiniThumbnail(tempGameobject), tempPropertyLabel);
                        }
                    }
                    else
                    {
                        itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempGameobject), tempPropertyLabel);
                    }

                }
                else
                {
                    itemContent = new GUIContent(tempPropertyLabel, ITEM_UNASSIGNED_ERROR);
                }

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                EditorGUI.BeginDisabledGroup(!prefabAssigned);

                if (GUI.Button(itemRect, itemContent, EditorCustomStyles.button))
                {
                    switch (category)
                    {
                        case LevelItem.Category.Character:
                            EditorSceneController.Instance.SpawnCharacter(tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue, GetSpawnItemSave(tempProperty), 1f);
                            break;
                        case LevelItem.Category.MovingObstacle:
                            EditorSceneController.Instance.SpawnMovingObstacle(tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue, new MovingObstacleSave());
                            break;
                        case LevelItem.Category.Item:
                            EditorSceneController.Instance.SpawnItem(tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue, GetSpawnItemSave(tempProperty));
                            break;
                        default:
                            break;
                    }
                }

                EditorGUI.EndDisabledGroup();

                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private ItemSave GetSpawnItemSave(SerializedProperty element)
        {
            ItemSave itemSave = new ItemSave();
            itemSave.Type = itemEnumValues[element.FindPropertyRelative(TYPE_PROPERTY_PATH).enumValueIndex];
            itemSave.Position = element.FindPropertyRelative(SPAWN_POSITION_PROPERTY_PATH).vector3Value;
            itemSave.Rotation = Vector3.zero;
            itemSave.Scale = Vector3.one;
            return itemSave;
        }

        private void LoadLevel()
        {
            EditorSceneController.Instance.Clear();
            LoadStartPoints();
            LoadItems();
            LoadCharacters();
            LoadMovingObstacles();
        }

        private void LoadStartPoints()
        {
            int prefabIndex;
            GameObject prefabRef;
            prefabIndex = itemEnumValues.IndexOf(Item.StartPoint);
            prefabRef = GetItemPrefab(prefabIndex);

            EditorSceneController.Instance.SpawnStartPoint(prefabRef, selectedLevelRepresentation.startPointProperty.vector3Value);
        }

        private void LoadItems()
        {
            GameObject prefabRef = null;
            SerializedProperty element;
            ItemSave itemSave;

            for (int i = 0; i < selectedLevelRepresentation.itemsProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(i);
                prefabRef = GetItemPrefab(element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex);

                itemSave = new ItemSave();
                itemSave.Type = itemEnumValues[element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex];
                itemSave.Position = element.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                itemSave.Rotation = element.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value;
                itemSave.Scale = element.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value;

                EditorSceneController.Instance.SpawnItem(prefabRef, itemSave);
            }
        }


        private void LoadCharacters()
        {
            GameObject prefabRef = null;
            SerializedProperty element;
            ItemSave itemSave;
            float waitingPercentage;

            for (int i = 0; i < selectedLevelRepresentation.charactersProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.charactersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("itemSave");
                waitingPercentage = selectedLevelRepresentation.charactersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("waitingPercentage").floatValue;
                prefabRef = GetItemPrefab(element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex);

                itemSave = new ItemSave();
                itemSave.Type = itemEnumValues[element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex];
                itemSave.Position = element.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                itemSave.Rotation = element.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value;
                itemSave.Scale = element.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value;

                EditorSceneController.Instance.SpawnCharacter(prefabRef, itemSave, waitingPercentage);
            }
        }
        private void LoadMovingObstacles()
        {
            int prefabIndex;
            GameObject prefabRef;
            SerializedProperty element;

            prefabIndex = itemEnumValues.IndexOf(Item.MovingObstacle);
            prefabRef = GetItemPrefab(prefabIndex);

            for (int i = 0; i < selectedLevelRepresentation.movingObstaclesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.movingObstaclesProperty.GetArrayElementAtIndex(i);
                MovingObstacleSave save = new MovingObstacleSave();
                save.type = (MovingObstacleType)element.FindPropertyRelative("type").enumValueIndex;
                save.movementSpeed = element.FindPropertyRelative("movementSpeed").floatValue;
                save.loopedMovement = element.FindPropertyRelative("loopedMovement").boolValue;
                save.inverseDirection = element.FindPropertyRelative("inverseDirection").boolValue;
                save.circlarMovementCenter = element.FindPropertyRelative("circlarMovementCenter").vector3Value;
                save.circlarMovementRadius = element.FindPropertyRelative("circlarMovementRadius").floatValue;
                save.linearMovementStartPosition = element.FindPropertyRelative("linearMovementStartPosition").vector3Value;
                save.linearMovementFinishPosition = element.FindPropertyRelative("linearMovementFinishPosition").vector3Value;

                EditorSceneController.Instance.SpawnMovingObstacle(prefabRef, save);
            }
        }

        private void SaveLevel()
        {
            SaveStartPoints();
            SaveItems();
            SaveCharacters();
            SaveMovingObstacles();

            selectedLevelRepresentation.ApplyChanges();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
            AssetDatabase.SaveAssets();
        }

        private void SaveLevelIfPosssible()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return;
            }

            if (selectedLevelRepresentation == null)
            {
                return;
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            try
            {
                SaveLevel();
            }
            catch
            {

            }

            levelsHandler.SetLevelLabels();
        }

        private void SaveStartPoints()
        {
            selectedLevelRepresentation.startPointProperty.vector3Value = EditorSceneController.Instance.CollectStartPoint();
        }

        private void SaveItems()
        {
            ItemSave[] data = EditorSceneController.Instance.CollectItems();
            selectedLevelRepresentation.itemsProperty.arraySize = data.Length;
            SerializedProperty element;

            for (int i = 0; i < data.Length; i++)
            {
                element = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex = itemEnumValues.IndexOf(data[i].Type);
                element.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = data[i].Position;
                element.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value = data[i].Rotation;
                element.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value = data[i].Scale;
            }
        }

        private void SaveCharacters()
        {
            CharacterSave[] data = EditorSceneController.Instance.CollectCharacters();
            selectedLevelRepresentation.charactersProperty.arraySize = data.Length;
            SerializedProperty element;

            for (int i = 0; i < data.Length; i++)
            {
                selectedLevelRepresentation.charactersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("waitingPercentage").floatValue = data[i].WaitingPercentage;
                element = selectedLevelRepresentation.charactersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("itemSave");
                element.FindPropertyRelative(ITEM_SAVE_TYPE_PROPERTY_PATH).enumValueIndex = itemEnumValues.IndexOf(data[i].ItemSave.Type);
                element.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = data[i].ItemSave.Position;
                element.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value = data[i].ItemSave.Rotation;
                element.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value = data[i].ItemSave.Scale;
            }
        }

        private void SaveMovingObstacles()
        {
            MovingObstacleSave[] data = EditorSceneController.Instance.CollectMovingObstacles();
            selectedLevelRepresentation.movingObstaclesProperty.arraySize = data.Length;
            SerializedProperty element;

            for (int i = 0; i < data.Length; i++)
            {
                element = selectedLevelRepresentation.movingObstaclesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("type").enumValueIndex = (int)data[i].type;
                element.FindPropertyRelative("movementSpeed").floatValue = data[i].movementSpeed;
                element.FindPropertyRelative("loopedMovement").boolValue = data[i].loopedMovement;
                element.FindPropertyRelative("inverseDirection").boolValue = data[i].inverseDirection;
                element.FindPropertyRelative("circlarMovementCenter").vector3Value = data[i].circlarMovementCenter;
                element.FindPropertyRelative("circlarMovementRadius").floatValue = data[i].circlarMovementRadius;
                element.FindPropertyRelative("linearMovementStartPosition").vector3Value = data[i].linearMovementStartPosition;
                element.FindPropertyRelative("linearMovementFinishPosition").vector3Value = data[i].linearMovementFinishPosition;
            }
        }


        private GameObject GetItemPrefab(int enumValueIndex)
        {
            for (int i = 0; i < itemsSerializedProperty.arraySize; i++)
            {
                if (itemsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).enumValueIndex == enumValueIndex)
                {
                    return itemsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;
                }
            }

            Debug.LogError("GetItemPrefab element not found");
            return null;
        }

        private void OnDestroy()
        {
            SaveLevelIfPosssible();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                OpenScene(GAME_SCENE_PATH);
            }
        }

        protected class LevelRepresentation : LevelRepresentationBase
        {
            private const string START_POINT_PROPERTY_NAME = "startPoint";
            private const string CHARACTERS_PROPERTY_NAME = "charactersList";
            private const string MOVING_OBSTACLES_PROPERTY_NAME = "movingObstaclesList";
            private const string ITEMS_PROPERTY_NAME = "itemsList";
            private const string MAX_WAITING_DURATION_PROPERTY_NAME = "maxWaitingDuration";
            public SerializedProperty startPointProperty;
            public SerializedProperty charactersProperty;
            public SerializedProperty movingObstaclesProperty;
            public SerializedProperty itemsProperty;
            public SerializedProperty maxWaitingDurationProperty;

            //this empty constructor is nessesary
            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }


            protected override void ReadFields()
            {
                startPointProperty = serializedLevelObject.FindProperty(START_POINT_PROPERTY_NAME);
                charactersProperty = serializedLevelObject.FindProperty(CHARACTERS_PROPERTY_NAME);
                movingObstaclesProperty = serializedLevelObject.FindProperty(MOVING_OBSTACLES_PROPERTY_NAME);
                itemsProperty = serializedLevelObject.FindProperty(ITEMS_PROPERTY_NAME);
                maxWaitingDurationProperty = serializedLevelObject.FindProperty(MAX_WAITING_DURATION_PROPERTY_NAME);
            }

            public override void Clear()
            {
                if (!NullLevel)
                {
                    startPointProperty.vector3Value = Vector3.zero;
                    charactersProperty.arraySize = 0;
                    movingObstaclesProperty.arraySize = 0;
                    itemsProperty.arraySize = 0;
                    maxWaitingDurationProperty.floatValue = 1;
                    ApplyChanges();
                }

            }

            public override string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                if (NullLevel)
                {
                    return base.GetLevelLabel(index, stringBuilder);
                }
                else
                {
                    return base.GetLevelLabel(index, stringBuilder) + SEPARATOR + (charactersProperty.arraySize + movingObstaclesProperty.arraySize + itemsProperty.arraySize);
                }
            }
        }
    }
}

// -----------------
// Scene interraction level editor V1.5
// -----------------

// Changelog
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
