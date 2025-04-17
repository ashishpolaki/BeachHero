using UnityEngine;

namespace Bokka
{
    [RegisterModule("Save Controller", core: true, order: 900)]
    public class SaveInitModule : InitModule
    {
        public override string ModuleName => "Save Controller";

        [SerializeField] float autoSaveDelay = 0;

        public override void CreateComponent()
        {
            SaveController.Init(autoSaveDelay);
        }
    }
}
