using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    using Core;
    using View;

    public class DamageFire : BaseSlice, IInfluence
    {
        [SerializeReference, SubclassSelector(typeof(IDamage))]
        private List<IDamage> m_Damages = new List<IDamage>();

        [Serializable]
        private class DamageContainer : TypedContainer<ISliceVisualizer<IInfluence>> { }
        [SerializeField]
        private DamageContainer m_ViewPrefab;

        [SerializeField]
        private float m_Time = 5f;

        [SerializeField]
        private float m_DotTime = 0.5f;

        private float m_CurrenTime;
        private float m_FullTime;

        private ISliceVisualizer<IInfluence> m_View;

        private IUnit m_Sender;
        private IDamageManager DamageManager => Root.Get<IDamageManager>();

        #region IInfluence
        IReadOnlyCollection<IDamage> IInfluence.Damages => m_Damages;

        void IInfluence.Activate(IUnit sender, IUnit target, float deltaTime)
        {
            m_CurrenTime = 0;
            m_FullTime = 0;
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
            m_FullTime += deltaTime;
            if (m_FullTime < m_Time)
            {
                m_CurrenTime += deltaTime;
                int dotCount = Mathf.CeilToInt(m_CurrenTime / m_DotTime);
                for (int i = 0; i < dotCount; i++)
                {
                    foreach (var iter in m_Damages)
                        DamageManager.Damage(m_Sender, target, iter);
                }
                if (dotCount > 0)
                    m_CurrenTime -= dotCount * m_DotTime;
            }
            else
                target.RemoveInfluence(this);
        }
        #endregion
        #endregion
        public override void FillFrom(ISlice other)
        {
            if (other is DamageFire @base)
            {
                m_ViewPrefab = @base.m_ViewPrefab;
                m_Damages = new List<IDamage>(@base.m_Damages);
            }
        }
    }
}
