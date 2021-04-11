using System;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities.View
{
    public abstract class BaseVisualizer<I> : BaseMonoSlice, ISliceVisualizer<I>
        where I: ISlice
    {
        #region ISliceVisualizer
        void ISliceVisualizer.UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            UpdateView(unit, slice, deltaTime);
        }
        #endregion
        #region ISliceInit
        void ISliceInit.Init(IUnit unit) => Init(unit);
        void ISliceInit.Done(IUnit unit) => Done(unit);
        #endregion
        protected abstract void Init(IUnit unit);
        protected abstract void Done(IUnit unit);
        protected abstract void UpdateView(IUnit unit, ISlice slice, float deltaTime);
    }
}