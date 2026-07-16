#if UNITY_EDITOR
using BeachHero;
using UnityEditor;
using UnityEngine;

public class EditorSceneController : MonoBehaviour
{
    private static EditorSceneController instance;
    public static EditorSceneController Instance { get => instance; }

    [SerializeField] private GameObject container;
    [SerializeField] private GameObject playerPreviewPrefab;
    [SerializeField] private GameObject[] disableScenePickingObjects;
    private LevelSO currentLevel;

    //Spawn Item Paths 
    private static string drownCharacterPath = "Assets/Prefabs/Characters/DrownCharacter.prefab";
    private static string startPointPath = "Assets/Prefabs/StartPoint.prefab";

    private static string gameCurrencyPath = "Assets/Prefabs/Collectables/GameCurrency.prefab";
    private static string shieldPath = "Assets/Prefabs/Collectables/Shield.prefab";
    private static string speedBoostPath = "Assets/Prefabs/Collectables/SpeedBoost.prefab";
    private static string whirlpoolPath = "Assets/Prefabs/Obstacles/Whirlpool.prefab";
    private static string rockObstaclePath = "Assets/Prefabs/Obstacles/Rock.prefab";
    private static string barrelObstaclePath = "Assets/Prefabs/Obstacles/Barrel.prefab";
    private static string sharkObstaclePath = "Assets/Prefabs/Obstacles/Shark.prefab";
    private static string eelObstaclePath = "Assets/Prefabs/Obstacles/Eel.prefab";
    private static string mantaRayObstaclePath = "Assets/Prefabs/Obstacles/MantaRay.prefab";
    private static string icebergObstaclePath = "Assets/Prefabs/Obstacles/Iceberg.prefab";
    private static string shipWreckObstaclePath = "Assets/Prefabs/Obstacles/ShipWreck.prefab";

    public EditorSceneController()
    {
        instance = this;
        DisableScenePicking();
    }
    public void Clear()
    {
        if (container.transform != null)
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
    }
    private void DisableScenePicking()
    {
        foreach (var item in disableScenePickingObjects)
        {
            SceneVisibilityManager.instance.DisablePicking(item, true);
        }
    }

    #region Spawn
    public void SpawnPrefabItem(SpawnItemType spawnItemType, Object _object)
    {
        if (spawnItemType == SpawnItemType.DrownCharacter)
        {
            GameObject savedCharacterobject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            DrownCharacterEditTool savedCharacter = savedCharacterobject.AddComponent<DrownCharacterEditTool>();
            savedCharacterobject.transform.parent = container.transform;
            savedCharacter.Init(Vector3.zero, 1, currentLevel.LevelTime);
        }
        if (spawnItemType == SpawnItemType.MovingObstacle)
        {
            GameObject movingObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            MovingObstacle movingObstacleComponent = movingObstacleObject.GetComponent<MovingObstacle>();
            MovingObstacleEditTool movingObstacle = movingObstacleObject.AddComponent<MovingObstacleEditTool>();
            movingObstacleObject.transform.parent = container.transform;
            movingObstacle.Init(new MovingObstacleData()
            {
                type = movingObstacleComponent.ObstacleType,
                movementSpeed = 4,
                resolution = 1,
                rotationSpeedMultiplier = 1,
                loopedMovement = true
            });
        }
        if (spawnItemType == SpawnItemType.StaticObstacle)
        {
            GameObject staticObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            StaticObstacle staticObstacle = staticObstacleObject.GetComponent<StaticObstacle>();
            staticObstacleObject.transform.parent = container.transform;
            staticObstacle.Init(Vector3.zero);
        }
        if (spawnItemType == SpawnItemType.WhirlpoolObstacle)
        {
            GameObject whirlpoolObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            WhirlpoolEditTool whirlpool = whirlpoolObject.AddComponent<WhirlpoolEditTool>();
            whirlpoolObject.transform.parent = container.transform;

            int cyclonesCount = container.transform.GetComponentsInChildren<WhirlpoolEditTool>().Length;
            whirlpool.Init(new WhirlpoolObstacleData() { position = Vector3.zero, scale = 2 }, cyclonesCount);
        }
        if (spawnItemType == SpawnItemType.Collectable)
        {
            GameObject collectableObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            Collectable collectable = collectableObject.GetComponent<Collectable>();
            collectableObject.transform.parent = container.transform;
            collectable.Init(new CollectableData() { type = collectable.CollectableType });
        }
    }

