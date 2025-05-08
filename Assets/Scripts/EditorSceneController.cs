using BeachHero;
using UnityEditor;
using UnityEngine;

public class EditorSceneController : MonoBehaviour
{
    private static EditorSceneController instance;
    public static EditorSceneController Instance { get => instance; }

    [SerializeField] private GameObject container;
    private LevelSO currentLevel;

    public EditorSceneController()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.transform.GetChild(i).gameObject);
        }
    }

    #region Spawn
    public void SpawnPrefabItem(SpawnItemType spawnItemType,Object _object)
    {
        if(spawnItemType == SpawnItemType.SavedCharacter)
        {
            GameObject savedCharacterobject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            SavedCharacterEditComponent savedCharacter = savedCharacterobject.AddComponent<SavedCharacterEditComponent>();
            savedCharacterobject.transform.parent = container.transform;
            savedCharacter.Init(Vector3.zero, 1, currentLevel.LevelTime);
        }
        if (spawnItemType == SpawnItemType.MovingObstacle)
        {
            GameObject movingObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            MovingObstacleEditComponent movingObstacle = movingObstacleObject.AddComponent<MovingObstacleEditComponent>();
            movingObstacleObject.transform.parent = container.transform;
            movingObstacle.Init(new MovingObstacleData());
        }
        if (spawnItemType == SpawnItemType.StaticObstacle)
        {
            GameObject staticObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            StaticObstacle staticObstacle = staticObstacleObject.AddComponent<StaticObstacle>();
            staticObstacleObject.transform.parent = container.transform;
            staticObstacle.Init(Vector3.zero);
        }
        if (spawnItemType == SpawnItemType.Collectable)
        {
            GameObject collectableObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            Collectable collectable = collectableObject.GetComponent<Collectable>();
            collectableObject.transform.parent = container.transform;
            collectable.Init(new CollectableData());
        }
    }

    public void SpawnLevelData(LevelSO _levelSO)
    {
        currentLevel = _levelSO;
        SpawnStartPoint();
        SpawnMovingObstacles();
        SpawnStaticObstacles();
        SpawnCharacter();
        SpawnCollectable();
    }
    private void SpawnCharacter()
    {
        foreach (var characterItem in currentLevel.SavedCharacters)
        {
            string path = "Assets/Prefabs/SavedCharacter.prefab";
            SavedCharacter savedCharacterPrefab = AssetDatabase.LoadAssetAtPath<SavedCharacter>(path);
            GameObject savedCharacterobject = PrefabUtility.InstantiatePrefab(savedCharacterPrefab.gameObject) as GameObject;
            SavedCharacterEditComponent savedCharacter = savedCharacterobject.AddComponent<SavedCharacterEditComponent>();
            savedCharacterobject.transform.parent = container.transform;
            savedCharacter.Init(characterItem.Position, characterItem.WaitTimePercentage, currentLevel.LevelTime);
        }
    }
    private void SpawnStaticObstacles()
    {
        string path = string.Empty;
        foreach (var item in currentLevel.Obstacle.StaticObstacles)
        {
            switch (item.type)
            {
                case ObstacleType.Rock:
                    path = "Assets/Prefabs/Rock.prefab";
                    RockObstacle rockObstaclePrefab = AssetDatabase.LoadAssetAtPath<RockObstacle>(path);
                    GameObject rockGameobject = (GameObject)PrefabUtility.InstantiatePrefab(rockObstaclePrefab.gameObject);
                    rockGameobject.transform.parent = container.transform;
                    rockGameobject.transform.position = item.position;
                    rockGameobject.transform.rotation = Quaternion.Euler(item.rotation);
                    break;
            }
        }
    }

    private void SpawnStartPoint()
    {
        string path = "Assets/Prefabs/StartPoint.prefab";
        StartPointBehaviour startPointPrefab = AssetDatabase.LoadAssetAtPath<StartPointBehaviour>(path);
        GameObject startPoint = PrefabUtility.InstantiatePrefab(startPointPrefab.gameObject) as GameObject;
        startPoint.transform.parent = container.transform;
        startPoint.transform.position = currentLevel.StartPointData.Position;
        startPoint.transform.rotation = Quaternion.Euler(currentLevel.StartPointData.Rotation);
    }

    private void SpawnMovingObstacles()
    {
        string path = string.Empty;
        foreach (var item in currentLevel.Obstacle.MovingObstacles)
        {
            switch (item.type)
            {
                case ObstacleType.Shark:
                    path = "Assets/Prefabs/Shark.prefab";
                    MovingObstacle sharkObstaclePrefab = AssetDatabase.LoadAssetAtPath<MovingObstacle>(path);
                    GameObject sharkGameObject = (GameObject)PrefabUtility.InstantiatePrefab(sharkObstaclePrefab.gameObject);
                    MovingObstacleEditComponent movingObstacle = sharkGameObject.AddComponent<MovingObstacleEditComponent>();
                    movingObstacle.transform.parent = container.transform;
                    movingObstacle.Init(item);
                    break;
                case ObstacleType.Eel:
                    path = "Assets/Prefabs/Eel.prefab";
                    break;
            }
        }
    }

    private void SpawnCollectable()
    {
        string path = string.Empty;
        foreach (var item in currentLevel.Collectables)
        {
            switch (item.type)
            {
                case CollectableType.Coin:
                    path = "Assets/Prefabs/Coin.prefab";
                    Collectable coinPrefab = AssetDatabase.LoadAssetAtPath<Collectable>(path);
                    GameObject coinGameobject = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab.gameObject);
                    Collectable coin = coinGameobject.GetComponent<Collectable>();
                    coin.transform.parent = container.transform;
                    coin.Init(item);
                    break;
                case CollectableType.Magnet:
                    path = "Assets/Prefabs/Magnet.prefab";
                    break;
                case CollectableType.Speed:
                    path = "Assets/Prefabs/Speed.prefab";
                    break;
            }
        }
    }
    #endregion


    #region Get Edited Data
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
    public MovingObstacleEditComponent[] GetMovingObstacleEditData()
    {
        MovingObstacleEditComponent[] data = container.GetComponentsInChildren<MovingObstacleEditComponent>();
        return data;
    }
    public SavedCharacterEditComponent[] GetSavedCharacterEditData()
    {
        SavedCharacterEditComponent[] data = container.GetComponentsInChildren<SavedCharacterEditComponent>();
        return data;
    }
    public Collectable[] GetCollectableEditData()
    {
        Collectable[] data = container.GetComponentsInChildren<Collectable>();
        return data;
    }
    #endregion
}
