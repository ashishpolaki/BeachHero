#if CHEAT_CODE
using QFSW.QC;
using UnityEngine;

namespace BeachHero
{
    public class CheatCodes : MonoBehaviour
    {
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
