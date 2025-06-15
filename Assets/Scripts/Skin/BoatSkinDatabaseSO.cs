using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "BoatSkinDatabase", menuName = "Scriptable Objects/BoatSkinDatabase")]
    public class BoatSkinDatabaseSO : ScriptableObject
    {
        [SerializeField] private BoatSkinData[] boatSkins;
    }

    [System.Serializable]
    public struct BoatSkinData
    {
        [SerializeField, UniqueID] private string id;
        [SerializeField] private string name;
        [SerializeField, SkinPreview] private GameObject prefab;
        [SerializeField] private float speed;
        [Range(0f, 1f), SerializeField] private float speedMeter;

        public string ID => id;
        public int Hash { get; private set; }

        public void Initialize()
        {
            Hash = id.GetHashCode();
        }
    }
}
