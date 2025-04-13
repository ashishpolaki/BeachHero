using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SkinStore
{
    [CreateAssetMenu(fileName = "Skin Store Database", menuName = "Data/Skins/Store Database")]
    public class SkinStoreDatabase : ScriptableObject
    {
        [SerializeField] TabData[] tabs;
        public TabData[] Tabs => tabs;

        [SerializeField] SkinStoreProductData[] products;

        [SerializeField] int coinsForAdsAmount;
        [SerializeField] CurrencyType currencyForAds;

        public SkinTab[] TabTypes;

        public int CoinsForAds => coinsForAdsAmount;
        public CurrencyType CurrencyForAds => currencyForAds;

        public Dictionary<TabData, List<SkinStoreProductData>> Init()
        {
            var sortedProducts = new Dictionary<TabData, List<SkinStoreProductData>>();

            for(int i = 0; i < products.Length; i++)
            {
                var product = products[i];
                var tab = tabs[product.TabId];

                if (sortedProducts.ContainsKey(tab))
                {
                    sortedProducts[tab].Add(product); ;
                }
                else
                {
                    sortedProducts.Add(tab, new List<SkinStoreProductData> { product });
                }

                product.Init();
            }

            TabTypes = (SkinTab[]) Enum.GetValues(typeof(SkinTab));

            return sortedProducts;
        }
    }

    [System.Serializable]
    public class TabData
    {
        [SerializeField] string uniqueId;
        public string UniqueId => uniqueId;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] bool isActive = true;
        public bool IsActive => isActive;

        [SerializeField] SkinTab type;
        public SkinTab Type => type;

        [Space]
        [SerializeField] PreviewType previewType;
        public PreviewType PreviewType => previewType;

        [Space]
        [SerializeField] GameObject previewPrefab;
        public GameObject PreviewPrefab => previewPrefab;

        [SerializeField] Sprite backgroundImage;
        public Sprite BackgroundImage => backgroundImage;

        [SerializeField] Color backgroundColor = Color.white;
        public Color BackgroundColor => backgroundColor;

        [SerializeField] Color panelColor = Color.white;
        public Color PanelColor => panelColor;

        [SerializeField] Color tabActiveColor = Color.white;
        public Color TabActiveColor => tabActiveColor;

        [SerializeField] Color tabDisabledColor = Color.white;
        public Color TabDisabledColor => tabDisabledColor;

        [SerializeField] Color productBackgroundColor = Color.white;
        public Color ProductBackgroundColor => productBackgroundColor;

        public delegate void SimpleTabDelegate(TabData tab);
    }

    public enum PreviewType
    {
        Preview_2D,
        Preview_3D
    }
}