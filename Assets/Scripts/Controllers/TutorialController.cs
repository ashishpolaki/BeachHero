using System;
using UnityEngine;

namespace BeachHero
{
    public enum FTUETutorialType
    {
        None,
        TapAndDrag,    // Tap + drag to save
        RescueAll,       // Save all drowning characters
    }
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private FTUEConfigSO fTUEConfig;
        public event Action OnPlayerTapAction;
        public event Action OnPathDrawnAction;
        public event Action OnPowerupPressAction;

        public FTUETutorialType CurrentFTUEType { private set; get; }

        public bool IsFTUE(int levelNumber)
        {
            foreach (var item in fTUEConfig.entries)
            {
                if (item.levelNumber == levelNumber)
                {
                    CurrentFTUEType = item.tutorialType;
                    return true;
                }
            }
            return false;
        }
       
        public void OnPlayerTap()
        {
            OnPlayerTapAction?.Invoke();
        }
        public void OnPathDrawn()
        {
            OnPathDrawnAction?.Invoke();
        }
        public void OnPowerupPressed()
        {
            OnPowerupPressAction?.Invoke();
        }
    }
}
