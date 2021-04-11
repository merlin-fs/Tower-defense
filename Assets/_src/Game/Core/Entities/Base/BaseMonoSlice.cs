using System;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities
{
    using View;

    [System.Serializable]
    public abstract class BaseMonoSlice: MonoBehaviour, ISlice, ICoreMonoObject
    {
        private ISlice Self => this;

        #region ISlice
        void ISlice.FillFrom(ISlice other) => FillFrom(other);
        #endregion
        #region ICoreInstantiate
        T ICoreInstantiate.Instantiate<T>()
        {
            return Instantiate(this).GetComponent<T>();
        }

        ICoreInstantiate ICoreInstantiate.Instantiate()
        {
            return Self.Instantiate<ICoreInstantiate>();
        }
        #endregion

        #region ICoreDisposable
        public event Action<ICoreDisposable> OnDispose;
        
        void ICoreDisposable.Dispose() 
        {
            OnDispose?.Invoke(this);
            Dispose();
        }
        #endregion
        #region ICoreMonoObject
        GameObject ICoreMonoObject.GameObject => gameObject;
        #endregion
        protected virtual void Dispose() { }
        protected virtual void FillFrom(ISlice other) { }
    }
}
