#if UNITY_EDITOR
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
    public void Clear()
    {
        if (container.transform != null)
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
    }

    #region Spawn
    public void SpawnPrefabItem(SpawnItemType spawnItemType, Object _object)
    {
        if (spawnItemType == SpawnItemType.DrownCharacter)
        {
            GameObject savedCharacterobject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            DrownCharacterEditComponent savedCharacter = savedCharacterobject.AddComponent<DrownCharacterEditComponent>();
            savedCharacterobject.transform.parent = container.transform;
            savedCharacter.Init(Vector3.zero, 1, currentLevel.LevelTime);
        }
        if (spawnItemType == SpawnItemType.MovingObstacle)
        {
            GameObject movingObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            MovingObstacle movingObstacleComponent = movingObstacleObject.GetComponent<MovingObstacle>();
            MovingObstacleEditComponent movingObstacle = movingObstacleObject.AddComponent<MovingObstacleEditComponent>();
            movingObstacleObject.transform.parent = container.transform;
            movingObstacle.Init(new MovingObstacleData() { type = movingObstacleComponent.ObstacleType });
        }
        if (spawnItemType == SpawnItemType.StaticObstacle)
        {
            GameObject staticObstacleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            StaticObstacle staticObstacle = staticObstacleObject.GetComponent<StaticObstacle>();
            staticObstacleObject.transform.parent = container.transform;
            staticObstacle.Init(Vector3.zero);
        }
        if (spawnItemType == SpawnItemType.WaterHoleObstacle)
        {
            GameObject waterHoleObject = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            WaterHoleEditComponent waterHole = waterHoleObject.AddComponent<WaterHoleEditComponent>();
            waterHoleObject.transform.parent = container.transform;

            int cyclonesCount = container.transform.GetComponentsInChildren<WaterHoleEditComponent>().Length;
            waterHole.Init(new WaterHoleObstacleData() { position = Vector3.zero, scale = 2 }, cyclonesCount);
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
        SpawnStartPoint();
        SpawnMovingObstacles();
        SpawnStaticObstacles();
        SpawnWaterHoleObstacle();
        SpawnCharacter();
        SpawnCollectable();
    }

    private void SpawnWaterHoleObstacle()
    {
        string path = "Assets/Prefabs/WaterHole.prefab";
        if (currentLevel.Obstacle.WaterHoleObstacles == null || currentLevel.Obstacle.WaterHoleObstacles.Length == 0)
        {
            return;
        }
        int cycloneIndex = 0;
        foreach (var item in currentLevel.Obstacle.WaterHoleObstacles)
        {
            cycloneIndex++;
            WaterHoleObstacle waterHolePrefab = AssetDatabase.LoadAssetAtPath<WaterHoleObstacle>(path);
            GameObject waterHoleGameobject = PrefabUtility.InstantiatePrefab(waterHolePrefab.gameObject) as GameObject;
            WaterHoleEditComponent waterHoleEditComponent = waterHoleGameobject.AddComponent<WaterHoleEditComponent>();
            waterHoleGameobject.transform.parent = container.transform;
            waterHoleEditComponent.Init(item, cycloneIndex);
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
            string path = "Assets/Prefabs/DrownCharacter.prefab";
            DrownCharacter drownCharacterPrefab = AssetDatabase.LoadAssetAtPath<DrownCharacter>(path);
            GameObject drownCharacterobject = PrefabUtility.InstantiatePrefab(drownCharacterPrefab.gameObject) as GameObject;
            DrownCharacterEditComponent drownCharacter = drownCharacterobject.AddComponent<DrownCharacterEditComponent>();
            drownCharacterobject.transform.parent = container.transform;
            drownCharacter.Init(characterItem.Position, characterItem.WaitTimePercentage, currentLevel.LevelTime);
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
                case ObstacleType.Barrel:
                    path = "Assets/Prefabs/Barrel.prefab";
                    BarrelObstacle barrelObstaclePrefab = AssetDatabase.LoadAssetAtPath<BarrelObstacle>(path);
                    GameObject barrelGameobject = (GameObject)PrefabUtility.InstantiatePrefab(barrelObstaclePrefab.gameObject);
                    barrelGameobject.transform.parent = container.transform;
                    barrelGameobject.transform.position = item.position;
                    barrelGameobject.transform.rotation = Quaternion.Euler(item.rotation);
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
                    break;
                case ObstacleType.Eel:
                    path = "Assets/Prefabs/Eel.prefab";
                    break;
                case ObstacleType.MantaRay:
                    path = "Assets/Prefabs/MantaRay.prefab";
                    break;
            }
            MovingObstacle sharkObstaclePrefab = AssetDatabase.LoadAssetAtPath<MovingObstacle>(path);
            GameObject sharkGameObject = (GameObject)PrefabUtility.InstantiatePrefab(sharkObstaclePrefab.gameObject);
            MovingObstacleEditComponent movingObstacle = sharkGameObject.AddComponent<MovingObstacleEditComponent>();
            movingObstacle.transform.parent = container.transform;
            movingObstacle.Init(item);
        }
    }

    private void SpawnCollectable()
    {
        if (currentLevel.Collectables.Length == 0)
        {
            return;
        }
        string path = string.Empty;
        foreach (var item in currentLevel.Collectables)
        {
            switch (item.type)
            {
                case CollectableType.GameCurrency:
                    path = "Assets/Prefabs/Collectables/GameCurrency.prefab";
                    Collectable coinPrefab = AssetDatabase.LoadAssetAtPath<Collectable>(path);
                    GameObject coinGameobject = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab.gameObject);
                    Collectable coin = coinGameobject.GetComponent<Collectable>();
                    coin.transform.parent = container.transform;
                    coin.Init(item);
                    break;
                case CollectableType.Magnet:
                    path = "Assets/Prefabs/Collectables/Magnet.prefab";
                    Collectable magnetPrefab = AssetDatabase.LoadAssetAtPath<Collectable>(path);
                    GameObject magnetGameobject = (GameObject)PrefabUtility.InstantiatePrefab(magnetPrefab.gameObject);
                    Collectable magnet = magnetGameobject.GetComponent<Collectable>();
                    magnetGameobject.transform.parent = container.transform;
                    magnet.Init(item);
                    break;
                case CollectableType.SpeedBoost:
                    path = "Assets/Prefabs/Collectables/SpeedBoost.prefab";
                    Collectable speedPrefab = AssetDatabase.LoadAssetAtPath<Collectable>(path);
                    GameObject speedGameobject = (GameObject)PrefabUtility.InstantiatePrefab(speedPrefab.gameObject);
                    Collectable speed = speedGameobject.GetComponent<Collectable>();
                    speedGameobject.transform.parent = container.transform;
                    speed.Init(item);
                    break;
            }
        }
    }
    #endregion


    #region Get Edited Data

    public WaterHoleEditComponent[] GetWaterHoleEditData()
    {
        WaterHoleEditComponent[] data = container.GetComponentsInChildren<WaterHoleEditComponent>();
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
    public MovingObstacleEditComponent[] GetMovingObstacleEditData()
    {
        MovingObstacleEditComponent[] data = container.GetComponentsInChildren<MovingObstacleEditComponent>();
        return data;
    }
    public DrownCharacterEditComponent[] GetSavedCharacterEditData()
    {
        DrownCharacterEditComponent[] data = container.GetComponentsInChildren<DrownCharacterEditComponent>();
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
