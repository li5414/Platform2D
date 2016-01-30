using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T sInstance;
        protected static bool sAwakeOverwritable = false;

        public static T SetParent(Transform parent)
        {
            if (parent != null)
            {
                Transform me = Instance.transform;
                me.SetParent(parent);
            }
            
            return Instance;
        }
        
        public static void SetParent( GameObject parent)
        {
            SetParent( parent.transform );
        }

        static public T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = FindObjectOfType<T>();

                    if (sInstance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        DontDestroyOnLoad(go);
                        sInstance = go.AddComponent<T>();
                    }
                }

                return sInstance;
            }
        }

        virtual protected void Awake()
        {
            if (sInstance == null)
            {
                DontDestroyOnLoad(gameObject);
                sInstance = GetComponent<T>();
            }
            else if (sInstance != this)
            {
                if (sAwakeOverwritable)
                {
                    Destroy(sInstance.gameObject);
                    sInstance = GetComponent<T>();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
