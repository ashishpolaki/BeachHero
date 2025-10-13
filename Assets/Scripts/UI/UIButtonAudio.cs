using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class UIButtonAudio : MonoBehaviour
    {
        [SerializeField] private AudioType buttonAudioType;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform rect;

        private void Awake()
        {
            if (button != null)
            {
                button.ButtonRegister(PlayAudio);
            }
        }
        private void OnDestroy()
        {
            if (button != null)
            {
                button.ButtonDeRegisterAll();
            }
        }

        private void PlayAudio()
        {
            if (buttonAudioType != AudioType.None)
            {
                if (AudioController.GetInstance != null)
                {
                    AudioController.GetInstance.PlaySound(buttonAudioType);
                }
            }
        }
    }
}

