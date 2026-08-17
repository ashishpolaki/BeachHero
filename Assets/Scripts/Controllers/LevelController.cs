using LitMotion;
using System;
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
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private PoolController poolManager;
        [SerializeField] private LayerMask startPointLayer;
        [SerializeField] private LayerMask touchLayer;
        [SerializeField] private float minTrailPointsDistance = 0.3f;
        [SerializeField] private float spacing = 0.5f;
        [SerializeField] private float spawnAnimationDuration = 1;
        [SerializeField] private Ease spawnAnimationEase = Ease.OutElastic;
        [SerializeField] private int levelFailToShowHint = 2;
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
        private MedalCurrencyRequirements medalCurrencyRequirements = new MedalCurrencyRequirements();
        private bool hasDrawnPath = false;
        private bool isPathDrawingAllowed = false;
        private bool isPlayerInitialRotationSet = false;

        private int currentLevelIndex;
        private int coinCount;
        private int targetDrownCharacters;
        [Tooltip("Number of characters saved by the player in current level")]
        private int drownCharactersCounter;
        private int levelFailCounter;
        private float levelElapsedTime;
        #endregion

        #region Properties
        public Transform PlayerTransform => player != null ? player.transform : null;
        public bool IsLevelPassed => levelPhase == LevelPhase.CompletedSuccess;
        public int CoinCount => coinCount;
        public int StarsEarned
        {
            get; private set;
        }
        public Camera Cam => cam ??= Camera.main;
        public int CurrentLevelIndex => currentLevelIndex;
        #endregion

        #region Actions
        public event Action OnPlayerTouch;
        public event Action OnDrawPathError;
        public event Action OnCompleteSpawnAnimation;
        public event Action<int> OnMedalCountUpdated;
        public event Action OnCoinCollectAnimation;
        public event Action OnDrownCharactersCollected;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            if (InputManager.GetInstance != null)
            {
                InputManager.GetInstance.OnMouseClickDown += OnMouseClickDown;
                InputManager.GetInstance.OnMouseClickUp += OnMouseClickUp;
            }
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
            if (GameController.GetInstance.GameState != GameState.Playing ||
                TutorialController.GetInstance.IsActive)
            {
                return;
            }
            //Dont draw the path more than once
            if (!hasDrawnPath && levelPhase == LevelPhase.DrawingPath)
            {
                playerPathDrawTrail.ResetTrail(player.transform.position);
                ray = Cam.ScreenPointToRay(position);
                if (Physics.Raycast(ray, out raycastHit, 1000f, startPointLayer))
                {
                    HapticsManager.GetInstance.MediumImpactHaptic();
                    isPathDrawingAllowed = true;
                    AudioController.GetInstance.PlaySoundInLoop(AudioType.PathDraw);
                    OnPlayerTouch?.Invoke();
                }
            }
        }

        private void OnMouseClickUp(Vector2 position)
        {
            if (GameController.GetInstance.GameState != GameState.Playing
                 || TutorialController.GetInstance.IsActive)
            {
                return;
            }
            if (levelPhase == LevelPhase.DrawingPath && !hasDrawnPath)
            {
                hasDrawnPath = true;
                if (drawnPoints.Count >= 4)
                {
                    smoothedDrawnPoints = CatmullSplineUtils.GetEvenlySpacedPoints(drawnPoints, spacing);
                    if (smoothedDrawnPoints != null && smoothedDrawnPoints.Count >= 2)
                    {
                        GameController.GetInstance.PowerupController.ActivateSelectedPowerups();
                        StartSimulation();
                    }
                    else
                    {
                        hasDrawnPath = false;
                        drawnPoints.Clear();
                        if (isPathDrawingAllowed)
                        {
                            OnDrawPathError?.Invoke();
                        }
                    }
                }
                else
                {
                    hasDrawnPath = false;
                    drawnPoints.Clear();
                    if (isPathDrawingAllowed)
                    {
                        OnDrawPathError?.Invoke();
                    }
                }
                isPathDrawingAllowed = false;
                AudioController.GetInstance.StopSound(AudioType.PathDraw);
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

                    if (!isPlayerInitialRotationSet)
                    {
                        isPlayerInitialRotationSet = true;
                        var smoothedPoints = CatmullSplineUtils.GetEvenlySpacedPoints(curvePoints, spacing);

                        Vector3 nextPoint = smoothedPoints[1];
                        Vector3 direction = (nextPoint - player.transform.position).normalized;
                        Quaternion targetRot = Quaternion.LookRotation(direction);
                        TweenManager.Rotate(player.transform, player.transform.rotation, targetRot, 0.2f);
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
                    //if (!raycastHit.collider.CompareTag("Ground"))
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
            playerPathDrawTrail.InitializeTrail(smoothedDrawnPoints.ToArray(), player.MovementSpeed / 2f);
            startPointBehaviour.StopRippleAnimation();
            levelPhase = LevelPhase.Simulating;
            if (playerMode == PlayerMode.FTUE)
            {
                TutorialController.GetInstance.OnPathDrawn();
            }
        }

        public void SetLevelCompletionResult(bool passed)
        {
            levelPhase = passed ? LevelPhase.CompletedSuccess : LevelPhase.CompletedFail;
            if (!passed)
            {
                //  player.StopMovement();
            }
            // If the player failed, increment the fail counter. If they passed, reset it.
            levelFailCounter = passed ? 0 : levelFailCounter + 1;
        }
        #endregion

        #region Level Fail Hint
        public bool ShouldShowConsecutiveLossHint()
        {
            return levelFailCounter >= levelFailToShowHint;
        }
        public void ResetLevelFailCounter()
        {
            levelFailCounter = 0;
        }
        #endregion

        #region Player
        public void InitializePlayerData(bool isFTUE)
        {
            playerMode = isFTUE ? PlayerMode.FTUE : PlayerMode.Normal;
            levelPhase = LevelPhase.DrawingPath;
            //int boatIndex = GameController.GetInstance.SkinController.GetSavedBoatIndex();
            int boatIndex = levelDatabaseSO.GetLevelByIndex(currentLevelIndex).BoatIndex;
            int boatColorIndex = GameController.GetInstance.SkinController.GetSavedBoatColorIndex(boatIndex);
            float speed = GameController.GetInstance.SkinController.GetSelectedBoatSpeed(boatIndex);
            GameObject boatPRefab = GameController.GetInstance.SkinController.GetSelectedBoatPrefab(boatIndex);
            player.UpdateBoat(boatIndex, boatColorIndex, speed, boatPRefab);
        }
        public void UpdateBoat(int index, int boatColorIndex)
        {
            player.UpdateBoat(index, boatColorIndex, 0, GameController.GetInstance.SkinController.GetBoatSkinByIndex(index).BoatPrefab);
        }
        public float GetPlayerSpeed()
        {
            return player != null ? player.MovementSpeed : 0f;
        }
        #endregion

        #region Drown Character
        public void OnDrownCharacterPickUp()
        {
            drownCharactersCounter++;
            if (drownCharactersCounter >= targetDrownCharacters)
            {
                GameController.GetInstance.OnLevelWin();
                OnDrownCharactersCollected?.Invoke();
            }
        }
        public Transform GetDrowningCharacter(int index)
        {
            if (index >= 0 && index < savedCharactersList.Count)
                return savedCharactersList[index].transform;

            return null;
        }
        #endregion

        #region Path Trail 
        public void TrimTrailFromStart()
        {
            if (playerPathDrawTrail != null)
            {
                playerPathDrawTrail.TrimTrailFromStart();
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
                    collectable.ResetState();
                    if (collectable.CollectableType == CollectableType.Coin)
                    {
                        poolManager.GameCurrencyPool.ReturnObject(collectable.gameObject);
                    }
                    else if (collectable.CollectableType == CollectableType.Shield)
                    {
                        poolManager.SheildPowerupPool.ReturnObject(collectable.gameObject);
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
                savedCharacter.ResetState();
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
                    obstacle.ResetObstacle();
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
                        case ObstacleType.Whirlpool:
                            poolManager.WaterHolePool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Rock:
                            poolManager.RockPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Barrel:
                            poolManager.BarrelPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.Iceberg:
                            poolManager.IcebergPool.ReturnObject(obstacle.gameObject);
                            break;
                        case ObstacleType.ShipWreck:
                            poolManager.ShipWreckPool.ReturnObject(obstacle.gameObject);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Stars
        public void CalculateStars()
        {
            UpdateStarsCount();
            OnMedalCountUpdated?.Invoke(StarsEarned);
        }
        public void UpdateStarsCount()
        {
            if (coinCount >= medalCurrencyRequirements.requiredCurrencyForThreeMedals && StarsEarned < 3)
            {
                StarsEarned = 3;
            }
            else if (coinCount >= medalCurrencyRequirements.requiredCurrencyForTwoMedals && StarsEarned < 2)
            {
                StarsEarned = 2;
            }
            else if (coinCount >= medalCurrencyRequirements.requiredCurrencyForOneMedal && StarsEarned < 1)
            {
                StarsEarned = 1;
            }
        }
        public void SaveCurrentLevelProgress()
        {
            // Set the stars for the current level
            if (StarsEarned == 0)
            {
                UpdateStarsCount();
            }

            // Set the score for the current level
            float levelTime = levelDatabaseSO.GetLevelByIndex(currentLevelIndex).LevelTime;
            float remainingTime = Mathf.Max(0f, levelTime - levelElapsedTime);
            int score = IntUtils.LEADERBOARD_LEVEL_MULTIPLIER +
                StarsEarned * IntUtils.LEADERBOARD_STAR_MULTIPLIER +
                Mathf.RoundToInt(remainingTime * IntUtils.LEADERBOARD_REMAINING_TIME_MULTIPLIER);

            levelDatabaseSO.SetStarsAndScoreForLevel(currentLevelIndex,StarsEarned, score);
            PlayGamesController.GetInstance.SubmitScore();
            MapController.GetInstance.OnLevelComplete(currentLevelIndex);
        }
        #endregion

        #region Collectables
        public void OnCoinCollect()
        {
            coinCount++;
        }
        public void OnCoinAnimation()
        {
            OnCoinCollectAnimation?.Invoke();
        }
        private void EnsureCollectableList(CollectableType type)
        {
            if (!collectableDictionary.ContainsKey(type))
            {
                collectableDictionary[type] = new List<Collectable>();
            }
        }
        private Collectable GetCollectableFromPool(CollectableType type)
        {
            return type switch
            {
                CollectableType.Coin =>
                    poolManager.GameCurrencyPool.GetObject().GetComponent<Collectable>(),

                CollectableType.Shield =>
                    poolManager.SheildPowerupPool.GetObject().GetComponent<Collectable>(),

                CollectableType.SpeedBoost =>
                    poolManager.SpeedPowerupPool.GetObject().GetComponent<Collectable>(),

                _ => null
            };
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
            //if (isMagnetActive)
            //{
            //    if (collectableDictionary != null && collectableDictionary.ContainsKey(CollectableType.GameCurrency))
            //        foreach (var gameCurrency in collectableDictionary[CollectableType.GameCurrency])
            //        {
            //            float distance = Vector3.Distance(player.transform.position, gameCurrency.transform.position);
            //            GameCurrencyCollectable gcCollectable = (GameCurrencyCollectable)gameCurrency;
            //            if (!gcCollectable.CanMoveToTarget)
            //            {
            //                if (distance <= magnetRadius)
            //                {
            //                    gcCollectable.SetTarget(player.transform);
            //                }
            //            }
            //        }
            //}
        }
        #endregion

        #region States
        public void StartState(LevelSO levelSO, int _currentLevelIndex)
        {
            ResetState();
            currentLevelIndex = _currentLevelIndex;
            targetDrownCharacters = levelSO.DrownCharacters.Length;
            medalCurrencyRequirements = levelSO.MedalsRequirements;

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

            UpdateCollectables();

            // Start the Simulation after the path is drawn
            if (levelPhase != LevelPhase.Simulating)
            {
                return;
            }
            levelElapsedTime += Time.deltaTime;

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
            StarsEarned = 0;
            isPlayerInitialRotationSet = false;
            coinCount = 0;
            drownCharactersCounter = 0;
            levelElapsedTime = 0f;
            ReturnToPoolEverything();
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
            SpawnWaterHoleObstacle(obstacle.WhirlpoolObstacles);
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
        public void DisableStaticObstaclesNearCamera(Vector3 centerPosition, float distanceThreshold = 5f)
        {
            ObstacleType[] staticTypes = new ObstacleType[]
            {
        ObstacleType.Rock,
        ObstacleType.Barrel,
        ObstacleType.Iceberg,
        ObstacleType.ShipWreck
            };
            foreach (var type in staticTypes)
            {
                if (obstaclesDictionary.TryGetValue(type, out var obstacleList))
                {
                    foreach (var obstacle in obstacleList)
                    {
                        if (obstacle != null && obstacle.gameObject.activeSelf)
                        {
                            if (Vector3.Distance(obstacle.transform.position, centerPosition) <= distanceThreshold)
                            {
                                obstacle.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void SpawnStaticObstacles(ObstacleData obstacle)
        {
            if (obstacle.StaticObstacles != null && obstacle.StaticObstacles.Length > 0)
            {
                foreach (var staticObstacleData in obstacle.StaticObstacles)
                {
                    if (!obstaclesDictionary.ContainsKey(staticObstacleData.type))
                    {
                        obstaclesDictionary[staticObstacleData.type] = new List<Obstacle>();
                    }
                    StaticObstacle staticObstacle = null;
                    switch (staticObstacleData.type)
                    {
                        case ObstacleType.Rock:
                            staticObstacle = poolManager.RockPool.GetObject().GetComponent<StaticObstacle>();
                            break;
                        case ObstacleType.Barrel:
                            staticObstacle = poolManager.BarrelPool.GetObject().GetComponent<StaticObstacle>();
                            break;
                        case ObstacleType.Iceberg:
                            staticObstacle = poolManager.IcebergPool.GetObject().GetComponent<StaticObstacle>();
                            break;
                        case ObstacleType.ShipWreck:
                            staticObstacle = poolManager.ShipWreckPool.GetObject().GetComponent<StaticObstacle>();
                            break;
                        default:
                            break;
                    }
                    staticObstacle.transform.SetPositionAndRotation(staticObstacleData.position, Quaternion.Euler(staticObstacleData.rotation));
                    staticObstacle.SetScale(staticObstacleData.scale);
                    obstaclesDictionary[staticObstacleData.type].Add(staticObstacle);
                }
            }
        }
        #endregion

        #region Whirlpool Obstacle
        private void SpawnWaterHoleObstacle(WhirlpoolObstacleData[] whirlpoolObstacleData)
        {
            int cycloneIndex = 0;
            if (whirlpoolObstacleData != null && whirlpoolObstacleData.Length > 0)
            {
                foreach (var whirlpool in whirlpoolObstacleData)
                {
                    cycloneIndex++;
                    if (!obstaclesDictionary.ContainsKey(ObstacleType.Whirlpool))
                    {
                        obstaclesDictionary[ObstacleType.Whirlpool] = new List<Obstacle>();
                    }
                    WhirlpoolObstacle waterHoleObstacle = poolManager.WaterHolePool.GetObject().GetComponent<WhirlpoolObstacle>();
                    waterHoleObstacle.transform.position = whirlpool.position;
                    waterHoleObstacle.Init(whirlpool, cycloneIndex);
                    obstaclesDictionary[ObstacleType.Whirlpool].Add(waterHoleObstacle);
                }
            }
        }
        #endregion

        #endregion

        #region Spawn
        public void ResetAllSpawnedObjectsScale()
        {
            //Player
            player.transform.localScale = Vector3.zero;

            //Saved Character
            foreach (var savedCharacter in savedCharactersList)
            {
                savedCharacter.transform.localScale = Vector3.zero;
            }

            //Obstacles
            foreach (var obstacleList in obstaclesDictionary.Values)
            {
                foreach (var obstacle in obstacleList)
                {
                    obstacle.transform.localScale = Vector3.zero;
                }
            }

            //Collectables
            foreach (var collectableList in collectableDictionary.Values)
            {
                foreach (var collectable in collectableList)
                {
                    collectable.transform.localScale = Vector3.zero;
                }
            }

        }

        public void PlaySpawnAnimations()
        {
            AudioController.GetInstance.PlaySound(AudioType.Spawn);

            //Player
            TweenManager.Scale(Vector3.zero, Vector3.one, player.transform, spawnAnimationDuration, spawnAnimationEase);

            //Saved Character
            foreach (var savedCharacter in savedCharactersList)
            {
                TweenManager.Scale(Vector3.zero, Vector3.one, savedCharacter.transform, spawnAnimationDuration, spawnAnimationEase);
            }

            //Obstacles
            foreach (var obstacleList in obstaclesDictionary.Values)
            {
                foreach (var obstacle in obstacleList)
                {
                    TweenManager.Scale(Vector3.zero, obstacle.DesireScale, obstacle.transform, spawnAnimationDuration, spawnAnimationEase);
                }
            }

            //Collectables
            foreach (var collectableList in collectableDictionary.Values)
            {
                foreach (var collectable in collectableList)
                {
                    TweenManager.Scale(Vector3.zero, Vector3.one, collectable.transform, spawnAnimationDuration, spawnAnimationEase);
                }
            }

            OnCompleteSpawnAnimation?.Invoke();
            AdController.GetInstance.ShowBanner();
        }

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
                EnsureCollectableList(collectable.type);
                SpawnCollectable(collectable);
            }
        }
        private void SpawnCollectable(CollectableData data)
        {
            Collectable collectable = GetCollectableFromPool(data.type);
            if (collectable == null) return;

            collectable.Init(data);
            collectableDictionary[data.type].Add(collectable);
        }
        private void SpawnStartPoint(Vector3 pos, Vector3 rot)
        {
            startPointBehaviour = poolManager.StartPointPool.GetObject().GetComponent<StartPointBehaviour>();
            startPointBehaviour.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
            startPointBehaviour.Init();
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