    public void SpawnLevelData(LevelSO _levelSO)
    {
        currentLevel = _levelSO;
        DisableScenePicking();
        SpawnStartPoint();
        SpawnMovingObstacles();
        SpawnStaticObstacles();
        SpawnWhirlpoolObstacle();
        SpawnCharacter();
        SpawnCollectable();
    }


    private void SpawnWhirlpoolObstacle()
    {
        if (currentLevel.Obstacle.WhirlpoolObstacles == null || currentLevel.Obstacle.WhirlpoolObstacles.Length == 0)
        {
            return;
        }
        int cycloneIndex = 0;
        foreach (var item in currentLevel.Obstacle.WhirlpoolObstacles)
        {
            cycloneIndex++;
            WhirlpoolObstacle whirlpoolPrefab = AssetDatabase.LoadAssetAtPath<WhirlpoolObstacle>(whirlpoolPath);
            GameObject whirlpoolGameobject = PrefabUtility.InstantiatePrefab(whirlpoolPrefab.gameObject) as GameObject;
            WhirlpoolEditTool whirlpoolEditComponent = whirlpoolGameobject.AddComponent<WhirlpoolEditTool>();
            whirlpoolGameobject.transform.parent = container.transform;
            whirlpoolEditComponent.Init(item, cycloneIndex);
            SetChildrenNotEditable(whirlpoolGameobject.transform);
        }
    }

    private void SpawnCharacter()
    {
        if (currentLevel.DrownCharacters == null || currentLevel.DrownCharacters.Length == 0)
        {
            return;
        }
        foreach (var characterItem in currentLevel.DrownCharacters)
        {
            DrownCharacter drownCharacterPrefab = AssetDatabase.LoadAssetAtPath<DrownCharacter>(drownCharacterPath);
            GameObject drownCharacterobject = PrefabUtility.InstantiatePrefab(drownCharacterPrefab.gameObject) as GameObject;
            DrownCharacterEditTool drownCharacter = drownCharacterobject.AddComponent<DrownCharacterEditTool>();
            drownCharacterobject.transform.parent = container.transform;
            drownCharacter.Init(characterItem.Position, characterItem.WaitTimePercentage, currentLevel.LevelTime);
            SetChildrenNotEditable(drownCharacterobject.transform);
        }
    }

    private void SpawnStaticObstacles()
    {
        foreach (var item in currentLevel.Obstacle.StaticObstacles)
        {
            string path = item.type switch
            {
                ObstacleType.Rock => rockObstaclePath,
                ObstacleType.Barrel => barrelObstaclePath,
                ObstacleType.Iceberg => icebergObstaclePath,
                ObstacleType.ShipWreck => shipWreckObstaclePath,
                _ => null
            };

            StaticObstacle prefab = AssetDatabase.LoadAssetAtPath<StaticObstacle>(path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
            instance.transform.SetParent(container.transform);
            instance.transform.SetPositionAndRotation(item.position, Quaternion.Euler(item.rotation));
            instance.transform.localScale = item.scale;
            SetChildrenNotEditable(instance.transform);
        }
    }

    private void SpawnStartPoint()
    {
        StartPointBehaviour startPointPrefab = AssetDatabase.LoadAssetAtPath<StartPointBehaviour>(startPointPath);
        GameObject startPoint = PrefabUtility.InstantiatePrefab(startPointPrefab.gameObject) as GameObject;
        startPoint.transform.parent = container.transform;
        startPoint.transform.position = currentLevel.StartPointData.Position;
        startPoint.transform.rotation = Quaternion.Euler(currentLevel.StartPointData.Rotation);
        startPoint.gameObject.AddComponent<PlayerPreviewEditTool>();

        //Add Player preview Tool
        GameObject playerPreviewObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPreviewPrefab);
        playerPreviewObject.transform.parent = startPoint.transform;
        playerPreviewObject.transform.localPosition = Vector3.zero;
        SceneVisibilityManager.instance.DisablePicking(playerPreviewObject, false);
    }

