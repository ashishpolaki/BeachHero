using UnityEngine;

namespace BeachHero
{
    public class SavedCharacter : MonoBehaviour
    {
        [SerializeField] private SavedCharacterUI savedCharacterUI;
        [SerializeField] private float waitTimePercentage;
        [SerializeField] private float waitTime;

        public void Init(Vector3 _position, float _waitTimePercentage, float levelTime)
        {
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            waitTime = (levelTime * _waitTimePercentage) / 100f;
        }

        public void UpdateWaitTime(float deltaTime)
        {
            waitTime -= deltaTime;
            if (waitTime <= 0)
            {

            }
        }
    }
}
