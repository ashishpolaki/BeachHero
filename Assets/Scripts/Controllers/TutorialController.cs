using System;
using UnityEngine;

namespace BeachHero
{
    public class TutorialController : MonoBehaviour
    {
        [Tooltip("First Time User Experience Level Number")]
        private int ftueLevelNumber = 1;
        public event Action OnFTUEPlayerTouchAction;
        public event Action OnFTUEPathDrawnAction;
        public event Action OnPowerupPressAction;

        public bool IsFTUE(int levelNumber)
        {
            return ftueLevelNumber == levelNumber;
        }
        public void OnFTUEPlayerTouch()
        {
            OnFTUEPlayerTouchAction?.Invoke();
        }
        public void OnFTUEPathDrawn()
        {
            OnFTUEPathDrawnAction?.Invoke();
        }
        public void OnPowerupPressed()
        {
            OnPowerupPressAction?.Invoke();
        }
        public bool IsMagnetPowerupUnlocked()
        {
            bool isUnlocked = SaveSystem.LoadBool(StringUtils.MAGNET_UNLOCKED, false);
            return isUnlocked;
        }
        public bool IsMagnetUnlockLevel(int levelNumber)
        {
            return levelNumber == IntUtils.MAGNET_UNLOCK_LEVEL;
        }
        public bool IsSpeedBoostUnlockLevel(int levelNumber)
        {
            return levelNumber == IntUtils.SPEEDBOOST_UNLOCK_LEVEL;
        }
        public bool IsSpeedBoostPowerupUnlocked()
        {
            bool isUnlocked = SaveSystem.LoadBool(StringUtils.SPEEDBOOST_UNLOCKED, false);
            return isUnlocked;
        }
    }
}
