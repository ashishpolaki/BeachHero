using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;

        private StartPointBehaviour startPointBehaviour;
        private Player player;
        private List<SavedCharacter> savedCharactersList = new List<SavedCharacter>();
        private Dictionary<ObstacleType, List<IObstacle>> obstaclesDictionary = new Dictionary<ObstacleType, List<IObstacle>>();
        private Dictionary<CollectableType,List<ICollectable>> collectableDictionary = new Dictionary<CollectableType, List<ICollectable>>();

        public void StartState(LevelSO levelSO)
        {
            // Load the level data
            SpawnStartPoint(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnPlayer(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnObstacles(levelSO.Obstacle);
            SpawnCollectables(levelSO.Collectables);
            SpawnSavedCharacters(levelSO.LevelTime, levelSO.SavedCharacters);
        }
        public void UpdateState()
        {
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
        }

        public void ResetState()
        {
            player.Reset();
        }

        #region Obstacles
        private void SpawnObstacles(ObstacleData obstacle)
        {
            SpawnStaticObstacles(obstacle);
            SpawnMoveableObstacles(obstacle);
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
                            SpawnEel();
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
            obstaclesDictionary[ObstacleType.Shark] = new List<IObstacle>() { shark };
            shark.Init(movingObstacleData);
        }
        private void SpawnEel()
        {

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

        #endregion

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
            Collectable coin = poolManager.CoinsPool.GetObject().GetComponent<Collectable>();
            coin.Init(collectableData);
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
        }
    }
}
