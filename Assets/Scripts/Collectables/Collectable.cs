using UnityEngine;

namespace BeachHero
{
    public interface ICollectable
    {
        public CollectableType CollectableType { get; set; }
        public abstract void Collect();
        public abstract void UpdateState();
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

        public Vector3 Position
        {
            get
            {
                return transform.position;
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
        public virtual void UpdateState()
        {
        }
    }
}
