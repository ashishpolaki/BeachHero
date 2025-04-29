using System.Reflection;
using UnityEngine;

namespace BeachHero
{
    public class SingleTon<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static bool LazyCreate
        {
            get
            {
                return SingleTon<T>.lazyCreate;
            }
            set
            {
                SingleTon<T>.lazyCreate = value;
            }
        }

        public static T CreateInstance()
        {
            if (SingleTon<T>.instance == null)
            {
                SingleTon<T>.lazyCreate = true;
            }
            PropertyInfo property = typeof(T).GetProperty("GetInstance");
            if (property != null)
            {
                return property.GetValue(null, null) as T;
            }
            return SingleTon<T>.GetInstance;
        }

        public static void Destroy()
        {
            if (SingleTon<T>.instance != null)
            {
                UnityEngine.Object.Destroy(SingleTon<T>.instance.gameObject);
                SingleTon<T>.instance = (T)((object)null);
            }
            SingleTon<T>.lazyCreate = false;
        }

        public static bool Exists
        {
            get
            {
                return SingleTon<T>.instance != null;
            }
        }

        public static T GetInstance
        {
            get
            {

                if (SingleTon<T>.instance == null)
                {

                    SingleTon<T>.instance = (UnityEngine.Object.FindFirstObjectByType(typeof(T)) as T);
                    if (SingleTon<T>.instance != null)
                    {
                        SingleTon<T>.lazyCreate = false;
                    }
                    else if (SingleTon<T>.lazyCreate)
                    {
                        SingleTon<T>.lazyCreate = false;
                        string text = typeof(T).ToString();
                        GameObject gObject = null;
                        if (gObject == null)
                        {
                            gObject = (GameObject)Resources.Load(text, typeof(GameObject));
                        }
                        if (gObject != null)
                        {
                            GameObject gObject2 = UnityEngine.Object.Instantiate(gObject, Vector3.zero, Quaternion.identity) as GameObject;
                            gObject2.SetActive(true);
                            SingleTon<T>.instance = gObject2.GetComponent<T>();
                        }
                        if (SingleTon<T>.instance == null)
                        {
                            SingleTon<T>.instance = new GameObject(text).AddComponent<T>();
                        }
                    }
                    if (SingleTon<T>.instance != null)
                    {
                        SingleTon<T>.instance.gameObject.name = typeof(T).ToString();
                        UnityEngine.Object.DontDestroyOnLoad(SingleTon<T>.instance.gameObject);
                        UnityEngine.Object.DontDestroyOnLoad(SingleTon<T>.instance);
                    }
                }
                SingleTon<T>.instance.gameObject.SetActive(true);
                return SingleTon<T>.instance;
            }
        }

        public void InitialiseInstance()
        {
            if (SingleTon<T>.instance == null)
            {
                SingleTon<T>.instance = (T)((object)this);
                SingleTon<T>.instance.gameObject.SetActive(true);
                SingleTon<T>.instance.gameObject.name = typeof(T).ToString();
                UnityEngine.Object.DontDestroyOnLoad(SingleTon<T>.instance.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(SingleTon<T>.instance);
            }
        }

        private static T instance;

        private static bool lazyCreate = true;
    }
}
