using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public bool IsCurrentLevel;
        public bool IsCompleted;
        public int StarsEarned;
        public Vector3 WorldPosition; // World position in Unity scene


        public void MarkComplete()
        {
            IsCurrentLevel = false;
            IsCompleted = true;
        }

        public void MarkCurrentLevel()
        {
            IsCurrentLevel = true;
            IsCompleted = false;
        }
    }
}
