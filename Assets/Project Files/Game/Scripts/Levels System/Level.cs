using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BeachRescue
{
    public class Level : ScriptableObject
    {
        [SerializeField] Vector3 startPoint;
        public Vector3 StartPoint => startPoint;

        [SerializeField] List<CharacterSave> charactersList;
        public List<CharacterSave> CharactersList { get => charactersList; }

        [SerializeField] List<MovingObstacleSave> movingObstaclesList;
        public List<MovingObstacleSave> MovingObstaclesList { get => movingObstaclesList; }

        [SerializeField] List<ItemSave> itemsList;
        public List<ItemSave> ItemsList { get => itemsList; }

        [SerializeField] float maxWaitingDuration = 5f;
        public float MaxWaitingDuration => maxWaitingDuration;

        public Level()
        {
            charactersList = new List<CharacterSave>();
            itemsList = new List<ItemSave>();
        }

        public Level(List<ItemSave> itemsList, Vector3 startPoint, List<CharacterSave> charactersList, List<MovingObstacleSave> movingObstaclesList, Vector3 cameraPosition, Vector3 cameraRotation, float waitingTimeLength)
        {
            this.itemsList = itemsList;
            this.startPoint = startPoint;
            this.charactersList = charactersList;
            this.movingObstaclesList = movingObstaclesList;
            maxWaitingDuration = waitingTimeLength;
        }
    }
}