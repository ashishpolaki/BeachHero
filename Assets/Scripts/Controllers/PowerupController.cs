using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public enum PowerupType
    {
        Magnet,
        Speed
    }
    public class PowerupController : MonoBehaviour
    {
        private Dictionary<PowerupType, int> powerupCounts = new Dictionary<PowerupType, int>();

        #region Public Methods
        public void OnPowerupCollected(PowerupType powerupType)
        {
            IncrementPowerupCount(powerupType);
        }
        public void LoadPowerups()
        {
            foreach (PowerupType powerupType in System.Enum.GetValues(typeof(PowerupType)))
            {
                int powerupCount = SaveController.LoadInt($"{StringUtils.POWERUP}{(int)powerupType}", 1);
                powerupCounts[powerupType] = powerupCount;
            }
        }
        public int GetPowerupCount(PowerupType powerupType)
        {
            return powerupCounts.TryGetValue(powerupType, out int count) ? count : 0;
        }
        public void OnPowerupActivated(PowerupType powerupType)
        {
            // Implement logic for when a powerup is used
            // This could involve decrementing the count or applying the powerup effect
            powerupCounts[powerupType]--;
            SavePowerupCount(powerupType);
        }
        #endregion

        #region Private Methods
        private void IncrementPowerupCount(PowerupType powerupType)
        {
            if (!powerupCounts.ContainsKey(powerupType))
            {
                powerupCounts[powerupType] = 0;
            }
            powerupCounts[powerupType]++;
            SavePowerupCount(powerupType);
        }
        private void SavePowerupCount(PowerupType powerupType)
        {
            SaveController.SaveInt($"{StringUtils.POWERUP}{(int)powerupType}", powerupCounts[powerupType]);
        }
        #endregion
    }
}
