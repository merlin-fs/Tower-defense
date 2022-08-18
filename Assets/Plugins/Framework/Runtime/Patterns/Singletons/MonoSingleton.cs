using System;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Common.Singletons
{
    /// <remarks>
    /// Does not support objects hidden with hide flags.
    /// </remarks>
    [Singleton()]
    public class MonoSingleton<T>: MonoBehaviour, ISingleton
        where T : MonoBehaviour, ISingleton
    {
        static MonoSingleton()
        {
            s_Awoken = new HashSet<T>();
            s_Attribute = (SingletonAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(SingletonAttribute));
        }

        private static readonly SingletonAttribute s_Attribute;

        private static bool Persistent => s_Attribute.Persistent;

        private static bool Automatic => s_Attribute.Automatic;

        private static string Name => s_Attribute.Name;

        private static HideFlags HideFlags => s_Attribute.HideFlags;

        private static readonly object s_Lock = new object();

        private static readonly HashSet<T> s_Awoken;

        private static T s_Instance;

        private static bool s_IsDestroy = false;

        public static bool IsInstantiated
        {
            get
            {
                lock (s_Lock)
                {
                    return Application.isPlaying 
                        ? s_Instance != null 
                        : FindInstances().Length == 1;
                }
            }
        }

        protected static T Inst
        {
            get
            {
                lock (s_Lock)
                {
                    if (Application.isPlaying)
                    {
                        if (s_Instance == null)
                        {
                            Instantiate();
                        }
                        return s_Instance;
                    }
                    else
                    {
                        return Instantiate();
                    }
                }
            }
        }

        private static T[] FindInstances()
        {
            return UnityObject.FindObjectsOfType<T>();
        }

        private static T Instantiate()
        {
            lock (s_Lock)
            {
                if (s_IsDestroy)
                    return null;

                var instances = FindInstances();
                if (instances.Length == 1)
                {
                    var instance = instances[0];
                    Awake(instance);
                }
                else if (instances.Length == 0)
                {
                    if (Automatic)
                    {
                        // Create the parent game object with the proper hide flags
                        var singleton = new GameObject(Name ?? typeof(T).Name)
                        {
                            hideFlags = HideFlags
                        };

                        // Instantiate the component, letting Awake assign the real instance variable
                        var instance = singleton.AddComponent(typeof(T));
                        instance.hideFlags = HideFlags;

                        // Sometimes in the editor, for example when creating a new scene,
                        // AddComponent seems to call Awake add a later frame, making this call
                        // fail for exactly one frame. We'll force-awake it if need be.
                        Awake(s_Instance);
                    }
                    else
                    {
                        throw new UnityException($"Missing '{typeof(T)}' singleton in the scene.");
                    }
                }
                else if (instances.Length > 1)
                {
                    throw new UnityException($"More than one '{typeof(T)}' singleton in the scene.");
                }

                return s_Instance;
            }
        }

        private static void Awake([NotNull] T instance)
        {
            if (s_Awoken.Contains(instance))
            {
                return;
            }

            if (s_Instance != null)
            {
                throw new UnityException($"More than one '{typeof(T)}' singleton in the scene.");
            }

            s_Instance = instance;
            s_Awoken.Add(instance);
        }

        private static void OnDestroy([NotNull] T instance)
        {
            if (s_Instance == default)
                return;
            s_Instance = s_Instance.Equals(instance)
                ? default(T)
                : throw new UnityException($"Trying to destroy invalid instance of '{typeof(T)}' singleton.");
            s_IsDestroy = true;
        }

        protected virtual void Awake()
        {
            Awake(Inst);
            // Make the singleton persistent if need be
            if (Persistent && Application.isPlaying)
            {
                UnityObject.DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            OnDestroy(this as T);
        }

        protected virtual void OnApplicationQuit()
        {
            OnDestroy(this as T);
        }
    }
}
