using System;
using UnityEngine;

namespace Game.Entities
{
    [System.Serializable]
    public class Shield : BaseProperty<Shield>
    {
        [SerializeField]
        private float m_Default = 0;
        [SerializeField]
        private float m_RegenRate = 1;
        [SerializeField]
        private float m_StaggerDuration = 1;
        [SerializeField]
        private float m_Value;
        
        protected override void Init(IUnit unit)
        {
            base.Init(unit);
            m_Value = m_Default;
        }

        protected override float GetValue() => m_Value;

        protected override float GetMaxValue() => m_Default;

        protected override float GetMinValue() => 0;

        /*
        public void FixedUpdate(Unit unit, float deltaTime)
        {
            if (Default > 0 && RegenRate > 0 && m_CurrentStagger <= 0)
            {
                Value += RegenRate * Time.fixedDeltaTime;
                Value = Mathf.Clamp(Value, 0, Default);
            }
            m_CurrentStagger -= Time.fixedDeltaTime;
        }
        */

        public override void FillFrom(ISlice other)
        {
            base.FillFrom(other);
            if (other is Shield shield)
            {
                m_Default = shield.m_Default;
                m_RegenRate = shield.m_RegenRate;
                m_StaggerDuration = shield.m_StaggerDuration;
            }
        }
    }
}