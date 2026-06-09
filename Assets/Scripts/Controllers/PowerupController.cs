using System.Collections.Generic;
using UnityEngine;
using System;

namespace BeachHero
{
    public enum PowerupType
    {
        SpeedBoost,
        Shield,
        None
    }
    public class PowerupController : MonoBehaviour
    {
        #region Private variables
        private List<PowerupType> currentActivePowerupList = new List<PowerupType>();
        private int speedBoostBalance;
        private int shieldBalance;
        #endregion

        #region Actions
        public event Action<PowerupType> OnBalanceChange;
        public event Action<PowerupType> OnActivatePowerup;
        #endregion

        #region Properties
        public List<PowerupType> CurrentActivePowerupList => currentActivePowerupList;
        public int SpeedBoostBalance
        {
            get => speedBoostBalance;
            private set
            {
                speedBoostBalance = value;
                SaveSystem.SaveInt(StringUtils.SPEEDBOOST_BALANCE, speedBoostBalance);
                OnBalanceChange?.Invoke(PowerupType.SpeedBoost);
            }
        }
        public int ShieldBalance
        {
            get => shieldBalance;
            private set
            {
                shieldBalance = value;
                SaveSystem.SaveInt(StringUtils.SHIELD_BALANCE, shieldBalance);
                OnBalanceChange?.Invoke(PowerupType.Shield);
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
            speedBoostBalance = SaveSystem.LoadInt(StringUtils.SPEEDBOOST_BALANCE, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
            shieldBalance = SaveSystem.LoadInt(StringUtils.SHIELD_BALANCE, IntUtils.DEFAULT_SHIELD_BALANCE);
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
        public void OnPowerupCollected(PowerupType powerupType, int count)
        {
            switch (powerupType)
            {
                case PowerupType.Shield:
                    UpdatePowerupBalance(powerupType, count);
                    break;
                case PowerupType.SpeedBoost:
                    UpdatePowerupBalance(powerupType, count);
                    break;
                default:
                    DebugUtils.LogError($"Powerup {powerupType} not recognized.");
                    break;
            }
        }
        public void ActivateSelectedPowerups()
        {
            if (currentActivePowerupList.Count <= 0)
                return;

            foreach (var powerupType in currentActivePowerupList)
            {
                switch (powerupType)
                {
                    case PowerupType.SpeedBoost when SpeedBoostBalance > 0:
                        SpeedBoostBalance--;
                        OnActivatePowerup?.Invoke(powerupType);
                        break;

                    case PowerupType.Shield when ShieldBalance > 0:
                        ShieldBalance--;
                        OnActivatePowerup?.Invoke(powerupType);
                        break;

                    default:
                        DebugUtils.LogError($"Powerup {powerupType} not recognized or balance is zero.");
                        break;
                }
            }
            currentActivePowerupList.Clear();
        }

        public void UpdatePowerupBalance(PowerupType powerupType, int count)
        {
            switch (powerupType)
            {
                case PowerupType.SpeedBoost:
                    SpeedBoostBalance += count;
                    break;
                case PowerupType.Shield:
                    ShieldBalance += count;
                    break;
                default:
                    break;
            }
        }
        public int GetPowerupBalance(PowerupType powerupType)
        {
            return powerupType switch
            {
                PowerupType.SpeedBoost => SpeedBoostBalance,
                PowerupType.Shield => ShieldBalance,
                _ => 0
            };
        }
        #endregion

        #region Lock/Unlock
        public bool IsCurrentLevelUnlocksPowerup()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1; // +1 because level index is 0-based
            return IsUnlockLevelForPowerup(PowerupType.Shield, currentLevelNumber) ||
                   IsUnlockLevelForPowerup(PowerupType.SpeedBoost, currentLevelNumber);
        }
        public bool IsUnlockLevelForPowerup(PowerupType powerupType, int levelNumber)
        {
            int unlockLevel = powerupType switch
            {
                PowerupType.Shield => RemoteConfig.GetInstance.ShieldUnlockLevel,
                PowerupType.SpeedBoost => RemoteConfig.GetInstance.SpeedBoostUnlockLevel,
                _ => -1
            };

            if (unlockLevel == -1)
            {
                DebugUtils.LogError($" No unlock level defined for PowerupType: {powerupType}");
                return false;
            }

            return levelNumber == unlockLevel;
        }
        public bool IsPowerupUnlocked(PowerupType powerupType)
        {
            string key = powerupType switch
            {
                PowerupType.Shield => StringUtils.SHIELD_UNLOCKED,
                PowerupType.SpeedBoost => StringUtils.SPEEDBOOST_UNLOCKED,
                _ => null
            };

            if (string.IsNullOrEmpty(key))
            {
                DebugUtils.LogError($" No unlock key defined for PowerupType: {powerupType}");
                return false; // default safe value
            }

            return SaveSystem.LoadBool(key, false);
        }
        public void UnlockPowerup(PowerupType powerupType)
        {
            switch (powerupType)
            {
                case PowerupType.Shield:
                    SaveSystem.SaveBool(StringUtils.SHIELD_UNLOCKED, true);
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
