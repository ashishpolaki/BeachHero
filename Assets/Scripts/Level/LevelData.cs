using UnityEngine.Serialization;

namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public LevelVisualState State;
        public int StarsEarned;
        public void SetState(LevelVisualState newState)
        {
            State = newState;
        }
    }
}
