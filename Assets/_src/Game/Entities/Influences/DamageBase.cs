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
        private DamageContainer m_ViewPrefab;

        private ISliceVisualizer<IInfluence> m_View;

        private IUnit m_Sender;
        private IDamageManager DamageManager => Root.Get<IDamageManager>();

        #region IInfluence
        IReadOnlyCollection<IDamage> IInfluence.Damages => m_Damages;
        void IInfluence.Activate(IUnit sender, IUnit target, float deltaTime)
        {
            if (m_ViewPrefab != null && m_ViewPrefab.Value != null)
                m_View = m_ViewPrefab.Value.Instantiate<ISliceVisualizer<IInfluence>>();

            m_View?.Init(target);

            foreach (var iter in m_Damages)
                DamageManager.Damage(m_Sender, target, iter);

            m_View?.UpdateView(target, this, deltaTime);

            target.RemoveInfluence(this);
        }

        #region ISliceInit
        void ISliceInit.Init(IUnit unit)
        {
            m_Sender = unit;
        }
        
        void ISliceInit.Done(IUnit unit)
        {
            m_Sender = null; 
        }
        #endregion

        #region ISliceUpdate
        void ISliceUpdate.Update(IUnit target, float deltaTime)
        {
        }
        #endregion
        #endregion
        public override void FillFrom(ISlice other)
        {
            if (other is DamageBase @base)
            {
                m_ViewPrefab = @base.m_ViewPrefab;
                m_Damages = new List<IDamage>(@base.m_Damages);
            }
        }
    }
}
