using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class LevelController : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private PoolController poolManager;
        [SerializeField] private InputManager inputManager;

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
        private List<SavedCharacter> savedCharactersList = new List<SavedCharacter>();
        private Dictionary<ObstacleType, List<IObstacle>> obstaclesDictionary = new Dictionary<ObstacleType, List<IObstacle>>();
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
        private bool isLevelFinished;
        private bool levelPassed;
        private float levelTimer;
        private int totalCharactersInLevel;
        [Tooltip("Number of characters saved by the player in current level")]
        private int savedCharacterCounter;
        #endregion

        #region Properties
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

        #region Actions
        public Action<float> OnLevelTimerUpdate;

        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            inputManager.OnMouseClickDown += OnMouseClickDown;
            inputManager.OnMouseClickUp += OnMouseClickUp;
        }
        private void OnDisable()
        {
            inputManager.OnMouseClickDown -= OnMouseClickDown;
            inputManager.OnMouseClickUp -= OnMouseClickUp;
        }
        #endregion

        #region Mouse Methods
        private void OnMouseClickDown(Vector2 position)
        {
            //Dont draw the path more than once
            if (!isPathDrawn && isPlaying)
            {
                playerPathDrawTrail.ResetTrail(player.transform.position);
                ray = Cam.ScreenPointToRay(InputManager.MousePosition);
                if (Physics.Raycast(ray, out raycastHit, 1000f, startPointLayer))
                {
                    canDrawPath = true;
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
                    smoothedDrawnPoints = GetEvenlySpacedPoints(drawnPoints, spacing);
                    StartSimulation();
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
                        Vector3 interpolatedPoint = CatmullRom(
                            drawnPoints[drawnPoints.Count - 4], // P0
                            drawnPoints[drawnPoints.Count - 3], // P1
                            drawnPoints[drawnPoints.Count - 2], // P2
                            drawnPoints[drawnPoints.Count - 1], // P3
                            t
                        );

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
        }

        public void GameStart()
        {
            isPlaying = true;
        }
        private void UpdateLevelTimer()
        {
            levelTimer -= Time.deltaTime;
            if (levelTimer <= 0)
            {
                levelTimer = 0;
                isLevelFinished = true;
            }
            OnLevelTimerUpdate?.Invoke(levelTimer);
        }

        #region Powerup
        public void ActivateCoinMagnetPowerup()
        {
            coinMagnetActivated = true;
        }
        public void ActivateSpeedPowerup()
        {
            player.ActivateSpeedPowerup();
        }
        private void OnCoinMagnetPowerup()
        {
            if (coinMagnetActivated)
            {
                foreach (var coinCollectable in collectableDictionary[CollectableType.Coin])
                {
                    float distance = Vector3.Distance(player.transform.position, coinCollectable.transform.position);
                    Coin coin = (Coin)coinCollectable;
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
            levelTimer = levelSO.LevelTime;
            totalCharactersInLevel = levelSO.SavedCharacters.Length;

            // Load the level data
            SpawnStartPoint(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnPlayer(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnObstacles(levelSO.Obstacle);
            SpawnCollectables(levelSO.Collectables);
            SpawnSavedCharacters(levelSO.LevelTime, levelSO.SavedCharacters);
            SpawnTrails();
        }

        public void UpdateState()
        {
            // Update level timer
            UpdateLevelTimer();

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
            //Update Characters
            foreach (var savedCharacter in savedCharactersList)
            {
                savedCharacter.UpdateState(Time.deltaTime);
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
        }
        public void ResetState()
        {
            player.ResetState();
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
                    switch (movingObstacle.type)
                    {
                        case ObstacleType.Eel:
                            SpawnEel(movingObstacle);
                            break;
                        case ObstacleType.Shark:
                            SpawnShark(movingObstacle);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void SpawnShark(MovingObstacleData movingObstacleData)
        {
            SharkObstacle shark = poolManager.SharkPool.GetObject().GetComponent<SharkObstacle>();
            if (!obstaclesDictionary.ContainsKey(ObstacleType.Shark))
            {
                obstaclesDictionary[ObstacleType.Shark] = new List<IObstacle>();
            }
            obstaclesDictionary[ObstacleType.Shark].Add(shark);
            shark.Init(movingObstacleData);
        }
        private void SpawnEel(MovingObstacleData movingObstacleData)
        {
            Eel eel = poolManager.EelPool.GetObject().GetComponent<Eel>();
            if (!obstaclesDictionary.ContainsKey(ObstacleType.Eel))
            {
                obstaclesDictionary[ObstacleType.Eel] = new List<IObstacle>();
            }
            obstaclesDictionary[ObstacleType.Eel].Add(eel);
            eel.Init(movingObstacleData);
        }
        #endregion

        #region Static obstacle
        private void SpawnStaticObstacles(ObstacleData obstacle)
        {
            if (obstacle.StaticObstacles != null && obstacle.StaticObstacles.Length > 0)
            {
                foreach (var staticObstacle in obstacle.StaticObstacles)
                {
                    switch (staticObstacle.type)
                    {
                        case ObstacleType.Rock:
                            SpawnRock(staticObstacle.type, staticObstacle);
                            break;
                        case ObstacleType.WaterHole:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void SpawnRock(ObstacleType obstacleType, StaticObstacleData rockObstacle)
        {
            obstaclesDictionary[obstacleType] = new List<IObstacle>();
            RockObstacle rock = poolManager.RockPool.GetObject().GetComponent<RockObstacle>();
            obstaclesDictionary[obstacleType].Add(rock);
            rock.transform.SetPositionAndRotation(rockObstacle.position, Quaternion.Euler(rockObstacle.rotation));
        }
        private void SpawnWaterHole()
        {

        }
        #endregion

        #region WaterHole Obstacle
        private void SpawnWaterHoleObstacle(WaterHoleObstacleData[] waterHoleObstacleData)
        {
            if (waterHoleObstacleData != null && waterHoleObstacleData.Length > 0)
            {
                foreach (var waterHole in waterHoleObstacleData)
                {
                    obstaclesDictionary[ObstacleType.WaterHole] = new List<IObstacle>();
                    WaterHoleObstacle waterHoleObstacle = poolManager.WaterHolePool.GetObject().GetComponent<WaterHoleObstacle>();
                    obstaclesDictionary[ObstacleType.WaterHole].Add(waterHoleObstacle);
                    waterHoleObstacle.transform.position = waterHole.position;
                    waterHoleObstacle.Init(waterHole);
                }
            }
        }

        #endregion

        #endregion

        #region Spawn
        private void SpawnTrails()
        {
            playerPathDrawTrail = poolManager.PathTrailPool.GetObject().GetComponent<PathTrail>();
        }
        private void SpawnSavedCharacters(float levelTime, SavedCharacterData[] savedCharacterDatas)
        {
            foreach (var savedCharacterData in savedCharacterDatas)
            {
                var savedCharacter = poolManager.SavedCharacterPool.GetObject().GetComponent<SavedCharacter>();
                savedCharacter.Init(savedCharacterData.Position, savedCharacterData.WaitTimePercentage, levelTime);
                savedCharactersList.Add(savedCharacter);
            }
        }
        private void SpawnCollectables(CollectableData[] collectableDatas)
        {
            foreach (var collectable in collectableDatas)
            {
                switch (collectable.type)
                {
                    case CollectableType.Coin:
                        SpawnCoin(collectable);
                        break;
                    case CollectableType.Gem:
                        break;
                    case CollectableType.Magnet:
                        break;
                    case CollectableType.Speed:
                        break;
                    default:
                        break;
                }
            }
        }
        private void SpawnCoin(CollectableData collectableData)
        {
            Collectable collectable = poolManager.CoinsPool.GetObject().GetComponent<Collectable>();
            collectable.Init(collectableData);
            if (!collectableDictionary.ContainsKey(collectableData.type))
            {
                collectableDictionary[collectableData.type] = new List<Collectable>();
            }
            collectableDictionary[CollectableType.Coin].Add(collectable); 
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
            player.StartState();
        }
        #endregion

        #region Spline

        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Catmull-Rom spline formula
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        private List<Vector3> GetEvenlySpacedPoints(List<Vector3> pathPoints, float spacing)
        {
            List<Vector3> evenlySpacedPoints = new List<Vector3>();
            float distanceSinceLastPoint = 0f;

            evenlySpacedPoints.Add(pathPoints[0]); // Start with the first point

            for (int i = 0; i < pathPoints.Count - 3; i++)
            {
                Vector3 previousPoint = pathPoints[i + 1]; // Start from the second control point

                for (float t = 0; t <= 1; t += 0.01f) // High resolution for accurate arc length calculation
                {
                    Vector3 interpolatedPoint = CatmullRom(
                        pathPoints[i],
                        pathPoints[i + 1],
                        pathPoints[i + 2],
                        pathPoints[i + 3],
                        t
                    );

                    // Accumulate distance between the previous point and the current interpolated point
                    distanceSinceLastPoint += Vector3.Distance(previousPoint, interpolatedPoint);

                    // If the accumulated distance exceeds the spacing, add a new point
                    if (distanceSinceLastPoint >= spacing)
                    {
                        evenlySpacedPoints.Add(interpolatedPoint);
                        distanceSinceLastPoint = 0f; // Reset the distance counter
                    }

                    previousPoint = interpolatedPoint; // Update the previous point
                }
            }

            if (evenlySpacedPoints.Contains(pathPoints[pathPoints.Count - 1]) == false)
                evenlySpacedPoints.Add(pathPoints[pathPoints.Count - 1]); // Add the last point if not already added

            return evenlySpacedPoints;
        }
        #endregion
    }
}