    private void SpawnMovingObstacles()
    {
        string path = string.Empty;
        foreach (var item in currentLevel.Obstacle.MovingObstacles)
        {
            switch (item.type)
            {
                case ObstacleType.Shark:
                    path = sharkObstaclePath;
                    break;
                case ObstacleType.Eel:
                    path = eelObstaclePath;
                    break;
                case ObstacleType.MantaRay:
                    path = mantaRayObstaclePath;
                    break;
            }
            MovingObstacle sharkObstaclePrefab = AssetDatabase.LoadAssetAtPath<MovingObstacle>(path);
            GameObject sharkGameObject = (GameObject)PrefabUtility.InstantiatePrefab(sharkObstaclePrefab.gameObject);
            MovingObstacleEditTool movingObstacle = sharkGameObject.AddComponent<MovingObstacleEditTool>();
            movingObstacle.transform.parent = container.transform;
            movingObstacle.Init(item);
            SetChildrenNotEditable(movingObstacle.transform);
        }
    }

    private void SpawnCollectable()
    {
        if (currentLevel.Collectables == null || currentLevel.Collectables.Length == 0)
        {
            return;
        }
        foreach (var item in currentLevel.Collectables)
        {
            string path = item.type switch
            {
                CollectableType.GameCurrency => gameCurrencyPath,
                CollectableType.Shield => shieldPath,
                CollectableType.SpeedBoost => speedBoostPath,
                _ => null
            };
            Collectable prefab = AssetDatabase.LoadAssetAtPath<Collectable>(path);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
            Collectable collectable = go.GetComponent<Collectable>();
            collectable.transform.parent = container.transform;
            collectable.Init(item);
            SetChildrenNotEditable(collectable.transform);
        }
    }
    private void SetChildrenNotEditable(Transform parentTransform)
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            child.hideFlags |= HideFlags.HideInHierarchy;
            child.hideFlags |= HideFlags.NotEditable;
        }
    }

    #endregion

    #region Get Edited Data
    public WhirlpoolEditTool[] GetWhirlpoolEditData()
    {
        WhirlpoolEditTool[] data = container.GetComponentsInChildren<WhirlpoolEditTool>();
        return data;
    }

    public StaticObstacle[] GetStaticObstacleEditData()
    {
        StaticObstacle[] data = container.GetComponentsInChildren<StaticObstacle>();
        return data;
    }

    public (Vector3, Vector3) GetSpawnPointEditData()
    {
        StartPointBehaviour data = container.GetComponentInChildren<StartPointBehaviour>();
        Vector3 position = data.transform.position;
        Vector3 rotation = data.transform.rotation.eulerAngles;
        return (position, rotation);
    }
    public MovingObstacleEditTool[] GetMovingObstacleEditData()
    {
        MovingObstacleEditTool[] data = container.GetComponentsInChildren<MovingObstacleEditTool>();
        return data;
    }
    public DrownCharacterEditTool[] GetSavedCharacterEditData()
    {
        DrownCharacterEditTool[] data = container.GetComponentsInChildren<DrownCharacterEditTool>();
        return data;
    }
    public Collectable[] GetCollectableEditData()
    {
        Collectable[] data = container.GetComponentsInChildren<Collectable>();
        return data;
    }
    #endregion
}
#endif
