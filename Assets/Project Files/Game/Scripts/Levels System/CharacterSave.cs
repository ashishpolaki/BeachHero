using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CharacterSave
    {
        [SerializeField] ItemSave itemSave;

        [SerializeField] float waitingPercentage;

        public ItemSave ItemSave { get => itemSave; set => itemSave = value; }
        public float WaitingPercentage { get => waitingPercentage; set => waitingPercentage = value; }

        public CharacterSave()
        {
        }

        public CharacterSave(ItemSave itemSave, int waitingLevel)
        {
            this.itemSave = itemSave;
            this.waitingPercentage = waitingLevel;
        }
               
    }
}