using UnityEngine;

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
        [SerializeField] private float boostSpeed;
        [Range(0f, 1f), SerializeField] private float speedMeter;
        [Range(1, 8), SerializeField] private int speedBarFillAmount;
        [SerializeField] private int coinCost;
        [SerializeField] private BoatSkinColorData[] skinColors;
        #endregion

        #region Properties
        public string ID => id;
        public int Index => index;
        public string Name => boatName;
        public float Speed => speed;
        public float SpeedMeter => speedMeter;
        public int SpeedBarFillAmount => speedBarFillAmount;
        public float BoostSpeed => boostSpeed;
        public BoatSkinColorData[] SkinColors => skinColors;
        public int CoinCost => coinCost;
        public GameObject BoatPrefab => boatPrefab;
        //public int Hash { get; private set; }
        #endregion

        //public void Initialize()
        //{
        //    Hash = id.GetHashCode();
        //}
    }
    [System.Serializable]
    public struct BoatSkinColorData
    {
        public Color[] ShaderColors;
        public Color previewColor;
        public int coinCost;
    }
}
