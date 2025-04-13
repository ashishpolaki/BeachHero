using UnityEngine;

namespace Watermelon
{
    public class SkinReward : Reward
    {
        [SkinPicker]
        [SerializeField] string skinID;

        [SerializeField] bool disableIfSkinIsUnlocked;

        private SkinsController skinsController;

        private void Start()
        {
            skinsController = SkinsController.Instance;
        }

        private void OnEnable()
        {
            SkinsController.SkinUnlocked += OnSkinUnlocked;    
        }

        private void OnDisable()
        {
            SkinsController.SkinUnlocked -= OnSkinUnlocked;
        }

        public override void ApplyReward()
        {
            skinsController.UnlockSkin(skinID, true);
        }

        public override bool CheckDisableState()
        {
            if(disableIfSkinIsUnlocked)
            {
                return skinsController.IsSkinUnlocked(skinID);
            }

            return false;
        }

        private void OnSkinUnlocked(ISkinData skinData)
        {
            if(disableIfSkinIsUnlocked)
            {
                if(skinData.ID == skinID)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
