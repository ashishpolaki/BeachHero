using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinDatabase", menuName = "Scriptable Objects/Skin/BoatSkinDatabase")]
    public class BoatSkinDatabaseSO : ScriptableObject
    {
        [SerializeField] private BoatSkinSO[] boatSkins;
        public BoatSkinSO[] BoatSkins => boatSkins;
    }

    //[System.Serializable]
    //public struct BoatSkinData
    //{
    //    [SerializeField, UniqueID] private string id;
    //    [SerializeField] private int index;
    //    [SerializeField] private string name;
    //    [SerializeField] private float speed;
    //    [Range(0f, 1f), SerializeField] private float speedMeter;
    //    [SerializeField] private BoatSkinColorData[] skinColors;
    //    [SerializeField] private bool isDefault;
    //    [Hide("isDefault")][SerializeField] private bool isPurchasableWithGameCurrency;
    //    [Hide("isDefault")][SerializeField] private bool isPurchasableWithRealMoney;
    //    [Show("isPurchasableWithGameCurrency")][SerializeField] private int inGameCurrencyCost;
    //    [Show("isPurchasableWithRealMoney")][SerializeField] private int realMoneyCost;

    //    public string ID => id;
    //    public int Hash { get; private set; }
    //    public int Index => index;
    //    public string Name => name;
    //    public float Speed => speed;
    //    public float SpeedMeter => speedMeter;
    //    public BoatSkinColorData[] SkinColors => skinColors;
    //    public bool IsPurchasableWithGameCurrency => isPurchasableWithGameCurrency;
    //    public bool IsPurchasableWithRealMoney => isPurchasableWithRealMoney;
    //    public int InGameCurrencyCost => inGameCurrencyCost;
    //    public int RealMoneyCost => realMoneyCost;
    //    public bool IsDefaultBoat => isDefault;

    //    public void Initialize()
    //    {
    //        Hash = id.GetHashCode();
    //    }
    //}
}
