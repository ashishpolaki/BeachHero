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
        private const string EnableAllAdsKey = "Enable_All_Ads";
        private const string EnableBannerAdsKey = "Enable_Banner_Ads";
        private const string EnableInterstitialAdsKey = "Enable_Interstitial_Ads";
        #endregion

        #region Data
        public bool IsAllAdsEnabled { get; private set; }
        public bool IsBannerAdsEnabled { get; private set; }
        public bool IsInterstitialAdsEnabled { get; private set; }
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

            defaults.Add(EnableAllAdsKey, true);
            defaults.Add(EnableBannerAdsKey, true);
            defaults.Add(EnableInterstitialAdsKey, true);

            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
             .ContinueWithOnMainThread(task =>
             {
                 // [END set_defaults]
                 DebugUtils.Log("Remote Config - RemoteConfig configured and ready! ");
                 FetchDataAsync();
             });

            //AdsManager.Instance.LoadStartAd();
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
            IsAllAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableAllAdsKey).BooleanValue;
            IsBannerAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableBannerAdsKey).BooleanValue;
            IsInterstitialAdsEnabled = FirebaseRemoteConfig.DefaultInstance.GetValue(EnableInterstitialAdsKey).BooleanValue;
        }
    }
}
