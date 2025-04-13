using UnityEngine;
using Watermelon.BeachRescue;

namespace Watermelon
{
    [ExecuteInEditMode]
    public class CharacterSavableItem : MonoBehaviour
    {
        private CharacterBehaviour behaviour;
        private float waitingPercentageLastValue = float.MinValue;

        [SerializeField] Item item;
        public Item Item { get => item; set => item = value; }

        [SerializeField, Slider(0f, 1f)] float waitingPercentage = 1f;
        public float WaitingPercentage { get => waitingPercentage; set => waitingPercentage = value; }

        private void OnEnable()
        {
            behaviour = GetComponent<CharacterBehaviour>();
        }

        public void Update()
        {
            if (waitingPercentage != waitingPercentageLastValue)
            {
                waitingPercentageLastValue = waitingPercentage;
                behaviour.InitTimerForLevelEditor(waitingPercentage);
            }
        }
    }
}
