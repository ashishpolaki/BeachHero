#if CHEAT_CODE
using QFSW.QC;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class CheatCodes : MonoBehaviour
    {
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

        [Command]
        public static void WinLevel()
        {
            GameController.GetInstance.OnLevelWin();
        }

        [Command("set-level")]
        public static void SetLevel(int levelNumber)
        {
            SaveSystem.SaveInt(StringUtils.LEVELNUMBER, levelNumber);
            GameController.GetInstance.SpawnLevel();
        }
    }
}
#endif
