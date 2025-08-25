using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public enum BoatOffsetDirection
    {
        Left,
        Right,
    }
    [System.Serializable]
    public struct MapData
    {
        public int index;
        public int mapNumber;
        public string name;
        public string description;
        public int startLevelNumber;
        public int endLevelNumber;

        public BoatOffsetDirection boatOffsetDirection;
        public GameObject mapObject;
        public Transform levelsParent;
        public LineRenderer pathLine;

        public List<BezierPoint> points;
        public List<LevelVisual> levelVisuals;

        public void CalculateOffsetDirection(Transform target, out Vector3 Dir)
        {
            Dir = Vector3.zero;
            switch (boatOffsetDirection)
            {
                case BoatOffsetDirection.Left:
                    Dir = -target.right;
                    break;
                case BoatOffsetDirection.Right:
                    Dir = target.right;
                    break;
                default:
                    break;
            }
        }

        public void CalculateOffsetDirectionFromCross(Vector3 forward, out Vector3 Dir)
        {
            Dir = Vector3.zero;
            switch (boatOffsetDirection)
            {
                case BoatOffsetDirection.Left:
                    Dir = Vector3.Cross(forward, Vector3.back).normalized;
                    break;
                case BoatOffsetDirection.Right:
                    Dir = Vector3.Cross(forward, Vector3.forward).normalized;
                    break;
            }
        }

        public void LevelSetup(LevelDatabaseSO levelDatabaseSO)
        {
            int startLevelIndex = startLevelNumber - 1;
            int endLevelIndex = endLevelNumber - 1;
            int index = 0;
            //If startLevelNumber is 0, it means no levels are set up for this map
            if (startLevelNumber > 0)
            {
                for (var i = startLevelIndex; i <= endLevelIndex; i++)
                {
                    levelVisuals[index].Setup(levelDatabaseSO.LevelDatas[i]);
                    index++;
                }
            }
        }
        public LevelVisual GetLevelVisual(int levelNumber)
        {
            foreach (var levelVisual in levelVisuals)
            {
                if (levelVisual.LevelNumber == levelNumber)
                {
                    return levelVisual;
                }
            }
            return null;
        }
        public int GetCurrentLevelIndex(int levelNumber)
        {
            for (var i = 0; i < levelVisuals.Count; i++)
            {
                if (levelVisuals[i].LevelNumber == levelNumber)
                {
                    int index = i;
                    return index;
                }
            }
            return 0;
        }
    }
}
