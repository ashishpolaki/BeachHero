using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class StarFish : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int animationCount;
        [SerializeField] private float animationInterval = 3f;

        private int blinkHash = Animator.StringToHash("Blink");
        private int sideLookHash = Animator.StringToHash("SideLook");
        private int lookCornersHash = Animator.StringToHash("LookCorners");
        private int winkHash = Animator.StringToHash("Wink");

        private Coroutine playAnimationCoroutine;
        private List<int> animationsList = new List<int>();

        public void Init()
        {
            animationsList.Add(blinkHash);
            animationsList.Add(sideLookHash);
            animationsList.Add(lookCornersHash);
            animationsList.Add(winkHash);
            PlayRandomAnimation();
        }
        public void PlayRandomAnimation()
        {
            if (animationCount <= 0)
            {
                return;
            }
            playAnimationCoroutine = StartCoroutine(PlayAnimationsLoop());
        }
        public void StopAnimation()
        {
            if (playAnimationCoroutine != null)
            {
                StopCoroutine(playAnimationCoroutine);
            }
        }
        private IEnumerator PlayAnimationsLoop()
        {
            int randomAnimIndex = Random.Range(0, animationCount);
            animator.CrossFade(animationsList[randomAnimIndex], 0.1f);
            yield return new WaitForSeconds(animationInterval);
            PlayRandomAnimation();
        }
    }
}
