using System;
using System.Linq;
using Common.Core;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Targetting
{
    
    public interface ITargetProviderDesign
    {
        event Action OnTargetDrawGizmos;
    }

    public interface ITargetProvider : ICoreGameObjectInstantiate, IDisposable
    {
        event Action<ITargetable> OnTargetEnterRange;
        event Action<ITargetable> OnTargetExitRange;
        IReadOnlyList<ITargetable> Targets { get; }
    }

    [RequireComponent(typeof(Collider))]
    public class TargetProvider : MonoBehaviour, ITargetProvider, ITargetProviderDesign
    {
        private HashSet<ITargetable> m_Targetables = new HashSet<ITargetable>();
        ITargetProvider Self => this;

        void OnDrawGizmos()
        {
            OnTargetDrawGizmos?.Invoke();
        }

        /*
        void Test()
        {
            Collider col = GetComponent<Collider>();
            col.
        }
        */

        private void OnCollisionEnter(Collision collision)
        {

        }

        private void OnCollisionExit(Collision collision)
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            var targetable = other.GetComponent<ITargetable>();
            if (!m_Targetables.Contains(targetable) && targetable != null)
            {
                m_Targetables.Add(targetable);
                OnTargetEnterRange?.Invoke(targetable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var targetable = other.GetComponent<ITargetable>();
            if (m_Targetables.Contains(targetable))
            {
                m_Targetables.Remove(targetable);
                OnTargetExitRange?.Invoke(targetable);
            }
        }

        public event Action OnTargetDrawGizmos;

        #region ITargetProvider
        public event Action<ITargetable> OnTargetEnterRange;
        public event Action<ITargetable> OnTargetExitRange;
        IReadOnlyList<ITargetable> ITargetProvider.Targets => m_Targetables.ToList();
        #endregion
        #region ICoreObjectInstantiate
        ICoreObjectInstantiate ICoreObjectInstantiate.Instantiate()
        {
            return Instantiate(this);
        }

        T ICoreObjectInstantiate.Instantiate<T>()
        {
            return (T)Self.Instantiate();
        }
        #endregion
        #region ICoreGameObject
        GameObject ICoreGameObject.GameObject => gameObject;
        #endregion
        #region IDisposable
        void IDisposable.Dispose()
        {

        }
        #endregion
    }
}