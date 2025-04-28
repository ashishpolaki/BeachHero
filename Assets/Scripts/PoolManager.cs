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
        [SerializeField] private PoolSettings[] poolSettings;

        private Dictionary<Type, Queue<MonoBehaviour>> _pools = new Dictionary<Type, Queue<MonoBehaviour>>();
        public bool isEnableCharacter;

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
        private void Update()
        {
            if (isEnableCharacter)
            {
                isEnableCharacter = false;

                CharacterHero character = GetPoolObject<CharacterHero>();

                if (character != null)
                {
                    character.gameObject.SetActive(true);
                    Debug.Log("Got object from pool.");
                }
                else
                {
                    Debug.LogWarning("No available objects in the pool.");
                }
            }
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

            if (!_pools.ContainsKey(type))
            {
                _pools[type] = new Queue<MonoBehaviour>();

                for (int i = 0; i < count; i++)
                {
                    var obj = Instantiate(prefab, parent);
                    obj.gameObject.SetActive(false);
                    _pools[type].Enqueue(obj);
                }
            }
        }

        // Get an object from the pool
        public T GetPoolObject<T>() where T : MonoBehaviour
        {
            var type = typeof(T);

            if (_pools.ContainsKey(type) && _pools[type].Count > 0)
            {
                var obj = (T)_pools[type].Dequeue();
                return obj;
            }

            Debug.LogWarning($"No objects available in the pool for type {type.Name}.");
            return null;
        }

        // Return an object to the pool
        public void ReturnToPool<T>(T obj) where T : MonoBehaviour
        {
            var type = typeof(T);

            if (!_pools.ContainsKey(type))
            {
                Debug.LogWarning($"No pool exists for type {type.Name}. Creating a new pool.");
                _pools[type] = new Queue<MonoBehaviour>();
            }

            obj.gameObject.SetActive(false);
            _pools[type].Enqueue(obj);
        }
    }
}
