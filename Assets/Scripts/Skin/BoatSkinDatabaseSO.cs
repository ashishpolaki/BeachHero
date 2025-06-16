using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinDatabase", menuName = "Scriptable Objects/BoatSkinDatabase")]
    public class BoatSkinDatabaseSO : ScriptableObject
    {
        [SerializeField] private BoatSkinData[] boatSkins;
        public BoatSkinData[] BoatSkins => boatSkins;
    }

    [System.Serializable]
    public struct BoatSkinData
    {
        [SerializeField, UniqueID] private string id;
        [SerializeField] private int index;
        [SerializeField] private string name;
        [SerializeField] private GameObject prefab;
        [SerializeField, SkinPreview] private Sprite icon;
        [SerializeField] private float speed;
        [Range(0f, 1f), SerializeField] private float speedMeter;
        [SerializeField] private BoatSkinColorData[] skinColors;
        [SerializeField] private bool isPurchasableWithGameCurrency;
        [SerializeField] private bool isPurchasableWithAds;
        [Show("isPurchasableWithGameCurrency")]
        [SerializeField] private int inGameCurrencyCost;
        [Show("isPurchasableWithAds")]
        [SerializeField] private int adsRequired;

        public string ID => id;
        public int Hash { get; private set; }
        public int Index => index;
        public GameObject Prefab => prefab;
        public string Name => name;
        public float Speed => speed;
        public float SpeedMeter => speedMeter;
        public BoatSkinColorData[] SkinColors => skinColors;
        public Sprite Icon => icon;
        public bool IsPurchasableWithGameCurrency => isPurchasableWithGameCurrency;
        public bool IsPurchasableWithAds => isPurchasableWithAds;
        public int InGameCurrencyCost => inGameCurrencyCost;
        public int AdsRequired => adsRequired;

        public void Initialize()
        {
            Hash = id.GetHashCode();
        }
    }

    [System.Serializable]
    public struct BoatSkinColorData
    {
        public Color[] colors;
        public bool isPurchasableWithGameCurrency;
        public bool isPurchasableWithAds;
        [Show("isPurchasableWithGameCurrency")] public int inGameCurrencyCost;
        [Show("isPurchasableWithAds")] public int adsRequired;
    }
}
