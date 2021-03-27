using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEngine
{
    public abstract class SingletonMono<T> : MonoBehaviour
        where T : SingletonMono<T>
    {
        static SingletonMono()
        {
            m_Awoken = new HashSet<T>();
        }

        public static T Inst => GetInst();

        private static readonly object m_Lock = new object();
        private static T m_Inst;
        private static readonly HashSet<T> m_Awoken;

        private static T GetInst()
        {
            lock (m_Lock)
            {
                if (Application.isPlaying)
                {
                    if (m_Inst == null)
                    {
                        Init();
                    }
                    return m_Inst;
                }
                else
                {
                    return Init();
                }
            }
        }

        private static T[] FindInstances()
        {
            // Fails here on hidden hide flags
            return UnityObject.FindObjectsOfType<T>();
        }

        private static T Init()
        {
            lock (m_Lock)
            {
                var instances = FindInstances();

                if (instances.Length == 1)
                {
                    m_Inst = instances[0];
                }
                else if (instances.Length == 0)
                {
                    // Create the parent game object with the proper hide flags
                    var singleton = new GameObject(typeof(T).Name);
                    //singleton.hideFlags = hideFlags;

                    // Instantiate the component, letting Awake assign the real instance variable
                    var _instance = singleton.AddComponent<T>();
                    //_instance.hideFlags = hideFlags;

                    // Sometimes in the editor, for example when creating a new scene,
                    // AddComponent seems to call Awake add a later frame, making this call
                    // fail for exactly one frame. We'll force-awake it if need be.
                    Awake(_instance);
                }
                else if (instances.Length > 1)
                {
                    throw new UnityException($"More than one '{typeof(T)}' singleton in the scene.");
                }

                return m_Inst;
            }
        }
        public static void Awake(T instance)
        {
            if (m_Awoken.Contains(instance))
            {
                return;
            }

            if (m_Inst != null)
            {
                throw new UnityException($"More than one '{typeof(T)}' singleton in the scene.");
            }

            m_Inst = instance;

            m_Awoken.Add(instance);
        }

        public static void OnDestroy(T instance)
        {
            //Ensure.That(nameof(instance)).IsNotNull(instance);
            if (m_Inst == instance)
            {
                m_Inst = null;
            }
            else
            {
                throw new UnityException($"Trying to destroy invalid instance of '{typeof(T)}' singleton.");
            }
        }
    }

}