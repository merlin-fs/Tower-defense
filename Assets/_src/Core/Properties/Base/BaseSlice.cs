using System;
using St.Common.Core;

namespace TowerDefense.Core
{
    [System.Serializable]
    public abstract class BaseSlice : ISlice
    {
        protected ISlice Self => this;
        T ICoreObjectInstantiate.Instantiate<T>()
        {
            ISlice slice = (ISlice)System.Activator.CreateInstance(GetType());
            slice.FillFrom(this);
            return (T)slice;
        }

        ICoreObjectInstantiate ICoreObjectInstantiate.Instantiate()
        {
            return Self.Instantiate<ICoreObjectInstantiate>();
        }

        void IDisposable.Dispose() { }

        public abstract void FillFrom(ISlice other);
    }
}
