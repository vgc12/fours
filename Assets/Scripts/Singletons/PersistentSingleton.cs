using System;
using UnityEngine;

namespace Singletons
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;
        public bool AutoUnparentOnAwake = true;

        public static bool HasInstance => Instance != null;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                try
                {
                    instance = FindAnyObjectByType<T>() ?? null;
                }
                catch (Exception e)
                {
                    // ignored
                }


                if (instance != null) return instance;
                try
                {
                    var go = new GameObject(typeof(T).Name + " Auto-Generated");
                    instance = go.AddComponent<T>();
                }
                catch (Exception _)
                {
                    // ignored
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        public static T TryGetInstance()
        {
            return HasInstance ? Instance : null;
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            if (AutoUnparentOnAwake) transform.SetParent(null);

            if (!instance)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}