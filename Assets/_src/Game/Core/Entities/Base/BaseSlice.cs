using System;
using St.Common.Core;

namespace Game.Entities
{
    using View;

    [System.Serializable]
    public abstract class BaseSlice: ISlice
    {
        protected ISlice Self => this;
        
        T ICoreInstantiate.Instantiate<T>()
        {
            ISlice slice = (ISlice)System.Activator.CreateInstance(GetType());
            slice.FillFrom(this);
            return (T)slice;
        }

        ICoreInstantiate ICoreInstantiate.Instantiate()
        {
            return Self.Instantiate<ICoreInstantiate>();
        }

        public event Action<ICoreDisposable> OnDispose;
        
        void ICoreDisposable.Dispose() 
        {
            OnDispose?.Invoke(this);
        }

        public abstract void FillFrom(ISlice other);
    }

}
