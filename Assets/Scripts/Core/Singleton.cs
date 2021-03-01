using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Singleton class
    /// </summary>
    /// <typeparam name="T">Type of the singleton</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : class
    {
        /// <summary>
        /// The static reference to the instance
        /// </summary>
        public static T Inst { get; protected set; }

        /// <summary>
        /// Gets whether an instance of this singleton exists
        /// </summary>
        public static bool IsExists
        {
            get { return Inst != null; }
        }

        /// <summary>
        /// Awake method to associate singleton with instance
        /// </summary>
        protected virtual void Awake()
        {
            if (IsExists)
                Destroy(gameObject);
            else
                Inst = (T)(object)this;
        }

        /// <summary>
        /// OnDestroy method to clear singleton association
        /// </summary>
        protected virtual void OnDestroy()
        {
            if ((object)Inst == this)
                Inst = null;
        }
    }
}