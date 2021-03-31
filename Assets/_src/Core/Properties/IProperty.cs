using System;
using UnityEngine;

namespace TowerDefense.Core
{
    using View;

    public interface IProperty : ISlice, ISliceInit, ISliceUpdate
    {
        float Value { get; }
    }


    [System.Serializable]
    public abstract class BaseProperty<T> : BaseSlice, IProperty
        where T : BaseProperty<T>
    {
        [Serializable]
        private class VisualizerContainer : TypedContainer<ISliceVisualizer<T>> { }
        [SerializeField]
        private VisualizerContainer m_View;

        protected ISliceVisualizer<T> View => m_View.Value;

        #region  IProperty
        float IProperty.Value => Mathf.InverseLerp(GetMinValue(), GetMaxValue(), GetValue());
        #endregion
        #region  ISliceUpdate
        void ISliceUpdate.Update(IUnit unit, float deltaTime) => Update(unit, deltaTime);
        #endregion
        #region  ISliceInit
        void ISliceInit.Init(IUnit unit) => Init(unit);
        void ISliceInit.Done(IUnit unit) => Done(unit);
        #endregion

        public override void FillFrom(ISlice other)
        {
            if (other is BaseProperty<T> prop)
            {
                m_View = prop.m_View;
            }
        }

        protected abstract float GetValue();
        
        protected abstract float GetMaxValue();
        
        protected abstract float GetMinValue();
        
        protected virtual void Init(IUnit unit)
        {

        }
        
        protected virtual void Done(IUnit unit)
        {
            if (m_View is IDisposable disposable)
                disposable.Dispose();
        }

        protected virtual void Update(IUnit unit, float deltaTime)
        {
            View?.UpdateView(unit, this, deltaTime);
        }
    }
}