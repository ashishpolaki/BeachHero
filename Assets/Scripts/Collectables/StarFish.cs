using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BeachHero
{
    public class StarFish : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float animationInterval = 3f;
        [SerializeField] private float thresholdNormalizedTime = 0.99f;

        private CancellationTokenSource playAnimationCTS;
        private List<int> animationClips = new List<int>();

        private void Awake()
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                int hash = Animator.StringToHash(clip.name);
                animationClips.Add(hash);
            }
        }
        private async UniTaskVoid PlayAnimationsLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (animationClips.Count == 0)
                        return;

                    int randomAnimationHash = animationClips[UnityEngine.Random.Range(0, animationClips.Count)];
                    animator.Play(randomAnimationHash, 0, 0f);

                    // allow one frame so animator state updates
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                    // Wait until we reach the threshold
                    while (!token.IsCancellationRequested &&
                           animator.GetCurrentAnimatorStateInfo(0).normalizedTime < thresholdNormalizedTime)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }

                    // Wait interval (cancellable)
                    await UniTask.Delay(TimeSpan.FromSeconds(animationInterval), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on cancel
            }
        }

        public void PlayRandomAnimation()
        {
            if (playAnimationCTS != null)
                return;

            playAnimationCTS = new CancellationTokenSource();
            PlayAnimationsLoopAsync(playAnimationCTS.Token).Forget();
        }
        public void StopAnimation()
        {
            animator.Rebind();
            animator.StopPlayback();
            if (playAnimationCTS != null)
            {
                playAnimationCTS.Cancel();
                playAnimationCTS.Dispose();
                playAnimationCTS = null;
            }
        }
    }
}
