#pragma warning disable 0649 

using UnityEngine;

namespace Bokka
{
    public class SettingsLinkButton : SettingsButtonBase
    {
        [SerializeField] string url;

        public override void Init()
        {

        }

        public override void OnClick()
        {
            Application.OpenURL(url);

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}