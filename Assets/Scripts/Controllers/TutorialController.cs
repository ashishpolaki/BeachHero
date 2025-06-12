using System;
using UnityEngine;

namespace BeachHero
{
    public class TutorialController : MonoBehaviour
    {
        [Tooltip("First Time User Experience Level Number")]
        private int ftueLevelNumber = 1;
        private int magnetPowerup_UnlockLevel = 2;
        private int speedBoost_UnlockLevel = 3;
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
            bool isUnlocked = SaveController.LoadBool(StringUtils.MAGNET_POWERUP, false);
            return isUnlocked;
        }
        public bool IsMagnetUnlockLevel(int levelNumber)
        {
            return levelNumber == magnetPowerup_UnlockLevel;
        }
        public bool IsSpeedBoostUnlockLevel(int levelNumber)
        {
            return levelNumber == speedBoost_UnlockLevel;
        }
        public bool IsSpeedBoostPowerupUnlocked()
        {
            bool isUnlocked = SaveController.LoadBool(StringUtils.SPEEDBOOST_POWERUP, false);
            return isUnlocked;
        }
    }
}
