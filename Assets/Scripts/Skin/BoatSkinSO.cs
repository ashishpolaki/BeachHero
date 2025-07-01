using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinSO", menuName = "Scriptable Objects/Skin/BoatSkinSO")]
    public class BoatSkinSO : ScriptableObject
    {
        #region Inspector Variables
        [SerializeField] private string id;
        [SerializeField] private GameObject boatPrefab;
        [SerializeField] private int index;
        [SerializeField] private string boatName;
        [SerializeField] private float speed;
        [Range(0f, 1f), SerializeField] private float speedMeter;
        [SerializeField] private bool isDefault;
        [Hide("isDefault"), SerializeField] private bool isGameCurrency;
        [Hide("isDefault"), SerializeField] private bool isRealMoney;
        [Show("isGameCurrency"), SerializeField] private int gameCurrencyCost;
        [Show("isRealMoney"), SerializeField] private ProductType productType;
        [Show("isRealMoney"), SerializeField] private string realMoneyCost;
        [SerializeField] private BoatSkinColorData[] skinColors;
       
        #endregion

        #region Properties
        public string ID => id;
        public int Hash { get; private set; }
        public int Index => index;
        public string Name => boatName;
        public float Speed => speed;
        public float SpeedMeter => speedMeter;
        public BoatSkinColorData[] SkinColors => skinColors;
        public bool IsDefaultBoat => isDefault;
        public bool IsGameCurrency => isGameCurrency;
        public bool IsRealMoney => isRealMoney;
        public int InGameCurrencyCost => gameCurrencyCost;
        public string RealMoneyCost => realMoneyCost;
        public GameObject BoatPrefab => boatPrefab;
        public ProductType ProductType => productType;
        #endregion

        public void Initialize()
        {
            Hash = id.GetHashCode();
        }

        public void SetRealMoneyCost(string _realMoneyCost)
        {
            realMoneyCost = _realMoneyCost;
        }
    }
    [System.Serializable]
    public struct BoatSkinColorData
    {
        public Color[] ShaderColors;
        [SkinPreview] public Sprite sprite;
        public bool isDefault;
        [Hide("isDefault")] public bool isGameCurrency;
        [Hide("isDefault")] public bool isAds;
        [Show("isGameCurrency")] public int inGameCurrencyCost;
        [Show("isAds")] public int adsRequired;
    }
}
