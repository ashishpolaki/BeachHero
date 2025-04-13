using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextController : MonoBehaviour
    {
        private static FloatingTextController floatingTextController;

        [SerializeField] FloatingTextCase[] floatingTextCases;
        private Dictionary<int, FloatingTextCase> floatingTextLink;

        public void Init()
        {
            floatingTextController = this;

            floatingTextLink = new Dictionary<int, FloatingTextCase>();
            for (int i = 0; i < floatingTextCases.Length; i++)
            {
                FloatingTextCase floatingText = floatingTextCases[i];
                if(string.IsNullOrEmpty(floatingText.Name))
                {
                    Debug.LogError("[Floating Text]: Floating Text initialization failed. A unique name (ID) must be provided. Please ensure the 'name' field is not empty before proceeding.", this);

                    continue;
                }

                if (floatingText.FloatingTextBehavior == null)
                {
                    Debug.LogError(string.Format("Floating Text ({0}) initialization failed. No Floating Text Behavior linked. Please assign a valid Floating Text Behavior before proceeding.", floatingText.Name), this);

                    continue;
                }

                floatingText.Init();

                floatingTextLink.Add(floatingText.Name.GetHashCode(), floatingText);
            }
        }

        private void OnDestroy()
        {
            if(!floatingTextCases.IsNullOrEmpty())
            {
                for (int i = 0; i < floatingTextCases.Length; i++)
                {
                    PoolManager.DestroyPool(floatingTextCases[i].FloatingTextPool);
                }
            }
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, Color color)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, color);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, Color color)
        {
            if (floatingTextController.floatingTextLink.ContainsKey(floatingTextNameHash))
            {
                FloatingTextCase floatingTextCase = floatingTextController.floatingTextLink[floatingTextNameHash];

                GameObject floatingTextObject = floatingTextCase.FloatingTextPool.GetPooledObject();
                floatingTextObject.transform.position = position;
                floatingTextObject.transform.rotation = rotation;
                floatingTextObject.SetActive(true);

                FloatingTextBaseBehavior floatingTextBehavior = floatingTextObject.GetComponent<FloatingTextBaseBehavior>();
                floatingTextBehavior.Activate(text, color);

                return floatingTextBehavior;
            }

            return null;
        }

        public static void Unload()
        {
            FloatingTextCase[] floatingTextCases = floatingTextController.floatingTextCases;
            for (int i = 0; i < floatingTextCases.Length; i++)
            {
                floatingTextCases[i].FloatingTextPool.ReturnToPoolEverything(true);
            }
        }
    }
}