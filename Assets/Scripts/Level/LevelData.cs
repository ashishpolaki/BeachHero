namespace BeachHero
{
    [System.Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public LevelVisualState State;
        public int StarsEarned;
        public int Score;

        public void SetState(LevelVisualState newState)
        {
            State = newState;
        }
    }
}
