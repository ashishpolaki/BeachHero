using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "ScriptableObjectPool", menuName = "Scriptable Objects/ScriptableObjectPool")]
    public class ScriptableObjectPool : ScriptableObject
    {
        public GameObject prefab;
        public int poolSize;

        private Queue<GameObject> pool;
        private Transform poolParent;
        [SerializeField] private bool isInitialised;

        public void Initialise()
        {
            if (isInitialised)
            {
                return;
            }
            pool ??= new Queue<GameObject>();
            if (poolParent == null)
            {
                GameObject parentGO = new GameObject($"{name}_Pool");
                DontDestroyOnLoad(parentGO);
                poolParent = parentGO.transform;
            }
            isInitialised = true;
            for (int i = 0; i < poolSize; i++)
            {
                CreateInstance();
            }
        }

        private void CreateInstance()
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.SetActive(false);
            pool.Enqueue(obj);
             Debug.Log($"[{name}] pool count: {pool.Count}");
        }

        public GameObject GetObject()
        {
            if (!isInitialised)
            {
                Initialise();
            }
            Debug.Log($"[{name}] pool count: {pool.Count}");

            if (pool == null)
            {
                isInitialised = false;
                Initialise();
                Debug.Log($"{name} pool is null");
                return null;
            }
            if (pool.Count == 0)
            {
                isInitialised = false;
                Initialise();
            }
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolParent);
            pool.Enqueue(obj);
        }

        public void ResetState()
        {
            if (pool != null)
            {
                // $"[{name}] pool is cleared".Log();
                foreach (var obj in pool)
                {
                    if (obj != null)
                        GameObject.Destroy(obj);
                }
            }
            isInitialised = false;
            pool?.Clear();
        }
    }
}
