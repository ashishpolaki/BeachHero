using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public bool IsUnlocked;
        public bool IsCompleted;
        public int StarsEarned;
        public Vector3 WorldPosition; // World position in Unity scene
    }
}
