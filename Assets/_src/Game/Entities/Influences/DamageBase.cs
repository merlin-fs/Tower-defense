using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    using Core;
    using View;

    public class DamageBase : BaseSlice, IInfluence
    {
        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Damages = new List<IDamage>();
        [Serializable]
        private class DamageContainer : TypedContainer<ISliceVisualizer<IInfluence>> { }
        [SerializeField]
        private DamageContainer m_DamageView;
        private IDamageManager DamageManager => Root.Get<IDamageManager>();

        #region IInfluence
        IReadOnlyCollection<IDamage> IInfluence.Damages => m_Damages;

        void IInfluence.Apply(IUnit sender, IUnit target)
        {
            foreach (var iter in m_Damages)
                DamageManager.Damage(sender, target, iter);
            m_DamageView.Value.UpdateView(target, this, Time.deltaTime);
        }
        #endregion
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
