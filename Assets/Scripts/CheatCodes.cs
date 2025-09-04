#if CHEAT_CODE
using QFSW.QC;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class CheatCodes : MonoBehaviour
    {
        #region Tap Counter
        public Button tapButton;
        public int requiredTaps = 3;
        private bool activate;
        private int tapCounter;

        private void OnEnable()
        {
            tapButton.onClick.AddListener(RegisterTap);
            QuantumConsole.Instance.OnDeactivate += () =>
            {
                activate = false;
                tapCounter = 0;
            };
        }
        private void OnDisable()
        {
            tapButton.onClick.RemoveListener(RegisterTap);
            QuantumConsole.Instance.OnDeactivate -= () =>
            {
                activate = false;
                tapCounter = 0;
            };
        }
        private void RegisterTap()
        {
            tapCounter++;
            if (tapCounter >= requiredTaps)
            {
                tapCounter = 0;
                activate = !activate;
                if (activate)
                {
                    QuantumConsole.Instance.Activate();
                }
                else
                {
                    QuantumConsole.Instance.Deactivate();
                }
            }
        }
        #endregion

        #region FPS
        public GameObject fpsObject;
        [Command("enable-fps")]
        public void EnableFPSCounter(bool val)
        {
            fpsObject.SetActive(val);
        }
        #endregion

        #region Commands
        [Command("unlock-powerups")]
        public static void UnlockPowerups()
        {
            GameController.GetInstance.PowerupController.UnlockPowerup(PowerupType.Magnet);
            GameController.GetInstance.PowerupController.UnlockPowerup(PowerupType.SpeedBoost);
        }

        [Command("level-win")]
        public static void WinLevel()
        {
            GameController.GetInstance.OnLevelWin();
            GameController.GetInstance.LevelController.PlayerTransform.GetComponent<Player>().PlayVictoryAnimation();
        }

        [Command("level-fail")]
        public static void LoseLevel()
        {
            GameController.GetInstance.OnLevelFailed();
        }

        [Command("force-set-level")]
        public static void SetLevel(int levelNumber)
        {
            SaveSystem.SaveInt(StringUtils.LEVELNUMBER, levelNumber);
            SaveSystem.SaveBool(StringUtils.MAGNET_UNLOCKED, false);
            SaveSystem.SaveBool(StringUtils.SPEEDBOOST_UNLOCKED, false);
            SaveSystem.SaveInt(StringUtils.MAGNET_BALANCE, IntUtils.DEFAULT_MAGNET_BALANCE);
            SaveSystem.SaveInt(StringUtils.SPEEDBOOST_BALANCE, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
            GameController.GetInstance.PowerupController.UpdateMagnetBalance(IntUtils.DEFAULT_MAGNET_BALANCE);
            GameController.GetInstance.PowerupController.UpdateSpeedBoostBalance(IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
            MapController.GetInstance.Awake();
            GameController.GetInstance.SpawnLevel();
        }

        [Command("add-star-fish")]
        public static void AddStarFish(int amount)
        {
            GameController.GetInstance.StoreController.IncrementGameCurrencyBalance(amount);
        }

        [Command("add-magnets")]
        public static void AddMagnets(int amount)
        {
            GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.Magnet, amount);
        }
        [Command("add-speed-boosts")]
        public static void AddSpeedBoosts(int amount)
        {
            GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.SpeedBoost, amount);
        }
        #endregion
    }
}
#endif
