using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    [System.Serializable]
    public class Health : IProperty
    {
        public float Default = 10;
        public float RegenRate = 0;
        public float StaggerDuration = 10;
        public float Value { get; private set; }
        private float m_CurrentStagger = 0;
        public void Init(Unit unit)
        {
            Value = Default;
            m_CurrentStagger = 0;
        }
        public void Update(Unit unit, float deltaTime)
        {
            if (Default > 0 && RegenRate > 0 && m_CurrentStagger <= 0)
            {
                Value += RegenRate * Time.fixedDeltaTime;
                Value = Mathf.Clamp(Value, 0, Default);
            }
            m_CurrentStagger -= Time.fixedDeltaTime;
        }
        ISlice ISlice.Clone()
        {
            return Clone();
        }
        public IProperty Clone()
        {
            return new Health()
            {
                Default = this.Default,
                RegenRate = this.RegenRate,
                StaggerDuration = this.StaggerDuration
            };
        }
    }
}