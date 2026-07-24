using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Threading.Tasks;

namespace BeachHero
{
    public class RemoteConfig : SingleTon<RemoteConfig>
    {
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        #region Keys
        // Ads
        private const string EnableAllAdsKey = "Enable_All_Ads";
        private const string EnableBannerAdsKey = "Enable_Banner_Ads";
        private const string EnableInterstitialAdsKey = "Enable_Interstitial_Ads";
        private const string InterstitialIntervalKey = "Interstitial_Interval";
        private const string AdsStartLevelKey = "Ads_Start_Level";

        //Powerups
        private const string SpeedBoostUnlockLevelKey = "SpeedBoost_Unlock_Level";
        private const string ShieldUnlockLevelKey = "Shield_Unlock_Level";
        private const string FreezeUnlockLevelKey = "Freeze_Unlock_Level";
        private const string StarFishMultiplierUnlockLevelKey = "StarFish_Multiplier_Unlock_Level";

        // UI
        private const string RateUsShowLevelKey = "RateUs_Show_Level";
        #endregion

        #region Data
        // Ads
        public bool IsAllAdsEnabled { get; private set; }
        public bool IsBannerAdsEnabled { get; private set; }
        public bool IsInterstitialAdsEnabled { get; private set; }
        public int InterstitialInterval { get; private set; }
        public int AdsStartLevel { get; private set; }

        // Powerups - Unlock
        public int SpeedBoostUnlockLevel { get; private set; }
        public int ShieldUnlockLevel { get; private set; }
        //   public int FreezeUnlockLevel { get; private set; }
        //  public int StarFishMultiplierUnlockLevel { get; private set; }

        // UI
        public int RateUsShowLevel { get; private set; }
        #endregion

        public void Init()
        {
            CheckRemoteConfigValues();
        }

        private void CheckRemoteConfigValues()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
              {
                  dependencyStatus = task.Result;
                  if (dependencyStatus == DependencyStatus.Available)
                  {
                      InitializeFirebase();
                      DebugUtils.Log("Remote Config - Resolved all Firebase dependencies: " + dependencyStatus);
                  }
                  else
                  {
                      DebugUtils.Log("Remote Config - Could not resolve all Firebase dependencies: " + dependencyStatus);
                  }
              });
        }
        private void InitializeFirebase()
        {
            // [START set_defaults]
            System.Collections.Generic.Dictionary<string, object> defaults =
              new System.Collections.Generic.Dictionary<string, object>();

            //Ads
            defaults.Add(EnableAllAdsKey, true);
            defaults.Add(EnableBannerAdsKey, true);
            defaults.Add(EnableInterstitialAdsKey, true);
            defaults.Add(InterstitialIntervalKey, IntUtils.INTERSTITIAL_AD_INTERVAL);
            defaults.Add(AdsStartLevelKey, IntUtils.ADS_START_LEVEL);

            // Unlock Powerup levels
            defaults.Add(SpeedBoostUnlockLevelKey, SaveSystem.LoadInt(SpeedBoostUnlockLevelKey, IntUtils.SPEEDBOOST_UNLOCK_LEVEL));
            defaults.Add(ShieldUnlockLevelKey, SaveSystem.LoadInt(ShieldUnlockLevelKey, IntUtils.SHIELD_UNLOCK_LEVEL));
            //  defaults.Add(FreezeUnlockLevelKey, IntUtils.FREEZE_UNLOCK_LEVEL);
            // defaults.Add(StarFishMultiplierUnlockLevelKey, IntUtils.STARFISH_MULTIPLIER_UNLOCK_LEVEL);

            // UI
            defaults.Add(RateUsShowLevelKey, IntUtils.RATE_US_TRIGGER_LEVEL);

            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
             .ContinueWithOnMainThread(task =>
             {
                 // [END set_defaults]
                 DebugUtils.Log("Remote Config - RemoteConfig configured and ready! ");
                 FetchDataAsync();
             });
        }

        private Task FetchDataAsync()
        {
            DebugUtils.Log("Remote Config - Fetching Remote Data ");
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }

        private void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                DebugUtils.Log("Remote Config - Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                DebugUtils.Log("Remote Config - Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                DebugUtils.Log("Remote Config - Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        DebugUtils.Log(String.Format("Remote Config - Remote data loaded and ready (last fetch time {0}).",
                                       info.FetchTime));
                        GetRemoteData();
                    });
                    break;

                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            DebugUtils.Log("Remote Config - Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            DebugUtils.Log("Remote Config - Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    GetRemoteData();
                    break;

                case LastFetchStatus.Pending:
                    DebugUtils.Log("Remote Config - Latest Fetch call still pending.");
                    break;
            }
        }

        private void GetRemoteData()
        {
            //ADs
            IsAllAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableAllAdsKey).BooleanValue;
            IsBannerAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableBannerAdsKey).BooleanValue;
            IsInterstitialAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableInterstitialAdsKey).BooleanValue;
            InterstitialInterval = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(InterstitialIntervalKey).LongValue;
            AdsStartLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(AdsStartLevelKey).LongValue;

            // Unlock levels
            SpeedBoostUnlockLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(SpeedBoostUnlockLevelKey).LongValue;
            ShieldUnlockLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(ShieldUnlockLevelKey).LongValue;
            SaveSystem.SaveInt(SpeedBoostUnlockLevelKey, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(SpeedBoostUnlockLevelKey).LongValue);
            SaveSystem.SaveInt(ShieldUnlockLevelKey, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(ShieldUnlockLevelKey).LongValue);
            //  FreezeUnlockLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(FreezeUnlockLevelKey).LongValue;
            //  StarFishMultiplierUnlockLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(StarFishMultiplierUnlockLevelKey).LongValue;

            // UI
            RateUsShowLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(RateUsShowLevelKey).LongValue;
        }
    }
}
