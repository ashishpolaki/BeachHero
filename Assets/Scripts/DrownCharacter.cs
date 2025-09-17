using UnityEngine;

namespace BeachHero
{
    public class DrownCharacter : MonoBehaviour
    {
        [SerializeField] private DrownCharacterUI drownCharacterUI;
        [SerializeField] private ParticleSystem pickUpParticle;
        [SerializeField] private ParticleSystem angryparticleSystem;
        //  [SerializeField] private ParticleSystem bloodParticle;
        [SerializeField] private GameObject graphicsSkin;
        [SerializeField] private GameObject graphicsUI;
        [SerializeField] private Animator animatorRef;
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color drownColor = Color.red;
        [SerializeField] private float waitTimePercentage;

        private Vector3 graphicsSkinPosition;
        private float waitTime;
        private float levelTime;
        private bool isPickedUp = false;
        private bool isDrown;

        private int DRAWN_HASH = Animator.StringToHash(StringUtils.DROWN_ANIM);
        private int IDLE_HASH = Animator.StringToHash(StringUtils.IDLE_ANIM);

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.CompareTag("Obstacle"))
        //    {
        //        IObstacle obstacle = other.GetComponent<IObstacle>();
        //        if (obstacle.ObstacleType == ObstacleType.Shark || obstacle.ObstacleType == ObstacleType.Eel)
        //        {
        //            OnMovingObstacleTrigger();
        //        }
        //    }
        //}

        //private void OnMovingObstacleTrigger()
        //{
        //    graphicsSkin.SetActive(false);
        //    bloodParticle.gameObject.SetActive(true);
        //    bloodParticle.Play();
        //    isDrown = true;
        //    GameController.GetInstance.OnLevelFailed();
        //    graphicsUI.SetActive(false);
        //}

        public void ResetState()
        {
            animatorRef.enabled = false;
            graphicsSkin.transform.localPosition = graphicsSkinPosition;
        }

        public void Init(Vector3 _position, float _waitTimePercentage, float levelTime)
        {
            if (graphicsSkinPosition == Vector3.zero)
            {
                graphicsSkinPosition = graphicsSkin.transform.localPosition;
            }
            //  bloodParticle.Stop();
            // bloodParticle.gameObject.SetActive(false);
            pickUpParticle.Stop();
            pickUpParticle.gameObject.SetActive(false);
            angryparticleSystem.Stop();
            angryparticleSystem.gameObject.SetActive(false);
            graphicsUI.SetActive(true);
            graphicsSkin.SetActive(true);
            animatorRef.enabled = true;
            animatorRef.SetTrigger(IDLE_HASH);
            isPickedUp = false;
            isDrown = false;
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            this.levelTime = levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            meshRenderer.material.SetColor(Shader.PropertyToID(StringUtils.TINT_COLOR), defaultColor);
            drownCharacterUI.UpdateTimer(waitTimePercentage);
        }

        public void UpdateState()
        {
            if (isPickedUp || isDrown)
            {
                return;
            }
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                waitTime = 0;
                OnTimeUp();
            }
            float waitPercentage = Mathf.Clamp01(waitTime / levelTime);
            drownCharacterUI.UpdateTimer(waitPercentage);
            Color color = Color.Lerp(drownColor, defaultColor, Mathf.InverseLerp(0f, waitTimePercentage, waitPercentage));
            meshRenderer.material.SetColor(Shader.PropertyToID(StringUtils.TINT_COLOR), color);
        }
        public void OnTimeUp()
        {
            isDrown = true;
            animatorRef.SetTrigger(DRAWN_HASH);
            angryparticleSystem.gameObject.SetActive(true);
            angryparticleSystem.Play();
            AudioController.GetInstance.PlaySound(AudioType.Die);
            graphicsUI.SetActive(false);
            GameController.GetInstance.OnLevelFailed(LevelFailDelayType.Long);
        }
        public void OnPickUp()
        {
            AudioController.GetInstance.PlaySound(AudioType.Collect2);
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            graphicsSkin.SetActive(false);
            graphicsUI.SetActive(false);
            isPickedUp = true;
            GameController.GetInstance.OnCharacterPickUp();
        }
    }
}
