using UnityEngine;

namespace BeachHero
{
    public class SavedCharacter : MonoBehaviour
    {
        [SerializeField] private SavedCharacterUI savedCharacterUI;
        [SerializeField] private float waitTimePercentage;
        private float waitTime;
        private float levelTime;

        public void Init(Vector3 _position, float _waitTimePercentage, float levelTime)
        {
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            this.levelTime = levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            savedCharacterUI.UpdateTimer(waitTimePercentage);
        }

        public void UpdateState(float deltaTime)
        {
            waitTime -= deltaTime;
            if (waitTime <= 0)
            {

            }
            float waitPercentage = Mathf.Clamp01(waitTime / levelTime);
            savedCharacterUI.UpdateTimer(waitPercentage);
        }
    }
}
