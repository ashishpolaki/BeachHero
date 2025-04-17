using UnityEngine;
using Bokka.SkinStore;

namespace Bokka.BeachRescue
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;
        public static GameController Instance { get => instance; }

        [SerializeField] LevelsDatabase levelsDatabase;
        public LevelsDatabase LevelsDatabase => instance.levelsDatabase;

        [SerializeField] UIController uiController;
        [SerializeField] MusicSource musicSource;
        [SerializeField] ParticleSystem levelCompleteParticleSystem;

        private static LevelController levelController;
        private static SkinsController skinController;
        private static SkinStoreController skinStoreController;

        private static SimpleIntSave levelIndexSave;
        public static int CurrentLevelIndex
        {
            get => levelIndexSave.Value;
            private set => levelIndexSave.Value = value;
        }

        private UIMainMenu mainMenu;

        private void Awake()
        {
            instance = this;

            levelIndexSave = SaveController.GetSaveObject<SimpleIntSave>("Level Number");

            // Cache components
            CacheComponent(out levelController);
            CacheComponent(out skinController);
            CacheComponent(out skinStoreController);
        }

        private void Start()
        {
            musicSource.Init();
            musicSource.Activate();

            uiController.Init();

            skinController.Init();
            skinStoreController.Init(skinController);

            levelsDatabase.Init();
            levelController.Init();

            uiController.InitPages();

            mainMenu = UIController.GetPage<UIMainMenu>();

            LoadGame();

            GameLoading.MarkAsReadyToHide();
        }

        private static void LoadGame()
        {
            levelController.LoadLevel(instance.levelsDatabase.GetLevelByIndex(CurrentLevelIndex));

            SavePresets.CreateSave("Level " + (CurrentLevelIndex + 1).ToString("000"));

            UIController.ShowPage<UIMainMenu>();
        }

        public static void StartGame()
        {
            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIGame>();

            levelController.StartGame();
        }

        public void LevelComplete()
        {
            levelCompleteParticleSystem.PlayCase();

            CurrentLevelIndex++;

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();
        }

        public static void OnLevelCompletePanelClosed()
        {
            UIController.HidePage<UIComplete>(() =>
            {
                UIController.ShowPage<UIMainMenu>();

                AdsManager.ShowInterstitial(null);
            });

            LevelController.ApplyReward();
            SaveController.MarkAsSaveIsRequired();

            LoadGame();
        }

        public void OnReplayButtonPressed()
        {
            CurrentLevelIndex--;

            UIController.HidePage<UIComplete>(() =>
            {
                UIController.ShowPage<UIMainMenu>();

                AdsManager.ShowInterstitial(null);
            });

            LoadGame();
        }

        public void LevelFailed()
        {
            UIController.ShowPage<UIGameOver>();
        }

        public static void OnLevelFailedPanelClosed()
        {
            UIController.HidePage<UIGameOver>();
            UIController.HidePage<UIGame>();

            AdsManager.ShowInterstitial(null);

            LoadGame();
        }

        public void OnSuccessfullySkippedLevel()
        {
            CurrentLevelIndex++;

            UIController.HidePage<UIGameOver>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });

            LoadGame();
        }

        public void SkipLevelFromGameplay()
        {
            LevelController.Instance.OnLevelBeingSkiped();

            CurrentLevelIndex++;

            UIController.HidePage<UIGame>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });

            LoadGame();
        }

        #region Developement

        public static void FirstLevelDev()
        {
            CurrentLevelIndex = 0;

            LoadGame();
        }

        public static void NextLevelDev()
        {
            CurrentLevelIndex++;

            LoadGame();
        }

        public static void PrevLevelDev()
        {
            CurrentLevelIndex--;
            if (CurrentLevelIndex <= 0)
            {
                CurrentLevelIndex = instance.levelsDatabase.LevelsAmount;
            }

            LoadGame();
        }
        #endregion

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}