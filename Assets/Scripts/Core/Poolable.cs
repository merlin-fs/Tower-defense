using UnityEngine;
using St.Common.Core;

namespace TowerDefense.Core
{
    /// <summary>
    /// Class that is to be pooled
    /// </summary>

    [RequireComponent(typeof(ICoreObjectInstantiate))]
    public class Poolable : MonoBehaviour
    {
        /// <summary>
        /// Number of poolables the pool will initialize
        /// </summary>
        public int initialPoolCapacity = 10;

        /// <summary>
        /// Pool that this poolable belongs to
        /// </summary>
        public Pool<Poolable> pool;

        /// <summary>
        /// Repool this instance, and move us under the poolmanager
        /// </summary>
        protected virtual void Repool()
        {
            transform.SetParent(PoolManager.Inst.GameObject.transform, false);
            pool.Return(this);
        }

        /// <summary>gameObject
        /// Pool the object if possible, otherwise destroy it
        /// </summary>
        /// <param name="gameObject">GameObject attempting to pool</param>
        public static void TryPool(GameObject gameObject)
        {
            var poolable = gameObject.GetComponent<Poolable>();
            if (poolable != null && poolable.pool != null && PoolManager.IsExists)
                poolable.Repool();
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// If the prefab is poolable returns a pooled object otherwise instantiates a new object
        /// </summary>
        /// <param name="prefab">Prefab of object required</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>The pooled or instantiated component</returns>
        public static I TryGetPoolable<I>(GameObject prefab) where I : ICoreObject
        {
            var poolable = prefab.GetComponent<ICoreObjectInstantiate>();

            I instance = (poolable != null && PoolManager.IsExists)
                ? (I)PoolManager.Inst.GetPoolable(poolable)
                : poolable.Instantiate<I>();
            return instance;
        }

        /// <summary>
        /// If the prefab is poolable returns a pooled object otherwise instantiates a new object
        /// </summary>
        /// <param name="prefab">Prefab of object required</param>
        /// <returns>The pooled or instantiated gameObject</returns>
        public static ICoreObjectInstantiate TryGetPoolable(GameObject prefab)
        {
            var poolable = prefab.GetComponent<ICoreObjectInstantiate>();
            ICoreObjectInstantiate instance = poolable != null && PoolManager.IsExists
                ? PoolManager.Inst.GetPoolable(poolable)
                : poolable.Instantiate();

            return instance;
        }
    }
}