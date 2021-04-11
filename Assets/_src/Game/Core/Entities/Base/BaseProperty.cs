using System;
using System.Collections.Generic;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities
{
    using View;

    [System.Serializable]
    public abstract class BaseProperty<T> : BaseSlice, IProperty
        where T : BaseProperty<T>
    {
        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Absorbs = new List<IDamage>();

        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Resists = new List<IDamage>();

        private ISliceVisualizer<T> m_View;

        protected float m_Damage;

        protected IUnit Owner { get; private set; }

        protected ISliceVisualizer<T> View => m_View;


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
        float IProperty.Normalize => Mathf.InverseLerp(0, GetValue(), (this as IProperty).Value);
        #endregion
        #region  ISliceUpdate
        void ISliceUpdate.Update(IUnit unit, float deltaTime) => Update(unit, deltaTime);
        #endregion
        #region  ISliceInit
        void ISliceInit.Init(IUnit unit)
        {
            Owner = unit;
            m_View = unit.GameObject.GetComponentInChildren<ISliceVisualizer<T>>();
            Init(unit);
            View?.Init(unit);
        }

        void ISliceInit.Done(IUnit unit)
        {
            View?.Done(unit);
            Done(unit);
        }
        #endregion

        public override void FillFrom(ISlice other)
        {
            if (other is BaseProperty<T> prop)
            {
                m_Absorbs = new List<IDamage>(prop.m_Absorbs);
                m_Resists = new List<IDamage>(prop.m_Resists);
            }
        }

        protected abstract float GetValue();

        protected abstract void OnDamage(IUnit sender);

        protected virtual void Init(IUnit unit) { }

        protected virtual void Done(IUnit unit) { }

        protected virtual void Update(IUnit unit, float deltaTime)
        {
            View?.UpdateView(unit, this, deltaTime);
        }
    }
}
