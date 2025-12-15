using UnityEngine;

namespace Singletons
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;

        public static bool HasInstance => Instance != null;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = FindAnyObjectByType<T>();

                if (instance != null) return instance;

                var go = new GameObject(typeof(T).Name + " Auto-Generated");
                instance = go.AddComponent<T>();
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
            instance = this as T;
        }
    }
}