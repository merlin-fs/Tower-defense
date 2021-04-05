using System;
using System.Collections.Generic;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities
{
    using View;

    /// <summary>
    /// Свойства объекта (Health, shield, armor и т.п.)
    /// </summary>
    public interface IProperty : ISlice, ISliceInit, ISliceUpdate, IDamaged
    {
        float Value { get; }
        float Normalize { get; }
    }


    [System.Serializable]
    public abstract class BaseProperty<T> : BaseSlice, IProperty
        where T : BaseProperty<T>
    {
        [Serializable]
        private class VisualizerContainer : TypedContainer<ISliceVisualizer<T>> { }
        [SerializeField]
        private VisualizerContainer m_View;

        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Absorbs = new List<IDamage>();

        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Resists = new List<IDamage>();

        private float m_Damage;
        protected IUnit Owner { get; private set; }

        protected ISliceVisualizer<T> View => m_View.Value;

        #region IDamaged
        IReadOnlyCollection<IDamage> IDamaged.Absorb => m_Absorbs;

        IReadOnlyCollection<IDamage> IDamaged.Resist => m_Resists;

        void IDamaged.AddDamage(IUnit sender, float value)
        {
            m_Damage += value;
            OnDamage(sender);
        }
        #endregion

        #region IProperty
        float IProperty.Value => GetValue() - m_Damage;
        float IProperty.Normalize => Mathf.InverseLerp(GetMinValue(), GetMaxValue(), (this as IProperty).Value);
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
                m_Absorbs = new List<IDamage>(prop.m_Absorbs);
                m_Resists = new List<IDamage>(prop.m_Resists);
            }
        }

        protected abstract float GetValue();
        
        protected abstract float GetMaxValue();
        
        protected abstract float GetMinValue();

        protected abstract void OnDamage(IUnit sender);

        protected virtual void Init(IUnit unit)
        {
            Owner = unit;
        }
        
        protected virtual void Done(IUnit unit)
        {
            if (m_View is ICoreDisposable disposable)
                disposable.Dispose();
        }

        protected virtual void Update(IUnit unit, float deltaTime)
        {
            View?.UpdateView(unit, this, deltaTime);
        }
    }
}