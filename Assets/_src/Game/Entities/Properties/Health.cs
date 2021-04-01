using System;
using UnityEngine;

namespace Game.Entities
{
    [Serializable]
    public class Health : BaseProperty<Health>
    {
        [SerializeField]
        public float m_Default = 10;
        [SerializeField]
        public float m_RegenRate = 0;
        [SerializeField]
        public float m_StaggerDuration = 10;
        [SerializeField]
        private float m_Value;
        protected override void Init(IUnit unit)
        {
            base.Init(unit);
            m_Value = m_Default - 5;
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
            if (other is Health health)
            {
                m_Default = health.m_Default;
                m_RegenRate = health.m_RegenRate;
                m_StaggerDuration = health.m_StaggerDuration;
            }
        }
    }
}