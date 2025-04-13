using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Currencies Database", menuName = "Data/Core/Currencies Database")]
    public class CurrenciesDatabase : ScriptableObject
    {
        [SerializeField] Currency[] currencies;
        public Currency[] Currencies => currencies;

        public void Init()
        {
            // Initialize currencies
            for(int i = 0; i < currencies.Length; i++)
            {
                currencies[i].Init();
            }
        }
    }
}