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

        public void LoadLevel(LevelSO levelSO)
        {
            // Load the level data
            SpawnStartPoint(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnPlayer(levelSO.StartPointData.Position, levelSO.StartPointData.Rotation);
            SpawnObstacles(levelSO.Obstacle);
            SpawnCollectables();
            SpawnSavedCharacters(levelSO.LevelTime, levelSO.SavedCharacters);
        }

        #region Obstacles
        private void SpawnObstacles(ObstacleData obstacle)
        {
            SpawnStaticObstacles(obstacle);
            SpawnMoveableObstacles(obstacle);
        }
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
        private void SpawnMoveableObstacles(ObstacleData obstacle)
        {
            if (obstacle.MovingObstacles != null && obstacle.MovingObstacles.Length > 0)
            {
                foreach (var movingObstacle in obstacle.MovingObstacles)
                {
                    switch (movingObstacle.type)
                    {
                        case ObstacleType.Eel:
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
        private void SpawnEel()
        {

        }
        private void SpawnShark(MovingObstacleData movingObstacleData)
        {
            SharkObstacle shark = poolManager.SharkPool.GetObject().GetComponent<SharkObstacle>();
            obstaclesDictionary[ObstacleType.Shark] = new List<IObstacle>() { shark };
            shark.Init(movingObstacleData);
        }
        private void SpawnWaterHole()
        {

        }
        private void SpawnRock(ObstacleType obstacleType, StaticObstacleData rockObstacle)
        {
            obstaclesDictionary[obstacleType] = new List<IObstacle>();
            RockObstacle rock = poolManager.RockPool.GetObject().GetComponent<RockObstacle>();
            obstaclesDictionary[obstacleType].Add(rock);
            rock.transform.SetPositionAndRotation(rockObstacle.position, Quaternion.Euler(rockObstacle.rotation));
        }
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
        private void SpawnCollectables()
        {

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
