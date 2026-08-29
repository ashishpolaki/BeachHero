using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinDatabase", menuName = "Scriptable Objects/Skin/BoatSkinDatabase")]
    public class BoatSkinDatabaseSO : ScriptableObject
    {
        [SerializeField] private BoatSkinSO[] boatSkins;
        public BoatSkinSO[] BoatSkins => boatSkins;

        public BoatSkinSO GetBoatSkinByIndex(int index)
        {
            foreach (var skin in boatSkins)
            {
                if (skin.Index == index)
                    return skin;
            }
            DebugUtils.LogError("BoatSkinsDatabase is null or index is out of range.");
            return null;
        }

    }
}
