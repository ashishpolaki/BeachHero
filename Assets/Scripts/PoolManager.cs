using System.Collections.Generic;
using UnityEngine;
using System;

namespace BeachHero
{
    [System.Serializable]
    public struct PoolSettings
    {
        public GameObject Prefab;
        public int Count;
    }
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        #region Inspector Variables
        [SerializeField] private PoolSettings[] poolSettings;
        #endregion

        #region Private Variables
        private Dictionary<Type, Queue<MonoBehaviour>> _poolDictionary = new Dictionary<Type, Queue<MonoBehaviour>>();
        #endregion

        #region Unity Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        private void Start()
        {
              LoadObjects();
        }
        #endregion

        private void LoadObjects()
        {
            foreach (var poolSetting in poolSettings)
            {
                InitializePool(poolSetting.Prefab.GetComponent<MonoBehaviour>(), poolSetting.Count);
            }
        }

        // Initialize a pool with a specific count
        private void InitializePool<T>(T prefab, int count, Transform parent = null) where T : MonoBehaviour
        {
            var type = prefab.GetType();

            if (!_poolDictionary.ContainsKey(type))
            {
                _poolDictionary[type] = new Queue<MonoBehaviour>();
            }
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                _poolDictionary[type].Enqueue(obj);
            }
        }

        // Get an object from the pool
        public T GetPoolObject<T>() where T : MonoBehaviour
        {
            var type = typeof(T);

            if (!_poolDictionary.ContainsKey(type))
            {
                _poolDictionary[type] = new Queue<MonoBehaviour>();
            }
            if (_poolDictionary[type].Count == 0)
            {
                foreach (var poolSetting in poolSettings)
                {
                    if (poolSetting.Prefab.GetComponent<T>().GetType() == typeof(T))
                    {
                        InitializePool(poolSetting.Prefab.GetComponent<MonoBehaviour>(), poolSetting.Count);
                        break;
                    }
                }
            }
            if (_poolDictionary.ContainsKey(type) && _poolDictionary[type].Count > 0)
            {
                var obj = (T)_poolDictionary[type].Dequeue();
                return obj;
            }

            return null;
        }

        // Return an object to the pool
        public void ReturnToPool<T>(T obj) where T : MonoBehaviour
        {
            var type = typeof(T);

            if (!_poolDictionary.ContainsKey(type))
            {
                Debug.LogWarning($"No pool exists for type {type.Name}. Creating a new pool.");
                _poolDictionary[type] = new Queue<MonoBehaviour>();
            }

            obj.gameObject.SetActive(false);
            _poolDictionary[type].Enqueue(obj);
        }
    }
}
