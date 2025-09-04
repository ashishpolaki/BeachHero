using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class StarFish : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float animationInterval = 3f;
        [SerializeField] private float thresholdNormalizedTime = 0.99f;

        private Coroutine playAnimationCoroutine;
        private List<int> animationClips = new List<int>();

        private void Awake()
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                int hash = Animator.StringToHash(clip.name);
                animationClips.Add(hash);
            }
        }
        public void Init()
        {
            PlayRandomAnimation();
        }
        public void PlayRandomAnimation()
        {
            if (playAnimationCoroutine == null && animationClips.Count > 0)
            {
                playAnimationCoroutine = StartCoroutine(PlayAnimationsLoop());
            }
        }
        private IEnumerator PlayAnimationsLoop()
        {
            while (true)
            {
                int randomAnimationHash = animationClips[Random.Range(0, animationClips.Count)];
                animator.Play(randomAnimationHash, 0, 0);
                yield return null;

                // Wait until we reach the threshold 
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < thresholdNormalizedTime)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(animationInterval);
            }
        }
        public void StopAnimation()
        {
            if (playAnimationCoroutine != null)
            {
                StopCoroutine(playAnimationCoroutine);
                playAnimationCoroutine = null;
            }
        }
    }
}
