using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Currencies", false)]
    public class CurrencyInitModule : InitModule
    {
        public override string ModuleName => "Currencies";

        [SerializeField] CurrenciesDatabase currenciesDatabase;

        public override void CreateComponent()
        {
            CurrenciesController.Init(currenciesDatabase);
        }
    }
}
