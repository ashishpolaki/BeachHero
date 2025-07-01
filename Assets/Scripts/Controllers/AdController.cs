using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class AdController : SingleTon<AdController>
    {
        private BannerView bannerView;
        private InterstitialAd interstitial;
        private NativeOverlayAd _nativeOverlayAd;
        private RewardedAd rewardedAd;

        private bool isBannerActive = false;
        private bool isInterstitialActive = false;
        private string gameName = "Beach Hero";

        #region Ad Id's
        // test id's ----------------------------------------
        private readonly string androidAppId = "ca-app-pub-3940256099942544~3347511713";
        private readonly string androidRewardedAdId = "ca-app-pub-3940256099942544/5224354917";
        private readonly string androidInterstitialAdId = "ca-app-pub-3940256099942544/1033173712";
        private readonly string androidNativeOverlayAdId = "ca-app-pub-3940256099942544/2247696110";
        private readonly string androidBannerAdId = "ca-app-pub-3940256099942544/6300978111";

        private readonly string iosAppId = "ca-app-pub-3940256099942544~1458002511";
        private readonly string iosBannerAdId = "ca-app-pub-3940256099942544/2934735716";
        private readonly string iosInterstitialAdID = "ca-app-pub-3940256099942544/4411468910";
        private readonly string iosNativeOverlayAdId = "ca-app-pub-3940256099942544/3986624511";
        private readonly string iosRewardedAdId = "ca-app-pub-3940256099942544/1712485313";

        // orginal id's -------------------------------------
        //   private string androidAppId = "";
        //private string androidRewardedAdId = "";
        //private string androidInterstitialAdId = "";
        //private string androidBannerAdId = "ca-app-pub-9358123754024746/6409681398";

        //private string iosAppId = "";
        //private string iosBannerAdId = "";
        //private string iosInterstitialAdID = "";
        //private string iosRewardedAdId = "";
        #endregion

        public void Init()
        {
#if UNITY_ANDROID
            string appId = androidAppId;
#elif UNITY_IPHONE
		string appId = iosAppId; 
#else
		string appId = "unexpected_platform";
#endif
            MobileAds.SetiOSAppPauseOnBackground(true);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((initStatus) =>
            {
                Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;
                    switch (status.InitializationState)
                    {
                        case AdapterState.NotReady:
                            // The adapter initialization did not complete.
                            //MonoBehaviour.print("Adapter: " + className + " not ready.");
                            break;
                        case AdapterState.Ready:
                            // The adapter was successfully initialized.
                            //  Debug.Log("Adapter: " + className + " is initialized.");
                            break;
                    }
                }
            });
            RequestADs();
        }

        public void PurchasedNoADsPack()
        {
            SaveController.SaveBool(StringUtils.NO_ADS_PURCHASED, true);
        }

        public void RequestADs()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            // Request Banner Ad
            RequestBanner();

            // Request Interstitial Ad
            RequestInterstitial();

            // Request Rewarded Video Ad
            RequestRewardedAD();

            // Request Native Overlay Ad
            RequestNativeOverlay();
        }

        #region NativeOverlay AD
        public void RequestNativeOverlay()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }
            // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
            string _adUnitId = androidNativeOverlayAdId;
#elif UNITY_IPHONE
   string _adUnitId = iosNativeOverlayAdId;
#else
   string _adUnitId = "unused";
