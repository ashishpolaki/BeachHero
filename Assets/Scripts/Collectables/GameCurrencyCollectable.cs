using UnityEngine;

namespace BeachHero
{
    public class GameCurrencyCollectable : Collectable
    {
        #region Inspector Variables
        [SerializeField] private GameObject graphics;
        [SerializeField] private float rotateDuration = 1f;
        [SerializeField] private float moveDuration = 0.4f;
        #endregion

        #region Private Variables
        private bool canMoveToTarget;
        private TweenHandle moveTween;
        private TweenHandle rotationTween;
        #endregion

        public bool CanMoveToTarget => canMoveToTarget;

        public void SetTarget(Transform target)
        {
            canMoveToTarget = true;
        }
        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            graphics.SetActive(true);
            canMoveToTarget = false;
            if (Application.isPlaying)
            {
                rotationTween = TweenManager.RotateEulerAngles(transform, transform.eulerAngles, transform.eulerAngles + new Vector3(0f, 360, 0f), rotateDuration, loops: -1, loopType: LitMotion.LoopType.Incremental);
            }
        }
        public override void ResetState()
        {
            base.ResetState();
            graphics.SetActive(false);
            canMoveToTarget = false;
            if (Application.isPlaying)
            {
                rotationTween.Cancel();
                moveTween.Cancel();
            }
        }
        public override void Collect()
        {
            base.Collect();
            //  var particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleAutoDisable>();
            //  particle.PlayParticle(transform.position);
            //  particle.SetRotation(transform.eulerAngles);
            // graphics.SetActive(false);
            AudioController.GetInstance.PlaySound(AudioType.Collect1);
            GameController.GetInstance.OnGameCurrencyPickup();

            Vector3 moveTarget = UIController.GetInstance.StarsPanelWorldPosition();
            moveTween = TweenManager.Move(transform, transform.position, moveTarget, moveDuration,
            onComplete: () =>
            {
                ResetState();
                GameController.GetInstance.LevelController.OnGameCurrencyAnimation();
            });
        }

        #region Star Fish Float Animtion
        //[Header("Float Animation Settings")]
        //[SerializeField] private StarFish starFish;
        //[SerializeField] float floatTiltSpeed = 2f;
        //[SerializeField] float zTiltAmount = 5f;
        //[SerializeField] float yTiltAmount = 6f;
        //[SerializeField] float floatScaleAmount = 0.03f;
        //private float seed;
        //private Quaternion baseRotation;
        //seed = Random.Range(0f, 100f);
        //    baseRotation = transform.rotation;
        //private void OnFloatAnimation()
        //{
        //    float t = Time.time + seed;
        //    float wave = Mathf.Sin(t * floatTiltSpeed);

        //    // Wave rotation offsets
        //    float zTilt = wave * zTiltAmount;
        //    float yTilt = wave * yTiltAmount;

        //    // Combine with base rotation
        //    Quaternion waveRotation = Quaternion.Euler(0f, yTilt, zTilt);

        //    transform.rotation = baseRotation * waveRotation;

        //    // Scale (your current version)
        //    float scale = 1 + wave * floatScaleAmount;
        //    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scale, Time.deltaTime);
        //}
        #endregion
    }
}
