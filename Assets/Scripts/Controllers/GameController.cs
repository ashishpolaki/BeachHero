using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public enum GameState
    {
        NotStarted,
        Playing,
        Paused,
        LevelWin,
        LevelFail,
        Map
    }
    [System.Serializable]
    public struct LevelFailDelay
    {
        public LevelFailDelayType type;
        public float seconds;
    }
    public enum LevelFailDelayType
    {
        None,
        Short,
        Medium,
        Long
    }
    public class GameController : SingleTon<GameController>
    {
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private LevelController levelController;
        [SerializeField] private PoolController poolManager;
        [SerializeField] private PowerupController powerupController;
        [SerializeField] private StoreManager storeController;
        [SerializeField] private SkinController skinController;
        [SerializeField] private LevelFailDelay[] levelFailDelays;

        [Tooltip("The Index Starts from 0")]
        private int highestCompletedLevelIndex;

        private GameState gameState = GameState.NotStarted;
        private GameState previousGameState = GameState.NotStarted;

        #region Properties
        public GameState GameState => gameState;
        public GameState PreviousGameState => previousGameState;
        public int HighestCompletedLevelIndex => highestCompletedLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
        public PowerupController PowerupController => powerupController;
        public StoreManager StoreController => storeController;
        public SkinController SkinController => skinController;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (GameState == GameState.Playing || GameState == GameState.LevelWin)
            {
                levelController.UpdateState();
                if (EnvironmentController.GetInstance != null)
                {
                    EnvironmentController.GetInstance.UpdateWaterAnimation();
                }
            }
            if (GameState == GameState.Map)
            {
                if (MapController.GetInstance != null)
                {
                    MapController.GetInstance.UpdateState();
                }
            }
        }
        private void OnDestroy()
        {
            poolManager.Reset();
        }
        #endregion

        #region Initialization
        public void Init()
        {
            highestCompletedLevelIndex = LoadHighestCompletedLevelNumber() - 1;
            powerupController.Init();
            storeController.Init();
        }
        public void SpawnLevel()
        {
            highestCompletedLevelIndex = LoadHighestCompletedLevelNumber() - 1;
            InitializeLevel(highestCompletedLevelIndex);
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }
        private void InitializeLevel(int levelIndex)
        {
            SetGameState(GameState.NotStarted);
            CameraController.GetInstance.SetActiveCamera(GameCameraType.GameView);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(levelIndex),levelIndex );
            levelDatabaseSO.Init();
        }
        #endregion

        #region Game Flow
        public void StartGameplay()
        {
            SetGameState(GameState.Playing);
            CameraController.GetInstance.SetActiveCamera(GameCameraType.GameView);
            bool isFTUE = TutorialController.GetInstance.IsTutorial(levelController.CurrentLevelIndex + 1);
            ScreenTabType screenTabType = isFTUE ? ScreenTabType.LevelTutorial : ScreenTabType.None;
            levelController.InitializePlayerData(isFTUE);
            levelController.ResetAllSpawnedObjectsScale();
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open, screenTabType);
        }
        public void BackToMainMenu()
        {
            InitializeLevel(HighestCompletedLevelIndex);
        }
        public void RetryLevel()
        {
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(LevelController.CurrentLevelIndex), LevelController.CurrentLevelIndex);
            levelDatabaseSO.Init();
        }
        public void NextLevel()
        {
            InitializeLevel(HighestCompletedLevelIndex);
        }
        public void SkipLevel()
        {
            IncrementLevel();
            InitializeLevel(HighestCompletedLevelIndex);
        }
        private void IncrementLevel()
        {
            if (HighestCompletedLevelIndex + 1 >= levelDatabaseSO.TotalLevelsCount)
            {
                // If there are no more levels, stay on the current level.
                return;
            }
            if (LevelController.CurrentLevelIndex == HighestCompletedLevelIndex)
            {
                highestCompletedLevelIndex++;
                SaveSystem.SaveInt(StringUtils.HIGHEST_COMPLETED_LEVEL, highestCompletedLevelIndex + 1);
            }
        }
        public void OnLevelWin()
        {
            IncrementLevel();
            SetGameState(GameState.LevelWin);
            levelController.SetLevelCompletionResult(true);
        }
        public void OnLevelFailed(LevelFailDelayType levelFailDelayType)
        {
            // If the level is already passed, do not allow to fail again.
            if (GameState == GameState.LevelWin)
            {
                LevelWinFeedback();
                return;
            }
            StartCoroutine(IELevelFailed(levelFailDelayType));
        }
        IEnumerator IELevelFailed(LevelFailDelayType levelFailDelayType)
        {
            SetGameState(GameState.LevelFail);
            levelController.SetLevelCompletionResult(false);
            float delay = GetLevelFailDelayInSeconds(levelFailDelayType);
            yield return new WaitForSeconds(delay);
            LevelFailFeedback();
        }
        private void LevelFailFeedback()
        {
            AudioController.GetInstance.PlaySound(AudioType.LevelFailed);
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelFail);
        }
        public void LevelWinFeedback()
        {
            levelController.SetStarsForCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelWin);
            AudioController.GetInstance.PlaySound(AudioType.LevelWin);
        }
        private float GetLevelFailDelayInSeconds(LevelFailDelayType type)
        {
            foreach (LevelFailDelay delay in levelFailDelays)
            {
                if (delay.type == type)
                {
                    return delay.seconds;
                }
            }
            return 0f;
        }
        public void SetLevel(int levelIndex)
        {
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(levelIndex), levelIndex);
        }
        private int LoadHighestCompletedLevelNumber()
        {
            return SaveSystem.LoadInt(StringUtils.HIGHEST_COMPLETED_LEVEL, IntUtils.DEFAULT_LEVEL);
        }
        #endregion

        #region Collect
        public void OnGameCurrencyPickup()
        {
            levelController.OnGameCurrencyCollect();
        }
        #endregion

        #region Utilities
        public void SetGameState(GameState state)
        {
            previousGameState = gameState;
            gameState = state;
        }
        public void SetPreviousGameState()
        {
            gameState = previousGameState;
        }
        #endregion
    }
}
