using UnityEngine;

namespace TowerDefense.Core
{
    [System.Serializable]
    public class Shield : BaseSlice, IProperty
    {
        public float Default = 0;
        public float RegenRate = 1;
        public float StaggerDuration = 1;
        public float Value { get; private set; }
        private float m_CurrentStagger = 0;
        public void Init(IUnit unit)
        {
            Value = Default;
            m_CurrentStagger = 0;
        }
        
        public void Done(IUnit unit) { }
        
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
        
        public void Update(IUnit unit, float deltaTime) { }
        
        public override void FillFrom(ISlice other)
        {
            if (other is Shield)
            {
                Shield shield = (other as Shield);
                Default = shield.Default;
                RegenRate = shield.RegenRate;
                StaggerDuration = shield.StaggerDuration;
            }
        }
    }
}