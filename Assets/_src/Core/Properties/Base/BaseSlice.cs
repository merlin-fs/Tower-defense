using System;
using Common.Core;

namespace TowerDefense.Core
{
    using View;

    [System.Serializable]
    public abstract class BaseSlice: ISlice
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
