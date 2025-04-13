using UnityEngine;

namespace Watermelon
{
    public class SettingsPrivacyButton : SettingsButtonBase
    {
        private string url;

        public override void Init()
        {
#if MODULE_MONETIZATION
            if(Monetization.IsActive)
            {
                url = Monetization.Settings.PrivacyLink;
                if(string.IsNullOrEmpty(url))
                    gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
#else
            gameObject.SetActive(false);
#endif
        }

        public override void OnClick()
        {
            if (string.IsNullOrEmpty(url)) return;

            Application.OpenURL(url);

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}