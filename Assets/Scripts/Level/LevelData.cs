using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public bool IsCurrentLevel;
        public bool IsCompleted;
        public int MedalsEarned;

        public void MarkComplete()
        {
            IsCurrentLevel = false;
            IsCompleted = true;
        }

        public void MarkIncomplete()
        {
            IsCurrentLevel = false;
            IsCompleted = false;
        }

        public void MarkCurrentLevel()
        {
            IsCurrentLevel = true;
            IsCompleted = false;
        }
    }
}
