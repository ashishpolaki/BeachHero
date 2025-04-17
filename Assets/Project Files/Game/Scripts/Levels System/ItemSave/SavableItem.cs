using UnityEngine;
namespace Bokka
{
    public class SavableItem : MonoBehaviour
    {
        [SerializeField] Item item;

        public Item Item { get => item; set => item = value; }
    }
}