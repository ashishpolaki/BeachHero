using UnityEngine;

namespace BeachHero
{
    public interface ICollectable
    {
        public CollectableType CollectableType { get; set; }
        public abstract void Collect();
    }
    public class Collectable : MonoBehaviour, ICollectable
    {
        [SerializeField] private CollectableType collectableType;

        public CollectableType CollectableType
        {
            get
            {
                return collectableType;
            }
            set
            {
                collectableType = value;
            }
        }

        public void Init(CollectableData collectableData)
        {
            transform.position = collectableData.position;
            collectableType = collectableData.type;
        }

        public virtual void Collect()
        {

        }
    }
}