#endif
            // Clean up the old ad before loading a new one.
            if (_nativeOverlayAd != null)
            {
                DestroyNativeAd();
            }

            Debug.Log("Loading Native Overlay ad with ad unit id: " + _adUnitId);
            var adRequest = CreateAdRequest();

            var options = new NativeAdOptions
            {
                AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
                MediaAspectRatio = MediaAspectRatio.Any,
            };
            // Send the request to load the ad.
            NativeOverlayAd.Load(_adUnitId, adRequest, options,
                (NativeOverlayAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("Native Overlay ad failed to load an ad " +
                               " with error: " + error);
                        return;
                    }

                    // The ad should always be non-null if the error is null, but
                    // double-check to avoid a crash.
                    if (ad == null)
                    {
                        Debug.LogError("Unexpected error: Native Overlay ad load event " +
                               " fired with null ad and null error.");
                        return;
                    }

                    // The operation completed successfully.
                    Debug.Log("Native Overlay ad loaded with response : " +
                       ad.GetResponseInfo());
                    _nativeOverlayAd = ad;

                    // Register to ad events to extend functionality.
                    RegisterNativeOverlayEventHandlers(ad);
                });
        }

        private void RegisterNativeOverlayEventHandlers(NativeOverlayAd nativeOverlayAd)
        {
            // Raised when the ad is estimated to have earned money.
            nativeOverlayAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Native Overlay ad paid {0} {1}.",
                                       adValue.Value,
                                                          adValue.CurrencyCode));
            };

            nativeOverlayAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Native Overlay ad recorded an impression.");
            };

            nativeOverlayAd.OnAdClicked += () =>
            {
                Debug.Log("Native Overlay ad was clicked.");
            };

            nativeOverlayAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Native Overlay ad full screen content opened.");
            };
            nativeOverlayAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Native Overlay ad full screen content closed.");
            };
        }

        /// <summary>
        /// Renders the ad.
        /// </summary>
        public void RenderNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Rendering Native Overlay ad.");

                // Define a native template style with a custom style.
                var style = new NativeTemplateStyle
                {
                    TemplateId = NativeTemplateId.Medium,
                    MainBackgroundColor = Color.red,
                    CallToActionText = new NativeTemplateTextStyle
                    {
                        BackgroundColor = Color.green,
                        TextColor = Color.white,
                        FontSize = 9,
                        Style = NativeTemplateFontStyle.Bold
                    }
                };

                // Renders a native overlay ad at the default size
                // and anchored to the bottom of the screne.
                _nativeOverlayAd.RenderTemplate(style, AdPosition.Bottom);
            }
        }
        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowNativeAd()
        {
            Debug.Log("Showing Native Overlay ad.");
            if (_nativeOverlayAd != null)
            {
                _nativeOverlayAd.Show();
            }
        }
        /// <summary>
        /// Hides the ad.
        /// </summary>
        public void HideNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Hiding Native Overlay ad.");
                _nativeOverlayAd.Hide();
            }
        }
        /// <summary>
        /// Destroys the native overlay ad.
        /// </summary>
        public void DestroyNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Destroying native overlay ad.");
                _nativeOverlayAd.Destroy();
                _nativeOverlayAd = null;
            }
        }
        #endregion

        #region RewardedVideo AD
        public void RequestRewardedAD()
        {
            // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
            string _adUnitId = androidRewardedAdId;
#elif UNITY_IPHONE
  string _adUnitId = iosRewardedAdId;
#else
   string _adUnitId = "unused";
#endif
            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_adUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());
                    rewardedAd = ad;
                    RegisterEventHandlers(rewardedAd);
                });
        }

        public void ShowRewardedAd(/*Action<Reward> onUserEarnedReward*/)
        {
            const string rewardMsg =
     "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
        }
        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                RequestRewardedAD();
                Debug.Log("Rewarded ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                RequestRewardedAD();
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }
        #endregion

        #region Interstitial AD
        public void RequestInterstitial()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            if (SaveController.LoadBool(StringUtils.NO_ADS_PURCHASED, false))
            {
                return;
            }

            // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
            string adUnitId = androidInterstitialAdId;
#elif UNITY_IPHONE
		string adUnitId = iosInterstitialAdID;
#else
		string adUnitId = "unused";
