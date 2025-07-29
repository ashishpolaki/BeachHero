using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class LevelController : MonoBehaviour
    {
        public enum LevelPhase
        {
            None,
            Intro,
            DrawingPath,
            Simulating,
            CompletedSuccess,
            CompletedFail
        }
        public enum PlayerMode
        {
            None,
            FTUE,
            Normal
        }

        #region Inspector Variables
        [SerializeField] private PoolController poolManager;
        [SerializeField] private LayerMask startPointLayer;
        [SerializeField] private LayerMask touchLayer;
        [SerializeField] private float minTrailPointsDistance = 0.3f;
        [SerializeField] private float spacing = 0.5f;
        [SerializeField] private float magnetRadius = 5f;
        #endregion

        #region Private Variables
        private StartPointBehaviour startPointBehaviour;
        private Player player;
        private List<DrownCharacter> savedCharactersList = new();
        private Dictionary<ObstacleType, List<Obstacle>> obstaclesDictionary = new();
        private Dictionary<CollectableType, List<Collectable>> collectableDictionary = new();
        private List<Vector3> curvePoints = new List<Vector3>();
        private PathTrail playerPathDrawTrail;

        private Camera cam;
        private Ray ray;
        private RaycastHit raycastHit;
        private Vector3 lastTrailPoint;
        private List<Vector3> drawnPoints = new();
        private List<Vector3> smoothedDrawnPoints = new();

        private LevelPhase levelPhase = LevelPhase.None;
        private PlayerMode playerMode = PlayerMode.Normal;

        private bool hasDrawnPath = false;
        private bool isPathDrawingAllowed = false;
        private bool isMagnetActive = false;

        private int gameCurrencyCount;
        private int targetDrownCharacters;
        [Tooltip("Number of characters saved by the player in current level")]
        private int drownCharactersCounter;
        #endregion

        #region Properties
        public Transform PlayerTransform => player != null ? player.transform : null;
        public Transform DrownCharacter => savedCharactersList.Count > 0 ? savedCharactersList[0].transform : null;
        public bool IsLevelPassed => levelPhase == LevelPhase.CompletedSuccess;
        public int GameCurrencyCount => gameCurrencyCount;

        public Camera Cam => cam ??= Camera.main;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            InputManager.GetInstance.OnMouseClickDown += OnMouseClickDown;
            InputManager.GetInstance.OnMouseClickUp += OnMouseClickUp;
        }

        private void OnDisable()
        {
            if (InputManager.GetInstance != null)
            {
                InputManager.GetInstance.OnMouseClickDown -= OnMouseClickDown;
                InputManager.GetInstance.OnMouseClickUp -= OnMouseClickUp;
            }
        }
        #endregion

        #region Mouse Methods
        private void OnMouseClickDown(Vector2 position)
        {
            //Dont draw the path more than once
            if (!hasDrawnPath && levelPhase == LevelPhase.DrawingPath)
            {
                playerPathDrawTrail.ResetTrail(player.transform.position);
                ray = Cam.ScreenPointToRay(position);
                if (Physics.Raycast(ray, out raycastHit, 1000f, startPointLayer))
                {
                    HapticsManager.GetInstance.MediumImpactHaptic();
                    isPathDrawingAllowed = true;
                    if (playerMode == PlayerMode.FTUE)
                    {
                        GameController.GetInstance.TutorialController.OnFTUEPlayerTouch();
                    }
                    //   raycastHit.collider.GetComponent<StartPointBehaviour>();
                    // lastStartPointPosition = raycastHit.collider.gameObject.transform.position;
                }
            }
        }

        private void OnMouseClickUp(Vector2 position)
        {
            if (levelPhase == LevelPhase.DrawingPath && !hasDrawnPath)
            {
                hasDrawnPath = true;
                isPathDrawingAllowed = false;
                if (drawnPoints.Count >= 4)
                {
                    smoothedDrawnPoints = CatmullSplineUtils.GetEvenlySpacedPoints(drawnPoints, spacing);
                    StartSimulation();
                }
                else
                {
                    hasDrawnPath = false;
                    drawnPoints.Clear();
                }
            }
        }
        #endregion

        #region DrawPath

        private void UpdatePath(Vector3 newPosition)
        {
            if (Vector3.Distance(newPosition, lastTrailPoint) > minTrailPointsDistance)
            {
                // Add the new position to the path points
                drawnPoints.Add(newPosition);

                // Generate a smooth curve using Catmull-Rom splines
                if (drawnPoints.Count >= 4) // Need at least 4 points for Catmull-Rom
                {
                    for (float t = 0; t <= 1; t += 0.05f) // Adjust step size for smoother curves
                    {
                        Vector3 interpolatedPoint = CatmullSplineUtils.GetPoint(
                            drawnPoints[drawnPoints.Count - 4], // P0
                            drawnPoints[drawnPoints.Count - 3], // P1
                            drawnPoints[drawnPoints.Count - 2], // P2
                            drawnPoints[drawnPoints.Count - 1], // P3
                            t
                        );
                        curvePoints.Add(interpolatedPoint);

                        // Update the trail position to the interpolated point
                        playerPathDrawTrail.transform.position = interpolatedPoint;
                    }
                }

                // Update the last trail point
                lastTrailPoint = newPosition;
            }
        }

        private void DrawPath()
        {
            if (levelPhase == LevelPhase.DrawingPath && isPathDrawingAllowed)
            {
                ray = Cam.ScreenPointToRay(InputManager.MousePosition);
                if (Physics.Raycast(ray, out raycastHit, 1000f, touchLayer))
                {
                    Vector3 hitPoint = raycastHit.point;
                    if (!raycastHit.collider.CompareTag("Ground"))
                        hitPoint.y = 0f;

                    UpdatePath(hitPoint);
                }
            }
        }
        #endregion

        #region Game Flow
        private void StartSimulation()
        {
            player.StartMovement(smoothedDrawnPoints.ToArray());
            playerPathDrawTrail.SetTrailSpeed(player.MovementSpeed / 2f);
            levelPhase = LevelPhase.Simulating;
            if (playerMode == PlayerMode.FTUE)
            {
                GameController.GetInstance.TutorialController.OnFTUEPathDrawn();
            }
        }
        public void InitializePlayerData(bool isFirstTimeUser)
        {
            playerMode = isFirstTimeUser ? PlayerMode.FTUE : PlayerMode.Normal;
            levelPhase = LevelPhase.DrawingPath;

            int boatIndex = GameController.GetInstance.SkinController.GetSavedBoatIndex();
            float speed = GameController.GetInstance.SkinController.GetSelectedBoatSpeed();
            GameObject boatPRefab = GameController.GetInstance.SkinController.GetSelectedBoatPrefab();
            player.SpawnBoat(boatIndex, speed, boatPRefab);
        }
        public void SetLevelCompletionResult(bool passed)
        {
            levelPhase = passed ? LevelPhase.CompletedSuccess : LevelPhase.CompletedFail;
            if (!passed)
                player.StopMovement();
        }
        public void OnCharacterPickUp()
        {
            drownCharactersCounter++;
            if (drownCharactersCounter >= targetDrownCharacters)
            {
                GameController.GetInstance.OnLevelPass();
            }
        }
        #endregion

        #region Pool
        private void ReturnToPoolEverything()
        {
            //StartPoint
            if (startPointBehaviour != null)
                poolManager.StartPointPool.ReturnObject(startPointBehaviour.gameObject);

            //Player
            if (player != null)
                poolManager.PlayerPool.ReturnObject(player.gameObject);

            //Collectables
            foreach (var collectableList in collectableDictionary.Values)
            {
                foreach (var collectable in collectableList)
                {
                    if (collectable.CollectableType == CollectableType.GameCurrency)
                    {
                        poolManager.GameCurrencyPool.ReturnObject(collectable.gameObject);
                    }
                    else if (collectable.CollectableType == CollectableType.Magnet)
                    {
                        poolManager.MagnetPowerupPool.ReturnObject(collectable.gameObject);
                    }
                    else if (collectable.CollectableType == CollectableType.SpeedBoost)
                    {
                        poolManager.SpeedPowerupPool.ReturnObject(collectable.gameObject);
                    }
                }
            }

            //Characters
            foreach (var savedCharacter in savedCharactersList)
            {
                poolManager.SavedCharacterPool.ReturnObject(savedCharacter.gameObject);
            }

            //Trails
            if (playerPathDrawTrail != null)
            {
                poolManager.PathTrailPool.ReturnObject(playerPathDrawTrail.gameObject);
            }

            //Obstacles
            foreach (var obstacleList in obstaclesDictionary.Values)
            {
                foreach (var obstacle in obstacleList)
                {
                    switch (obstacle.ObstacleType)
                    {
                        case ObstacleType.Shark:
                            poolManager.SharkPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Eel:
                            poolManager.EelPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.MantaRay:
                            poolManager.MantaRayPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.WaterHole:
                            poolManager.WaterHolePool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Rock:
                            poolManager.RockPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Barrel:
                            poolManager.BarrelPool.ReturnObject(obstacle.gameObject);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Powerups/Collectables
        public void OnGameCurrencyCollect()
        {
            gameCurrencyCount++;
        }
        public void OnActivatePowerup(PowerupType powerUpType)
        {
            if (powerUpType == PowerupType.Magnet)
            {
                ActivateMagnetPowerup();
            }
            else if (powerUpType == PowerupType.SpeedBoost)
            {
                ActivateSpeedPowerup();
            }
        }

        public void ActivateMagnetPowerup()
        {
            isMagnetActive = true;
            player.ActivateMagnetPowerup();
        }

        public void ActivateSpeedPowerup()
        {
            player.ActivateSpeedPowerup();
        }

        private void UpdateCollectables()
        {
            foreach (var collectableList in collectableDictionary.Values)
            {
                foreach (var collectable in collectableList)
                {
                    collectable.UpdateState();
                }
            }
            if (isMagnetActive)
            {
                if (collectableDictionary != null && collectableDictionary.ContainsKey(CollectableType.GameCurrency))
                    foreach (var gameCurrency in collectableDictionary[CollectableType.GameCurrency])
                    {
                        float distance = Vector3.Distance(player.transform.position, gameCurrency.transform.position);
                        GameCurrencyCollectable gcCollectable = (GameCurrencyCollectable)gameCurrency;
                        if (!gcCollectable.CanMoveToTarget)
                        {
                            if (distance <= magnetRadius)
                            {
                                gcCollectable.SetTarget(player.transform);
                            }
                        }
                    }
            }
        }
        #endregion

        #region States
        public void StartState(LevelSO levelSO)
        {
            ResetState();
            targetDrownCharacters = levelSO.DrownCharacters.Length;

            // Load the level data
            SpawnStartPoint(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnPlayer(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnObstacles(levelSO.Obstacle);
            SpawnCollectables(levelSO.Collectables);
            SpawnSavedCharacters(levelSO.LevelTime, levelSO.DrownCharacters);
            SpawnTrails();
        }

        public void UpdateState()
        {
            DrawPath();
            UpdateObstacles();

            if (levelPhase == LevelPhase.Simulating || levelPhase == LevelPhase.CompletedSuccess)
            {
                player?.UpdateState();
            }

            // Start the Simulation after the path is drawn
            if (levelPhase != LevelPhase.Simulating)
            {
                return;
            }

            UpdateCollectables();

            if (levelPhase != LevelPhase.CompletedFail)
            {
                //Update Characters
                foreach (var savedCharacter in savedCharactersList)
                {
                    savedCharacter.UpdateState();
                }
            }
        }

        private void ResetState()
        {
            gameCurrencyCount = 0;
            drownCharactersCounter = 0;
            ReturnToPoolEverything();
            isMagnetActive = false;
            hasDrawnPath = false;
            isPathDrawingAllowed = false;
            levelPhase = LevelPhase.None;
            drawnPoints.Clear();
            curvePoints.Clear();
            smoothedDrawnPoints.Clear();
            lastTrailPoint = Vector3.zero;
            drownCharactersCounter = 0;
            savedCharactersList.Clear();
            obstaclesDictionary.Clear();
            collectableDictionary.Clear();
        }
        #endregion

        #region Obstacles
        private void SpawnObstacles(ObstacleData obstacle)
        {
            SpawnStaticObstacles(obstacle);
            SpawnMoveableObstacles(obstacle);
            SpawnWaterHoleObstacle(obstacle.WaterHoleObstacles);
        }

        private void UpdateObstacles()
        {
            foreach (var obstacleList in obstaclesDictionary.Values)
                foreach (var obstacle in obstacleList)
                    obstacle.UpdateState();
        }

        #region Moving obstacle
        private void SpawnMoveableObstacles(ObstacleData obstacle)
        {
            if (obstacle.MovingObstacles != null && obstacle.MovingObstacles.Length > 0)
            {
                foreach (var movingObstacle in obstacle.MovingObstacles)
                {
                    if (!obstaclesDictionary.ContainsKey(movingObstacle.type))
                    {
                        obstaclesDictionary[movingObstacle.type] = new List<Obstacle>();
                    }
                    switch (movingObstacle.type)
                    {
                        case ObstacleType.Eel:
                            SpawnEel(movingObstacle);
                            break;
                        case ObstacleType.Shark:
                            SpawnShark(movingObstacle);
                            break;
                        case ObstacleType.MantaRay:
                            SpawnMantaRay(movingObstacle);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SpawnMantaRay(MovingObstacleData movingObstacleData)
        {
            MantaRayObstacle mantaRay = poolManager.MantaRayPool.GetObject().GetComponent<MantaRayObstacle>();
            mantaRay.Init(movingObstacleData);
            obstaclesDictionary[ObstacleType.MantaRay].Add(mantaRay);
        }

        private void SpawnShark(MovingObstacleData movingObstacleData)
        {
            SharkObstacle shark = poolManager.SharkPool.GetObject().GetComponent<SharkObstacle>();
            shark.Init(movingObstacleData);
            obstaclesDictionary[ObstacleType.Shark].Add(shark);
        }

        private void SpawnEel(MovingObstacleData movingObstacleData)
        {
            Eel eel = poolManager.EelPool.GetObject().GetComponent<Eel>();
            eel.Init(movingObstacleData);
            obstaclesDictionary[ObstacleType.Eel].Add(eel);
        }
        #endregion

        #region Static obstacle
        private void SpawnStaticObstacles(ObstacleData obstacle)
        {
            if (obstacle.StaticObstacles != null && obstacle.StaticObstacles.Length > 0)
            {
                foreach (var staticObstacle in obstacle.StaticObstacles)
                {
                    if (!obstaclesDictionary.ContainsKey(staticObstacle.type))
                    {
                        obstaclesDictionary[staticObstacle.type] = new List<Obstacle>();
                    }
                    switch (staticObstacle.type)
                    {
                        case ObstacleType.Rock:
                            SpawnRock(staticObstacle.type, staticObstacle);
                            break;
                        case ObstacleType.Barrel:
                            SpawnBarrel(staticObstacle.type, staticObstacle);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SpawnBarrel(ObstacleType obstacleType, StaticObstacleData rockObstacle)
        {
            BarrelObstacle barrel = poolManager.BarrelPool.GetObject().GetComponent<BarrelObstacle>();
            barrel.transform.SetPositionAndRotation(rockObstacle.position, Quaternion.Euler(rockObstacle.rotation));
            obstaclesDictionary[obstacleType].Add(barrel);
        }

        private void SpawnRock(ObstacleType obstacleType, StaticObstacleData rockObstacle)
        {
            RockObstacle rock = poolManager.RockPool.GetObject().GetComponent<RockObstacle>();
            rock.transform.SetPositionAndRotation(rockObstacle.position, Quaternion.Euler(rockObstacle.rotation));
            obstaclesDictionary[obstacleType].Add(rock);
        }
        #endregion

        #region WaterHole Obstacle
        private void SpawnWaterHoleObstacle(WaterHoleObstacleData[] waterHoleObstacleData)
        {
            int cycloneIndex = 0;
            if (waterHoleObstacleData != null && waterHoleObstacleData.Length > 0)
            {
                foreach (var waterHole in waterHoleObstacleData)
                {
                    cycloneIndex++;
                    if (!obstaclesDictionary.ContainsKey(ObstacleType.WaterHole))
                    {
                        obstaclesDictionary[ObstacleType.WaterHole] = new List<Obstacle>();
                    }
                    WaterHoleObstacle waterHoleObstacle = poolManager.WaterHolePool.GetObject().GetComponent<WaterHoleObstacle>();
                    waterHoleObstacle.transform.position = waterHole.position;
                    waterHoleObstacle.Init(waterHole, cycloneIndex);
                    obstaclesDictionary[ObstacleType.WaterHole].Add(waterHoleObstacle);
                }
            }
        }
        #endregion
        #endregion

        #region Spawn
        private void SpawnTrails()
        {
            playerPathDrawTrail = poolManager.PathTrailPool.GetObject().GetComponent<PathTrail>();
            playerPathDrawTrail.ClearRenderer();
        }

        private void SpawnSavedCharacters(float levelTime, DrownCharacterData[] savedCharacterDatas)
        {
            foreach (var savedCharacterData in savedCharacterDatas)
            {
                var savedCharacter = poolManager.SavedCharacterPool.GetObject().GetComponent<DrownCharacter>();
                savedCharacter.Init(savedCharacterData.Position, savedCharacterData.WaitTimePercentage, levelTime);
                savedCharactersList.Add(savedCharacter);
            }
        }

        private void SpawnCollectables(CollectableData[] collectableDatas)
        {
            foreach (var collectable in collectableDatas)
            {
                if (!collectableDictionary.ContainsKey(collectable.type))
                {
                    collectableDictionary[collectable.type] = new List<Collectable>();
                }
                switch (collectable.type)
                {
                    case CollectableType.GameCurrency:
                        SpawnCoin(collectable);
                        break;
                    case CollectableType.Magnet:
                        SpawnMagnet(collectable);
                        break;
                    case CollectableType.SpeedBoost:
                        SpawnSpeed(collectable);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SpawnMagnet(CollectableData collectableData)
        {
            Collectable magnet = poolManager.MagnetPowerupPool.GetObject().GetComponent<Collectable>();
            magnet.Init(collectableData);
            collectableDictionary[collectableData.type].Add(magnet);
        }

        private void SpawnSpeed(CollectableData collectableData)
        {
            Collectable speed = poolManager.SpeedPowerupPool.GetObject().GetComponent<Collectable>();
            speed.Init(collectableData);
            collectableDictionary[collectableData.type].Add(speed);
        }

        private void SpawnCoin(CollectableData collectableData)
        {
            Collectable collectable = poolManager.GameCurrencyPool.GetObject().GetComponent<Collectable>();
            collectable.Init(collectableData);
            collectableDictionary[collectableData.type].Add(collectable);
        }

        private void SpawnStartPoint(Vector3 pos, Vector3 rot)
        {
            startPointBehaviour = poolManager.StartPointPool.GetObject().GetComponent<StartPointBehaviour>();
            startPointBehaviour.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
        }

        private void SpawnPlayer(Vector3 pos, Vector3 rot)
        {
            player = poolManager.PlayerPool.GetObject().GetComponent<Player>();
            player.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
            player.Init();
        }
        #endregion
    }
}
