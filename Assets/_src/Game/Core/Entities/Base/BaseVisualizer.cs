using System;
using UnityEngine;

namespace Game.Entities.View
{

    //[RequireComponent(typeof(RootVisualizer))]
    public abstract class BaseVisualizer<I> : MonoBehaviour, ISliceVisualizer<I>
        where I: ISlice
    {
        #region ISliceVisualizer
        void ISliceVisualizer.UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            UpdateView(unit, slice, deltaTime);
        }
        #endregion

        protected abstract void UpdateView(IUnit unit, ISlice slice, float deltaTime);
    }
}