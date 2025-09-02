using System.Collections.Generic;
using UnityEngine;
using System;

namespace BeachHero
{
    public enum PowerupType
    {
        Magnet,
        SpeedBoost
    }
    public class PowerupController : MonoBehaviour
    {
        #region Private variables
        private List<PowerupType> currentActivePowerupList = new List<PowerupType>();
        private int magnetBalance;
        private int speedBoostBalance;
        #endregion

        #region Actions
        public event Action OnMagnetBalanceChange;
        public event Action OnSpeedBoostBalanceChange;
        #endregion

        #region Properties
        public List<PowerupType> CurrentActivePowerupList => currentActivePowerupList;
        public int MagnetBalance
        {
            get => magnetBalance;
            private set
            {
                magnetBalance = value;
                SaveSystem.SaveInt(StringUtils.MAGNET_BALANCE, magnetBalance);
                OnMagnetBalanceChange?.Invoke();
            }
        }
        public int SpeedBoostBalance
        {
            get => speedBoostBalance;
            private set
            {
                speedBoostBalance = value;
                SaveSystem.SaveInt(StringUtils.SPEEDBOOST_BALANCE, speedBoostBalance);
                OnSpeedBoostBalanceChange?.Invoke();
            }
        }
        #endregion

        #region Init
        public void Init()
        {
            InitBalances();
        }
        private void InitBalances()
        {
            magnetBalance = SaveSystem.LoadInt(StringUtils.MAGNET_BALANCE, IntUtils.DEFAULT_MAGNET_BALANCE);
            speedBoostBalance = SaveSystem.LoadInt(StringUtils.SPEEDBOOST_BALANCE, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
        }
        #endregion

        #region Public Methods
        public void AddPowerupInList(PowerupType powerupType)
        {
            if (!currentActivePowerupList.Contains(powerupType))
            {
                currentActivePowerupList.Add(powerupType);
            }
        }
        public void RemovePowerupFromList(PowerupType powerupType)
        {
            if (currentActivePowerupList.Contains(powerupType))
            {
                currentActivePowerupList.Remove(powerupType);
            }
        }
        public void OnPowerupCollected(PowerupType powerupType,int count)
        {
            switch (powerupType)
            {
                case PowerupType.Magnet:
                    UpdateMagnetBalance(count);
                    break;
                case PowerupType.SpeedBoost:
                    UpdateSpeedBoostBalance(count);
                    break;
                default:
                    DebugUtils.LogError($"Powerup {powerupType} not recognized.");
                    break;
            }
        }
        public void ActivateSelectedPowerups()
        {
            foreach (var powerupType in currentActivePowerupList)
            {
                switch (powerupType)
                {
                    case PowerupType.Magnet when MagnetBalance > 0:
                        MagnetBalance--;
                        break;

                    case PowerupType.SpeedBoost when SpeedBoostBalance > 0:
                        SpeedBoostBalance--;
                        break;

                    default:
                        DebugUtils.LogError($"Powerup {powerupType} not recognized or balance is zero.");
                        break;
                }
            }
            currentActivePowerupList.Clear();
        }
        public void UpdateMagnetBalance(int count)
        {
            MagnetBalance += count;
        }
        public void UpdateSpeedBoostBalance(int count)
        {
            SpeedBoostBalance += count;
        }
        public int GetPowerupBalance(PowerupType powerupType)
        {
            return powerupType switch
            {
                PowerupType.Magnet => MagnetBalance,
                PowerupType.SpeedBoost => SpeedBoostBalance,
                _ => 0
            };
        }
        #endregion

        #region Lock/Unlock
        public bool IsUnlockLevelForPowerup(PowerupType powerupType, int levelNumber)
        {
            int unlockLevel = powerupType switch
            {
                PowerupType.Magnet => IntUtils.MAGNET_UNLOCK_LEVEL,
                PowerupType.SpeedBoost => IntUtils.SPEEDBOOST_UNLOCK_LEVEL,
                _ => -1
            };

            if (unlockLevel == -1)
            {
                Debug.LogError($" No unlock level defined for PowerupType: {powerupType}");
                return false;
            }

            return levelNumber == unlockLevel;
        }
        public bool IsPowerupUnlocked(PowerupType powerupType)
        {
            string key = powerupType switch
            {
                PowerupType.Magnet => StringUtils.MAGNET_UNLOCKED,
                PowerupType.SpeedBoost => StringUtils.SPEEDBOOST_UNLOCKED,
                _ => null
            };

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($" No unlock key defined for PowerupType: {powerupType}");
                return false; // default safe value
            }

            return SaveSystem.LoadBool(key, false);
        }
        public void UnlockPowerup(PowerupType powerupType)
        {
            switch (powerupType)
            {
                case PowerupType.Magnet:
                    SaveSystem.SaveBool(StringUtils.MAGNET_UNLOCKED, true);
                    break;
                case PowerupType.SpeedBoost:
                    SaveSystem.SaveBool(StringUtils.SPEEDBOOST_UNLOCKED, true);
                    break;
                default:
                    DebugUtils.LogError($"Powerup {powerupType} not recognized.");
                    break;
            }
        }
        #endregion
    }
}