#endif
            // Clean up the old ad before loading a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
                interstitial = null;
            }
            // create our request used to load the ad.
            var adRequest = CreateAdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        return;
                    }

                    interstitial = ad;
                    RegisterInterestialEventHandlers(interstitial);
                });
        }

        public void ShowInterstitialAd()
        {
            if (SaveController.LoadBool(StringUtils.NO_ADS_PURCHASED, false))
            {
                return;
            }
            if (interstitial != null && interstitial.CanShowAd())
            {
                interstitial.Show();
            }
            else
            {
                RequestInterstitial();
            }
        }

        private void RegisterInterestialEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                isInterstitialActive = true;
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                RequestInterstitial();
                //Fade Black Screen
                isInterstitialActive = false;
                Debug.Log("Interstitial ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                isInterstitialActive = false;
                RequestInterstitial();
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }
        #endregion

        #region Banner AD
        public void RequestBanner()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            if(SaveController.LoadBool(StringUtils.NO_ADS_PURCHASED,false))
            {
                return;
            }

            if (bannerView == null)
            {
#if UNITY_ANDROID
                string _adUnitId = androidBannerAdId;
#elif UNITY_IPHONE
  private string _adUnitId = iosBannerAdId;
#else
  private string _adUnitId = "unused";
#endif
                bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);
                BannerAddListeners();
                bannerView.LoadAd(CreateAdRequest());
                HideBanner();
            }
        }

        private AdRequest CreateAdRequest()
        {
            AdRequest adRequest = new AdRequest();
            adRequest.Keywords.Add(gameName);
            return adRequest;
        }

        /// <summary>
        /// Destroys the banner view.
        /// </summary>
        public void DestroyAd()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }
        }

        private void BannerAddListeners()
        {
            if (bannerView != null)
            {
                // Raised when an ad is loaded into the banner view.
                bannerView.OnBannerAdLoaded += HandleOnAdLoaded;
                // Raised when an ad fails to load into the banner view.
                bannerView.OnBannerAdLoadFailed += HandleOnAdLoadFailed;
                // Raised when the ad is estimated to have earned money.
                bannerView.OnAdPaid += HandleOnAdPaid;
                // Raised when an impression is recorded for an ad.
                bannerView.OnAdImpressionRecorded += HandleOnAdImpressionRecorded;
                // Raised when a click is recorded for an ad.
                bannerView.OnAdClicked += HandleOnAdClick;
                // Raised when an ad opened full screen content.
                bannerView.OnAdFullScreenContentOpened += HandleOnAdFullScreenOpen;
                // Raised when the ad closed full screen content.
                bannerView.OnAdFullScreenContentClosed += HandleOnAdFullScreenClosed;
            }
        }

        private void HandleOnAdLoaded()
        {
            DebugUtils.Log("Banner ad loaded successfully.");
        }
        private void HandleOnAdLoadFailed(LoadAdError loadAdError)
        {
            DebugUtils.LogError("Banner ad failed to load: " + loadAdError.GetMessage());
        }
        private void HandleOnAdPaid(AdValue adValue)
        {
            DebugUtils.Log($"Banner ad paid {adValue.Value} {adValue.CurrencyCode}.");
        }
        private void HandleOnAdImpressionRecorded()
        {
            DebugUtils.Log("Banner ad impression recorded.");
        }
        private void HandleOnAdClick()
        {
            DebugUtils.Log("Banner ad clicked.");
        }
        private void HandleOnAdFullScreenOpen()
        {
            DebugUtils.Log("Banner ad opened full screen content.");
        }
        private void HandleOnAdFullScreenClosed()
        {
            DebugUtils.Log("Banner ad closed full screen content.");
        }

        public void HideBanner()
        {
            if (bannerView != null)
            {
                isBannerActive = false;
                bannerView.Hide();
            }
        }

        public void ShowBanner()
        {
            if (SaveController.LoadBool(StringUtils.NO_ADS_PURCHASED, false))
            {
                return;
            }
            //  if (AllStringConstants.isTutorialInProgress)
            //     return;
            if (bannerView != null && !isBannerActive)
            {
                bannerView.Show();
                isBannerActive = true;
            }
            else
            {
                RequestBanner();
            }
        }
        #endregion
    }
}
