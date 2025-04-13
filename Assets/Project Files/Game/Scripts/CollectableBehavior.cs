using UnityEngine;

namespace Watermelon.BeachRescue
{
    public class CollectableBehavior : MonoBehaviour, IInteractable
    {
        private static int Counter { get; set; }

        private static int IDLE_HASH = Animator.StringToHash("Idle");
        private static int PICK_UP_HASH = Animator.StringToHash("PickUp");

        [SerializeField] Animator animator;
        [SerializeField] MeshRenderer meshRenderer;

        [Space]
        [SerializeField] CurrencyType currency;

        private IPool coinPickUpParticlePool;
        private TweenCase delayedCallTween;

        private bool available;

        private void Awake()
        {
            coinPickUpParticlePool = PoolManager.GetPoolByName("CoinPickUpParticle");
        }

        public static void ResetCounter()
        {
            Counter = 0;
        }

        public void Init()
        {
            meshRenderer.enabled = false;

            Tween.DelayedCall(Counter * 0.05f, () =>
            {
                available = true;
                meshRenderer.enabled = true;

                delayedCallTween.KillActive();
                animator.Rebind();
                animator.Play(IDLE_HASH, 0, Counter / 100f % 1f);
            });


            Counter++;
        }

        public void Interact()
        {
            if (!available)
                return;

            available = false;

            LevelController.OnCurrencyPicked(currency, 1);
            coinPickUpParticlePool.GetPooledObject().SetPosition(transform.position);
            AudioController.PlaySound(AudioController.AudioClips.coin);

            animator.SetTrigger(PICK_UP_HASH);

            delayedCallTween.KillActive();
            delayedCallTween = Tween.DelayedCall(1.1f, () =>
            {
                gameObject.SetActive(false);
            });
        }

        public void Reinit()
        {
            delayedCallTween.KillActive();
            gameObject.SetActive(true);

            animator.Play(IDLE_HASH, 0);

            available = true;

        }
    }
}