using System;
using UnityEngine;

namespace TowerDefense
{
    using Core;
    using Core.View;

    public class DamageBase : BaseSlice, IInfluence
    {

        [Serializable]
        private class DamageContainer : TypedContainer<ISliceVisualizer<IInfluence>> { }
        [SerializeField]
        private DamageContainer m_DamageView;

        private Health m_Health;
        private float m_Damage = 1f;
        public void Apply(IUnit target)
        {
            m_DamageView.Value.UpdateView(target, this, Time.deltaTime);
        }
        public void Done(IUnit unit)
        {
            unit.RemoveInfluence(this);
        }
        public override void FillFrom(ISlice other)
        {
            //base.FillFrom(other);
            if (other is DamageBase @base)
                m_DamageView = @base.m_DamageView;
        }
    }
}
