using UnityEngine;

namespace BeachHero
{
    public class Boat : MonoBehaviour
    {
        [SerializeField] private Transform boostPosition;
        [SerializeField] private Animator characterAnimator;
        private int VICTORY_HASH = Animator.StringToHash(StringUtils.VICTORY_ANIM);
        private int IDLE_HASH = Animator.StringToHash(StringUtils.SINKING_ANIM);

        public void PlayVictoryAnimation()
        {
            characterAnimator.SetTrigger(VICTORY_HASH);
        }

        public void PlayIdleAnimation()
        {
            characterAnimator.Play(IDLE_HASH, -1, Random.Range(0f, 1f));
        }
    }
}
