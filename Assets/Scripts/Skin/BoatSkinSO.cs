using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinSO", menuName = "Scriptable Objects/Skin/BoatSkinSO")]
    public class BoatSkinSO : ScriptableObject
    {
        #region Inspector Variables
        [SerializeField] private string id;
        [SerializeField] private int index;
        [SerializeField] private string boatName;
        [SerializeField] private float speed;
        [Range(0f, 1f), SerializeField] private float speedMeter;
        [SerializeField] private BoatSkinColorData[] skinColors;
        [SerializeField] private bool isDefault;
        [Hide("isDefault"), SerializeField] private bool isPurchasableWithGameCurrency;
        [Hide("isDefault"), SerializeField] private bool isPurchasableWithRealMoney;
        [Show("isPurchasableWithGameCurrency"), SerializeField] private int inGameCurrencyCost;
        [Show("isPurchasableWithRealMoney"), SerializeField] private int realMoneyCost;
        #endregion

        #region Properties
        public string ID => id;
        public int Hash { get; private set; }
        public int Index => index;
        public string Name => boatName;
        public float Speed => speed;
        public float SpeedMeter => speedMeter;
        public BoatSkinColorData[] SkinColors => skinColors;
        public bool IsPurchasableWithGameCurrency => isPurchasableWithGameCurrency;
        public bool IsPurchasableWithRealMoney => isPurchasableWithRealMoney;
        public int InGameCurrencyCost => inGameCurrencyCost;
        public int RealMoneyCost => realMoneyCost;
        public bool IsDefaultBoat => isDefault;
        #endregion

        public void Initialize()
        {
            Hash = id.GetHashCode();
        }
    }
    [System.Serializable]
    public struct BoatSkinColorData
    {
        public Color[] ShaderColors;
        [SkinPreview] public Sprite sprite;
        public bool isDefault;
        [Hide("isDefault")] public bool isPurchasableWithGameCurrency;
        [Hide("isDefault")] public bool isPurchasableWithAds;
        [Show("isPurchasableWithGameCurrency")] public int inGameCurrencyCost;
        [Show("isPurchasableWithAds")] public int adsRequired;
    }
}
