#if CHEAT_CODE
using QFSW.QC;
using BeachHero;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class CheatCodes : MonoBehaviour
{
    // Keep serialized fields available in every build so the scene data layout
    // remains identical when CHEAT_CODE is enabled or disabled.
    public Button tapButton;
    public int requiredTaps = 3;
    public GameObject fpsObject;

#if CHEAT_CODE
    #region Tap Counter
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
    [Command("enable-fps")]
    public void EnableFPSCounter(bool val = true)
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

    [Command("force-set-level", "Sets the level. Default: 1")]
    public static string SetLevel(int levelNumber = 1)
    {
        if (SaveSystem.CurrentData == null)
        {
            SaveSystem.LoadGameData();
        }

        int targetLevel = levelNumber <= 0 ? 1 : levelNumber;
        SaveSystem.CurrentData.highestCompletedLevel = targetLevel;
        SaveSystem.SaveGameData();
        SaveSystem.CurrentData.isShieldUnlock = false;
        SaveSystem.CurrentData.isSpeedBoostUnlock = false;
        SaveSystem.CurrentData.shieldBalance = IntUtils.DEFAULT_SHIELD_BALANCE;
        SaveSystem.CurrentData.speedBoostBalance = IntUtils.DEFAULT_SPEEDBOOST_BALANCE;
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
        if (Application.isPlaying)
        {
            if (GameController.GetInstance != null && GameController.GetInstance.PowerupController != null)
            {
                GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, IntUtils.DEFAULT_SPEEDBOOST_BALANCE);
                GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, IntUtils.DEFAULT_SHIELD_BALANCE);
            }
            if (GameController.GetInstance != null)
            {
                GameController.GetInstance.SpawnLevel();
            }
            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.SetupLevels();
            }
        }
        return $"Level set to {targetLevel}";
    }

    [Command("add-coins")]
    public static void AddCoins(int amount = 100)
    {
        GameController.GetInstance.StoreController.IncrementCoinsBalance(amount);
    }

    [Command("add-shields")]
    public static void AddShields(int amount = 2)
    {
        GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.Shield, amount);
    }
    [Command("add-speed-boosts")]
    public static void AddSpeedBoosts(int amount = 2)
    {
        GameController.GetInstance.PowerupController.OnPowerupCollected(PowerupType.SpeedBoost, amount);
    }
    #endregion
#endif
}
