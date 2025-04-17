using UnityEngine;

namespace Bokka.BeachRescue
{
    [System.Serializable]
    public class BoatSkinData : AbstractSkinData
    {
        [SkinPreview]
        [SerializeField] Sprite preview;
        public Sprite Preview => preview;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;
    }
}