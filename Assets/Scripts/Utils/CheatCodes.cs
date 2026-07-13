#if CHEAT_CODE
using QFSW.QC;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using BeachHero;

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
    //[Command("Active-WaterGraphics")]
    //public static void ActivateWaterGraphics(bool isActive)
    //{

    //    var water = Resources.FindObjectsOfTypeAll<GameObject>()
    //             .FirstOrDefault(go => go.name == "Water");
    //    water.SetActive(isActive);
    //    if (water != null)
    //    {
    //    }
    //}
    [Command("PurchaseNoAds")]
    public void PurchaseNoAds()
    {
        SaveSystem.SaveBool(StringUtils.NO_ADS_PURCHASED, true);
    }

    [Command("unlock-powerups")]
    public static void UnlockPowerups()
    {
        GameController.GetInstance.PowerupController.UnlockPowerup(PowerupType.Shield);
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
        GameController.GetInstance.OnLevelFailed(LevelFailDelayType.None);
    }

    [Command("force-set-level")]
    public static void SetLevel(int levelNumber)
    {
        SaveSystem.SaveInt(StringUtils.HIGHEST_COMPLETED_LEVEL, levelNumber);
        SaveSystem.SaveBool(StringUtils.SHIELD_UNLOCKED, false);
        SaveSystem.SaveBool(StringUtils.SPEEDBOOST_UNLOCKED, false);
        SaveSystem.SaveInt(StringUtils.SHIELD_BALANCE, IntUtils.DEFAULT_SHIELD_BALANCE);
        SaveSystem.SaveInt(StringUtils.SPEEDBOOST_BALANCE, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
        GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
        GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, IntUtils.DEFAULT_SHIELD_BALANCE);
#if UNITY_EDITOR
        var levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>("Assets/ScriptableObjects/Levels/LevelsDatabase.asset");
        if (levelDatabase != null)
        {
            levelDatabase.ClearLevelsData();
            EditorUtility.SetDirty(levelDatabase);
            AssetDatabase.SaveAssets();
        }
        else
        {
            DebugUtils.LogWarning("LevelDatabase asset not found at ");
        }
#endif
        GameController.GetInstance.SpawnLevel();
        MapController.GetInstance.SetupLevels();
    }

    [Command("add-coins")]
    public static void AddStarFish(int amount)
    {
        GameController.GetInstance.StoreController.IncrementCoinsBalance(amount);
    }

    [Command("add-shields")]
    public static void AddShields(int amount)
    {
        GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.Shield, amount);
    }
    [Command("add-speed-boosts")]
    public static void AddSpeedBoosts(int amount)
    {
        GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.SpeedBoost, amount);
    }
    #endregion
}
#endif
