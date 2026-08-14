using UnityEngine;

namespace BeachHero
{
    public enum NetworkActionType
    {
        None,
        RewardedAd,
        Leaderboard,
        StorePurchase
    }

    public static class NetworkController
    {
        private static NetworkActionType lastNetworkAction = NetworkActionType.None;

        public static bool IsInternetAvailable => Application.internetReachability != NetworkReachability.NotReachable;

        public static void ShowNoInternetScreen(NetworkActionType actionType)
        {
            lastNetworkAction = actionType;
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Push);
        }

        public static void ExecuteRetry()
        {
            switch (lastNetworkAction)
            {
                case NetworkActionType.RewardedAd:
                    if (AdController.GetInstance.IsRewardedADLoaded())
                    {
                        AdController.GetInstance.ShowRewardedAd();
                    }
                    else
                    {
                        AdController.GetInstance.RequestRewardedAD();
                    }
                    break;

                case NetworkActionType.Leaderboard:
                    PlayGamesController.GetInstance.ShowLeaderboardUI();
                    break;

                case NetworkActionType.StorePurchase:
                    // Reserved for Store purchase retries
                    break;
            }

            lastNetworkAction = NetworkActionType.None;
        }
    }
}
