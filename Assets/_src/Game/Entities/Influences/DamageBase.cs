using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    using View;

    public class DamageBase : BaseSlice, IInfluence
    {
        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Damages = new List<IDamage>();
        [Serializable]
        private class DamageContainer : TypedContainer<ISliceVisualizer<IInfluence>> { }
        [SerializeField]
        private DamageContainer m_DamageView;

        IReadOnlyCollection<IDamage> IInfluence.Damages => m_Damages;

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
            if (other is DamageBase @base)
            {
                m_DamageView = @base.m_DamageView;
                m_Damages = new List<IDamage>(@base.m_Damages);
            }
        }
    }
}
