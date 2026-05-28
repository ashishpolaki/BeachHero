using TMPro;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace BeachHero
{
    [System.Serializable]
    public class RealMoneyProduct
    {
        public int index;
        public string Id;                 // Google Play ID
        public ProductType Type;
        public string realMoneyCost;
        public StoreReward[] rewards;
    }

    [System.Serializable]
    public class GameCurrencyProduct
    {
        public int index;
        public int gameCurrencyCost;
        public StoreReward[] rewards;
    }

    [System.Serializable]
    public class StoreReward
    {
        public StoreItemType itemType;              // What kind of item is rewarded
        public int quantity;                        // How much of it is rewarded
    }

    public enum StoreItemType
    {
        Magnet,
        SpeedBoost,
        NoAds,
        GameCurrency
    }

    //Ui
    [System.Serializable]
    public struct RealMoneyProductUI
    {
        public int index;
        public Button purchaseButton;
        public TextMeshProUGUI priceText;
        public StoreRewardUI[] storeRewardUIs;
    }

    [System.Serializable]
    public struct GameCurrencyProductUI
    {
        public int index;
        public Button purchaseButton;
        public TextMeshProUGUI priceText;
        public StoreRewardUI[] storeRewardUIs;
    }
    [System.Serializable]
    public struct RewardAdItemUI
    {
        public StoreItemType itemType;
        public int quantity;
        public TextMeshProUGUI quantityText;
        public Button watchAdButton;
    }
    [System.Serializable]
    public struct StoreRewardUI
    {
        public StoreItemType itemType;
        public TextMeshProUGUI quantityText;
    }
}
