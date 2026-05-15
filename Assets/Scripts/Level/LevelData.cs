namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public LevelVisualState State;
        public int MedalsEarned;
        public void SetState(LevelVisualState newState)
        {
            State = newState;
        }
    }
}
