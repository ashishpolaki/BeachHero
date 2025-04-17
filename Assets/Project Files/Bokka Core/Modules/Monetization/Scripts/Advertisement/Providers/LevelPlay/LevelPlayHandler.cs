#pragma warning disable 0618

using System.Threading.Tasks;
using UnityEngine;

namespace Bokka
{
#if MODULE_LEVELPLAY
    public class LevelPlayHandler : AdProviderHandler
    {
        private bool isBannerLoaded = false;

        private IronSourceListner eventsHolder;

        public LevelPlayHandler(AdProvider moduleType) : base(moduleType) { }

        protected override async Task<bool> InitProviderAsync()
        {
            if (Monetization.VerboseLogging)
                Debug.Log("[AdsManager]: LevelPlay is trying to initialize!", adsSettings);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            IronSource.Agent.setConsent(AdsManager.CanRequestAds());

            eventsHolder = Initializer.GameObject.AddComponent<IronSourceListner>();
            eventsHolder.Init(this);

            if (adsSettings.RewardedVideoType == AdProvider.LevelPlay)
            {
                //Add AdInfo Rewarded Video Events
                IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
                IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
                IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
                IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            }

            if (adsSettings.InterstitialType == AdProvider.LevelPlay)
            {
                //Add AdInfo Interstitial Events
                IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
                IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
                IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
                IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
                IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
            }

            if (adsSettings.BannerType == AdProvider.LevelPlay)
            {
                //Add AdInfo Banner Events
                IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
            }

            IronSourceEvents.onSdkInitializationCompletedEvent += () =>
            {
                if (Monetization.VerboseLogging)
                {
                    Debug.Log("[AdsManager]: LevelPlay is initialized!");

                    IronSource.Agent.validateIntegration();
                }

                tcs.SetResult(true);
            };

            if (Monetization.DebugMode)
                IronSource.Agent.setMetaData("is_test_suite", "enable");

            IronSource.Agent.init(GetAppKey());

            return await tcs.Task;
        }

        public void OpenTestSuite()
        {
            IronSource.Agent.launchTestSuite();
        }

        #region RewardedAd callback handlers
        // The Rewarded Video ad view has opened. Your activity will loose focus.
        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.LevelPlay, AdType.RewardedVideo);
            });
        }

        // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.LevelPlay, AdType.RewardedVideo);
            });
        }

        // The user completed to watch the video, and should be rewarded.
        // The placement parameter will include the reward data.
        // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(true);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdRewardedEvent event received");

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestRewardBasedVideo();
            });
        }

        // The rewarded video ad was failed to show.
        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdShowFailedEvent event received with message: " + error);

                HandleAdLoadFailure(AdType.RewardedVideo, error.getDescription(), ref rewardedRetryAttempt, () => RequestRewardedVideo());
            });
        }
        #endregion

        #region Interstitial callback handlers
        /************* Interstitial AdInfo Delegates *************/
        // Invoked when the interstitial ad was loaded succesfully.
        private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad loaded");

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                AdsManager.OnProviderAdLoaded(AdProvider.LevelPlay, AdType.Interstitial);
            });
        }

        // Invoked when the initialization process has failed.
        private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + ironSourceError);

                HandleAdLoadFailure(AdType.Interstitial, ironSourceError.getDescription(), ref interstitialRetryAttempt, () => RequestInterstitial());
            });
        }

        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.LevelPlay, AdType.Interstitial);
            });
        }

        // Invoked when the ad failed to show.
        private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + ironSourceError);

                HandleAdLoadFailure(AdType.Interstitial, ironSourceError.getDescription(), ref interstitialRetryAttempt, () => RequestInterstitial());
            });
        }

        // Invoked when the interstitial ad closed and the user went back to the application screen.
        private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.LevelPlay, AdType.Interstitial);

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }
        #endregion

        #region Banner callback handlers
        //Invoked once the banner has loaded
        private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: BannerOnAdLoadedEvent event received");

                AdsManager.OnProviderAdLoaded(AdProvider.LevelPlay, AdType.Banner);
            });
        }
    #endregion

        public override void DestroyBanner()
        {
            IronSource.Agent.destroyBanner();

            isBannerLoaded = false;

            AdsManager.OnProviderAdClosed(AdProvider.LevelPlay, AdType.Banner);
        }

        public override void HideBanner()
        {
            if(isBannerLoaded)
                IronSource.Agent.hideBanner();

            AdsManager.OnProviderAdClosed(AdProvider.LevelPlay, AdType.Banner);
        }

        public override void ShowBanner()
        {
            if(!isBannerLoaded)
            {
                IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.BANNER;
                switch (adsSettings.LevelPlayContainer.BannerType)
                {
                    case LevelPlayContainer.BannerPlacementType.Large:
                        ironSourceBannerSize = IronSourceBannerSize.LARGE;
                        break;
                    case LevelPlayContainer.BannerPlacementType.Rectangle:
                        ironSourceBannerSize = IronSourceBannerSize.RECTANGLE;
                        break;
                    case LevelPlayContainer.BannerPlacementType.Smart:
                        ironSourceBannerSize = IronSourceBannerSize.SMART;
                        break;
                }

                IronSourceBannerPosition ironSourceBannerPosition = IronSourceBannerPosition.BOTTOM;
                if (adsSettings.LevelPlayContainer.BannerPosition == BannerPosition.Top)
                    ironSourceBannerPosition = IronSourceBannerPosition.TOP;

                IronSource.Agent.loadBanner(ironSourceBannerSize, ironSourceBannerPosition);

                isBannerLoaded = true;
            }
            else
            {
                IronSource.Agent.displayBanner();
            }

            AdsManager.OnProviderAdDisplayed(AdProvider.LevelPlay, AdType.Banner);
        }

        public override void RequestInterstitial()
        {
            IronSource.Agent.loadInterstitial();
        }

        public override void ShowInterstitial(AdvertisementCallback callback)
        {
            IronSource.Agent.showInterstitial();
        }

        public override void RequestRewardedVideo()
        {
            // Do nothing
        }

        public override void ShowRewardedVideo(AdvertisementCallback callback)
        {
            IronSource.Agent.showRewardedVideo();
        }

        public override bool IsInterstitialLoaded()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public string GetAppKey()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.LevelPlayContainer.AndroidAppKey;
#elif UNITY_IOS
            return adsSettings.LevelPlayContainer.IOSAppKey;
#else
            return "unexpected_platform";
#endif
        }

        private class IronSourceListner : MonoBehaviour
        {
            private LevelPlayHandler ironSourceHandler;

            public void Init(LevelPlayHandler ironSourceHandler)
            {
                this.ironSourceHandler = ironSourceHandler;
            }

            private void OnApplicationPause(bool isPaused)
            {
                IronSource.Agent.onApplicationPause(isPaused);
            }
        }
    }
#endif
}