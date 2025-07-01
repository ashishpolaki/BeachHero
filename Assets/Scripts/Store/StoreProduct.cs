using TMPro;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace BeachHero
{
    [System.Serializable]
    public class StoreProduct
    {
        public int index;
        public bool isRealMoney;
        public bool isGameCurrency;
        public StoreProductContent[] contents;      // Items given when purchased
        [Show("isRealMoney")] public string Id;                    // Matches Google Play Console Product ID
        [Show("isRealMoney")] public ProductType Type;
        [Show("isRealMoney")] public string realMoneyCost;
        [Show("isGameCurrency")] public int gameCurrencyCost;
    }

    [System.Serializable]
    public class StoreProductContent
    {
        public StoreItemType itemType;              // What kind of item is rewarded
        public int quantity;                        // How much of it is rewarded
    }

    public enum StoreItemType
    {
        Magnet,
        SpeedBoost,
        NoAds,
        Starfish,
        GameCurrency
    }

    [System.Serializable]
    public struct StoreProductUI
    {
        public int index;
        public StoreProductContentUI[] contentUis;

        public Button gameCurrencyPurchaseButton;
        public Button realMoneyPurchaseButton;

        public TextMeshProUGUI gameCurrencyPriceText;
        public TextMeshProUGUI realMoneyPriceText;
    }

    [System.Serializable]
    public struct StoreProductContentUI
    {
        public StoreItemType itemType;
        public TextMeshProUGUI quantityText;
    }
}
