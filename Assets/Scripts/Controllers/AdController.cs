using GoogleMobileAds.Api;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class AdController : SingleTon<AdController>
    {
        private BannerView bannerView;
        private bool isBannerActive = false;

        #region Ad Id's
        // test id's ----------------------------------------
        private readonly string androidAppId = "ca-app-pub-3940256099942544~3347511713";
        private readonly string androidRewardedAdId = "ca-app-pub-3940256099942544/5224354917";
        private readonly string androidInterstitialAdId = "ca-app-pub-3940256099942544/1033173712";
        private readonly string androidBannerAdId = "ca-app-pub-3940256099942544/6300978111";


        private readonly string iosAppId = "ca-app-pub-3940256099942544~1458002511";
        private readonly string iosBannerAdId = "ca-app-pub-3940256099942544/2934735716";
        private readonly string iosInterstitialAdID = "ca-app-pub-3940256099942544/4411468910";
        private readonly string iosRewardedAdId = "ca-app-pub-3940256099942544/1712485313";

        // orginal id's -------------------------------------
        //   private string androidAppId = "";
        //private string androidRewardedAdId = "";
        //private string androidInterstitialAdId = "";
        //private string androidBannerAdId = "";


        //private string iosAppId = "";
        //private string iosBannerAdId = "";
        //private string iosInterstitialAdID = "";
        //private string iosRewardedAdId = "";
        #endregion

        private void Start()
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
                            Debug.Log("Adapter: " + className + " is initialized.");
                            RequestBanner();
                            break;
                    }
                }
            });
        }

        public void RequestBanner()
        {
            if (bannerView == null)
            {
#if UNITY_ANDROID
                string _adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
  private string _adUnitId = "unused";
#endif
             
                bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);
                bannerView.LoadAd(CreateAdRequest());
             //   bannerView.Hide();
            }
        }

        private AdRequest CreateAdRequest()
        {
            AdRequest adRequest = new AdRequest();
            adRequest.Keywords.Add("beach");
            return adRequest;
        }
    }

}
