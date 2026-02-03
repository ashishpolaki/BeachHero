using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MedalsUI : MonoBehaviour
    {
        [SerializeField] private Image medal1;
        [SerializeField] private Image medal2;
        [SerializeField] private Image medal3;
        [SerializeField] private Color medalEarned;
        [SerializeField] private Color medalUnEarned;

        public void AddListeners()
        {
            GameController.GetInstance.LevelController.OnMedalCountUpdated += OnMedalCountUpdated;
        }

        public void RemoveListeners()
        {
            GameController.GetInstance.LevelController.OnMedalCountUpdated -= OnMedalCountUpdated;
            ResetMedals();
        }

        private void ResetMedals()
        {
            medal1.color = medalUnEarned;
            medal2.color = medalUnEarned;
            medal3.color = medalUnEarned;
        }

        private void OnMedalCountUpdated(int medalCount)
        {
            medal1.color = medalCount >= 1 ? medalEarned : medalUnEarned;
            medal2.color = medalCount >= 2 ? medalEarned : medalUnEarned;
            medal3.color = medalCount >= 3 ? medalEarned : medalUnEarned;
        }
    }
}
