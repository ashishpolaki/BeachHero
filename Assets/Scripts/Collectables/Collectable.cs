using UnityEngine;

namespace BeachHero
{
    public interface ICollectable
    {
        public CollectableType CollectableType { get; set; }
        public bool IsCollected { get; }
        public abstract void Collect();
        public abstract void UpdateState();
    }
    public class Collectable : MonoBehaviour, ICollectable
    {
        [SerializeField] private CollectableType collectableType;
        private bool isCollected = false;
        private int count;

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
        public bool IsCollected => isCollected;
        public int Count => count;

        public virtual void Init(CollectableData collectableData)
        {
            transform.position = collectableData.position;
            transform.rotation = Quaternion.Euler(collectableData.rotation);
            collectableType = collectableData.type;
            count = collectableData.count;
            isCollected = false;
        }

        public virtual void Collect()
        {
            isCollected = true;
        }
        public virtual void UpdateState()
        {
        }
        public virtual void ResetState()
        {
            count = 0;
            isCollected = false;
        }
    }
}
