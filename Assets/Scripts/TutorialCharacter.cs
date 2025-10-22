using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class TutorialCharacter : MonoBehaviour
    {
        [SerializeField] private GameObject blocker;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI matterText;

        public void ShowBlocker()
        {
            blocker.SetActive(true);
        }

        public void HideBlocker()
        {
            blocker.SetActive(false);
        }

        public void SkipAnimation()
        {

        }

        public void PlayAnimation()
        {

        }
    }
}
