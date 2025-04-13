using UnityEngine;
namespace Watermelon
{

    [System.Serializable]
    public class ItemSave
    {
        [SerializeField]
        private Item type;

        [SerializeField]
        private Vector3 position;

        [SerializeField]
        private Vector3 rotation;

        [SerializeField]
        private Vector3 scale;

        public Item Type { get => type; set => type = value; }
        public Vector3 Position { get => position; set => position = value; }
        public Vector3 Rotation { get => rotation; set => rotation = value; }
        public Vector3 Scale { get => scale; set => scale = value; }

        public ItemSave()
        {
        }

        public ItemSave(Item item, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.type = item;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

    }
}