using System;
using UnityEngine;

namespace Game.Entities
{
    [Serializable]
    public sealed class Health : BaseProperty<Health>
    {
        [SerializeField]
        private float m_Value;
        [SerializeField]
        public float m_Regen = 1;
        [SerializeField]
        public float m_RegenDuration = 10;

        private float m_CurrentStagger = 0;

        protected override float GetValue() => m_Value;

        protected override void Update(IUnit unit, float deltaTime)
        {
            base.Update(unit, deltaTime);
            if (!Owner.IsDead && m_Regen > 0 && m_Damage > 0 && m_CurrentStagger <= 0)
            {
                m_Damage -= m_Regen * deltaTime;
                m_Damage = Mathf.Clamp(m_Damage, 0, m_Value);
            }
            m_CurrentStagger -= deltaTime;
        }

        protected override void OnDamage(IUnit sender)
        {
            m_CurrentStagger = m_RegenDuration;
            if ((this as IProperty).Value <= 0)
                Owner.SetDead(0);
        }

        public override void FillFrom(ISlice other)
        {
            base.FillFrom(other);
            if (other is Health health)
            {
                m_Value = health.m_Value;
                m_Regen = health.m_Regen;
                m_RegenDuration = health.m_RegenDuration;
            }
        }
    }
}