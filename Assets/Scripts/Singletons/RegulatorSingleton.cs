using UnityEngine;

namespace Singletons
{
    public class RegulatorSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;

        public float InitializationTime { get; private set; }

        public static bool HasInstance => Instance != null;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = FindAnyObjectByType<T>();

                if (instance != null) return instance;

                var go = new GameObject(typeof(T).Name + " Auto-Generated");
                go.hideFlags = HideFlags.HideAndDontSave;
                instance = go.AddComponent<T>();
                return instance;
            }
        }

        protected void Awake()
        {
            InitializeSingleton();
        }


        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            InitializationTime = Time.time;

            DontDestroyOnLoad(gameObject);

            var oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (var old in oldInstances)
                if (old.GetComponent<RegulatorSingleton<T>>().InitializationTime < InitializationTime)
                    Destroy(old.gameObject);

            if (!instance) instance = this as T;
        }
    }
}