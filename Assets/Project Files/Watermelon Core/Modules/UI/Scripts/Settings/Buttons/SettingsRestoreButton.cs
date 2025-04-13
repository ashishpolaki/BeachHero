namespace Watermelon
{
    public class SettingsRestoreButton : SettingsButtonBase
    {
        public override void Init()
        {
#if MODULE_MONETIZATION
            gameObject.SetActive(Monetization.IsActive);
#else
            gameObject.SetActive(false);
#endif
        }

        public override void OnClick()
        {
#if MODULE_MONETIZATION
            IAPManager.RestorePurchases();
#endif

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}