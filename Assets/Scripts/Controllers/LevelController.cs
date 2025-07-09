using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class LevelController : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private PoolController poolManager;

        [SerializeField] private LayerMask startPointLayer;
        [SerializeField] private LayerMask touchLayer;
        [SerializeField] private float minTrailPointsDistance = 0.3f;
        [SerializeField] private float smoothingStep = 0.05f;
        [SerializeField] private float spacing = 0.5f;
        [SerializeField] private float magnetRadius = 5f;
        #endregion

        #region Private Variables
        private StartPointBehaviour startPointBehaviour;
        private Player player;
        private List<DrownCharacter> savedCharactersList = new List<DrownCharacter>();
        private Dictionary<ObstacleType, List<Obstacle>> obstaclesDictionary = new Dictionary<ObstacleType, List<Obstacle>>();
        private Dictionary<CollectableType, List<Collectable>> collectableDictionary = new Dictionary<CollectableType, List<Collectable>>();
        private PathTrail playerPathDrawTrail;
        private PathTrail playerPathTransparentTrail;

        private Camera cam;
        private Ray ray;
        private RaycastHit raycastHit;
        private Vector3 lastTrailPoint;
        private List<Vector3> drawnPoints = new List<Vector3>();
        private List<Vector3> smoothedDrawnPoints = new List<Vector3>();
        private bool isPathDrawn = false;
        private bool canDrawPath = false;
        private bool isPlaying;
        private bool coinMagnetActivated;
        private bool isLevelCompleted;
        private bool isLevelPassed;
        private bool isSimulationStarted;
        private bool isFTUE = false; // For First Time User Experience
        private int totalCharactersInLevel;
        [Tooltip("Number of characters saved by the player in current level")]
        private int savedCharacterCounter;
        #endregion

        #region Properties
        public Transform PlayerTransform => player != null ? player.transform : null;

        public Transform DrownCharacter => savedCharactersList.Count > 0 ? savedCharactersList[0].transform : null;

        public bool IsLevelPassed => isLevelPassed;

        public Camera Cam
        {
            get
            {
                if (cam == null)
                {
                    cam = Camera.main;
                }
                return cam;
            }
        }
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
            if (!isPathDrawn && isPlaying)
            {
                playerPathDrawTrail.ResetTrail(player.transform.position);
                ray = Cam.ScreenPointToRay(position);
                if (Physics.Raycast(ray, out raycastHit, 1000f, startPointLayer))
                {
                    canDrawPath = true;
                    if (isFTUE)
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
            if (isPlaying && !isPathDrawn)
            {
                isPathDrawn = true;
                canDrawPath = false;
                if (drawnPoints.Count >= 4)
                {
                    smoothedDrawnPoints = CatmullSplineUtils.GetEvenlySpacedPoints(drawnPoints, spacing);
                    StartSimulation();
                }
                else
                {
                    isPathDrawn = false;
                    canDrawPath = false;
                    drawnPoints.Clear();
                }
            }
        }
        #endregion

        #region DrawPath
        private List<Vector3> curvePoints = new List<Vector3>();

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
            if (isPlaying && canDrawPath)
            {
                ray = Cam.ScreenPointToRay(InputManager.MousePosition);
                if (Physics.Raycast(ray, out raycastHit, 1000f, touchLayer))
                {
                    if (raycastHit.collider.CompareTag("Ground"))
                    {
                        UpdatePath(raycastHit.point);
                    }
                    else
                    {
                        Vector3 hitpoint = raycastHit.point;
                        hitpoint.y = 0f;
                        UpdatePath(hitpoint);
                    }
                }
            }
        }
        #endregion

        private void StartSimulation()
        {
            player.StartMovement(smoothedDrawnPoints.ToArray());
            isSimulationStarted = true;
            if (isFTUE)
            {
                GameController.GetInstance.TutorialController.OnFTUEPathDrawn();
            }
        }

        public void GameStart(bool _isFTUE)
        {
            isFTUE = _isFTUE;
            isPlaying = true;
            int boatIndex = GameController.GetInstance.SkinController.GetCurrentSelectedBoatIndex();
            float speed = GameController.GetInstance.SkinController.GetSpeed();
            GameObject boatPRefab = GameController.GetInstance.SkinController.GetCurrentSelectedBoat();
            player.GameStart(boatIndex, speed, boatPRefab);
        }

        public void OnLevelCompleted(bool _val)
        {
            isLevelCompleted = true;
            isLevelPassed = _val;

            if (!isLevelPassed)
            {
                player.StopMovement();
            }
        }

        public void OnCharacterPickUp()
        {
            savedCharacterCounter++;
            if (savedCharacterCounter >= totalCharactersInLevel)
            {
                isPlaying = false;
                GameController.GetInstance.OnLevelPass();
                UIController.GetInstance.ScreenEvent(ScreenType.GameWin, UIScreenEvent.Open);
            }
        }

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
                    if (collectable.CollectableType == CollectableType.Coin)
                    {
                        poolManager.CoinsPool.ReturnObject(collectable.gameObject);
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

        #region Powerup
        public void OnActivatePowerup(PowerupType powerUpType)
        {
            if (powerUpType == PowerupType.Magnet)
            {
                ActivateCoinMagnetPowerup();
            }
            else if (powerUpType == PowerupType.SpeedBoost)
            {
                ActivateSpeedPowerup();
            }
        }

        public void ActivateCoinMagnetPowerup()
        {
            coinMagnetActivated = true;
            player.ActivateCoinMagnetPowerup();
        }

        public void ActivateSpeedPowerup()
        {
            player.ActivateSpeedPowerup();
        }

        private void OnCoinMagnetPowerup()
        {
            if (coinMagnetActivated)
            {
                if (collectableDictionary != null && collectableDictionary.ContainsKey(CollectableType.Coin))
                    foreach (var coinCollectable in collectableDictionary[CollectableType.Coin])
                    {
                        float distance = Vector3.Distance(player.transform.position, coinCollectable.transform.position);
                        CoinCollectable coin = (CoinCollectable)coinCollectable;
                        if (!coin.CanMoveToTarget)
                        {
                            if (distance <= magnetRadius)
                            {
                                coin.SetTarget(player.transform);
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
            totalCharactersInLevel = levelSO.DrownCharacters.Length;

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
            // Update Path 
            DrawPath();

            //Update Obstacles
            foreach (var obstacleList in obstaclesDictionary.Values)
            {
                foreach (var obstacle in obstacleList)
                {
                    obstacle.UpdateState();
                }
            }

            // Start the Simulation after the path is drawn
            if (!isSimulationStarted)
            {
                return;
            }

            //Update Player
            if (player != null)
            {
                player.UpdateState();
            }
            //Update Collectables
            OnCoinMagnetPowerup();
            foreach (var collectableList in collectableDictionary.Values)
            {
                foreach (var collectable in collectableList)
                {
                    collectable.UpdateState();
                }
            }

            if (!isLevelCompleted)
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
            ReturnToPoolEverything();
            coinMagnetActivated = false;
            isLevelCompleted = false;
            isLevelPassed = false;
            isPlaying = false;
            isSimulationStarted = false;
            isPathDrawn = false;
            canDrawPath = false;
            drawnPoints.Clear();
            curvePoints.Clear();
            smoothedDrawnPoints.Clear();
            lastTrailPoint = Vector3.zero;
            savedCharacterCounter = 0;
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
                    case CollectableType.Coin:
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
            Collectable collectable = poolManager.CoinsPool.GetObject().GetComponent<Collectable>();
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
