using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSkinData : ISkinData
    {
        [SerializeField, UniqueID] string id;
        public string ID => id;
        public int Hash { get; private set; }

        public AbstractSkinsDatabase SkinsProvider { get; private set; }

        public bool IsUnlocked => save.IsUnlocked;

        private SkinSave save;

        public virtual void Initialise(AbstractSkinsDatabase provider)
        {
            save = SaveController.GetSaveObject<SkinSave>(id);
            Hash = id.GetHashCode();

            SkinsProvider = provider;
        }

        public void Unlock()
        {
            save.IsUnlocked = true;
        }
    }
}
