using UnityEngine;

namespace BeachHero
{
    public enum TutorialCharacterType
    {
        Idle,
        WaveHand,
        Cry,
        Talk
    }
    public class TutorialCharacter : MonoBehaviour
    {
        [SerializeField] private GameObject characterRoot;
        [SerializeField] private Animator animator;

        private int waveHandAnim = Animator.StringToHash("Wavehand");
        private int cryAnim = Animator.StringToHash("Cry");
        private int talkAnim = Animator.StringToHash("Talk");

        public void SkipAnimation()
        {
            animator.StopPlayback();
            characterRoot.SetActive(false);
        }

        public void PlayAnimation(TutorialCharacterType tutorialCharacterType)
        {
            characterRoot.SetActive(true);
            switch (tutorialCharacterType)
            {
                case TutorialCharacterType.WaveHand:
                    animator.Play(waveHandAnim, 0, 0);
                    break;
                case TutorialCharacterType.Cry:
                    animator.Play(cryAnim, 0, 0);
                    break;
                case TutorialCharacterType.Talk:
                    animator.Play(talkAnim, 0, 0);
                    break;
                case TutorialCharacterType.Idle:
                    animator.Play("Idle", 0, 0);
                    break;
                default:
                    break;
            }
        }
    }
}
