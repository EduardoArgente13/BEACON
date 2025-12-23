using UnityEngine;

namespace BEACON.Core
{
    /// <summary>
    /// Generic Singleton base class for MonoBehaviours.
    /// Ensures only one instance exists and provides global access.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool isApplicationQuitting = false;

        public static T Instance
        {
            get
            {
                if (isApplicationQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on application quit.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            var singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                            instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} destroyed.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the singleton is first initialized.
        /// Override this instead of Awake() in derived classes.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
