using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    using View;

    [Serializable]
    public class Shooting : BaseSkill<Shooting>
    {
        [Serializable]
        private class TargetterContainer : TypedContainer<ITargetProvider> { }
        [SerializeField]
        private TargetterContainer m_TargetterPrefab;

        [SerializeField]
        private float m_SearchRate = 1;
        [SerializeField]
        private float m_EffectRate = 5;

        private float m_SearchTimer = 0f;
        private float m_EffectTimer = 0f;
        private ITargetProvider m_Targetter;
        private IUnit m_CurrrentTargetable;
        private IUnit m_Unit;

        public override void Init(IUnit unit)
        {
            base.Init(unit);
            m_Unit = unit;
            if (unit is UnitEnemy)
            {
                //unit.GameObject.GetComponent<ITargetable>();
            }
            m_SearchTimer = m_SearchRate;
            m_EffectTimer = m_EffectRate;
            m_Targetter = m_TargetterPrefab.Value.Instantiate<ITargetProvider>();
            m_Targetter.GameObject.transform.parent = unit.TargetPoint;
            m_Targetter.GameObject.transform.localPosition = Vector3.zero;
            m_Targetter.GameObject.transform.localScale = Vector3.one;
            m_Targetter.OnTargetEnterRange += OnTargetEnterRange;
            m_Targetter.OnTargetExitRange += OnTargetExitRange;
            if (m_Targetter is ITargetProviderDesign design)
                design.OnTargetDrawGizmos += OnTargetDrawGizmos;



        }

        public override void FillFrom(ISlice other)
        {
            base.FillFrom(other);
            if (other is Shooting shooting)
            {
                m_TargetterPrefab = shooting.m_TargetterPrefab;
                m_SearchRate = shooting.m_SearchRate;
                m_EffectRate = shooting.m_EffectRate;
            }
        }

        private void OnTargetDrawGizmos()
        {
            if (m_CurrrentTargetable != null && m_Unit != null)
                Gizmos.DrawLine(m_Unit.GameObject.transform.position, m_CurrrentTargetable.GameObject.transform.position);
        }

        private void OnTargetEnterRange(ITargetable targetable)
        {

        }

        private void OnTargetExitRange(ITargetable targetable)
        {
            var unit = targetable.GameObject.GetComponent<IUnit>();
            if (m_CurrrentTargetable == unit)
                m_CurrrentTargetable = null;
        }

        public override void Done(IUnit unit) 
        {
            base.Done(unit);
            if (m_Targetter is ITargetProviderDesign design)
                design.OnTargetDrawGizmos += OnTargetDrawGizmos; 
            m_Targetter.OnTargetEnterRange -= OnTargetEnterRange;
            m_Targetter.Dispose();
        }

        public override void Update(IUnit unit, float deltaTime)
        {
            m_SearchTimer -= deltaTime;
            m_EffectTimer -= deltaTime;

            if (m_SearchTimer <= 0.0f && m_CurrrentTargetable == null && m_Targetter.Targets.Count > 0)
			{
				m_CurrrentTargetable = GetNearestTargetable();
				if (m_CurrrentTargetable != null)
					m_SearchTimer = m_SearchRate;
			}

            unit.Turret?.AnimTurret(m_CurrrentTargetable);

            if (m_EffectTimer <= 0.0f && m_CurrrentTargetable != null)
            {
                View?.UpdateView(m_Unit, this, deltaTime);
                ApplyEffects(m_Unit, m_CurrrentTargetable, deltaTime);
                m_EffectTimer = m_EffectRate;
            }
        }

        //-----------------------------------------
        private IUnit GetNearestTargetable()
        {
            int length = m_Targetter.Targets.Count;
            if (length == 0)
                return null;

            IUnit nearest = null;
            float distance = float.MaxValue;
            for (int i = length - 1; i >= 0; i--)
            {
                IUnit targetable = m_Targetter.Targets[i].GameObject.GetComponent<IUnit>();
                if (targetable == null || targetable.IsDead)
                    continue;
                float currentDistance = (m_Targetter.GameObject.transform.position - targetable.GameObject.transform.position).magnitude;
                if (currentDistance < distance)
                {
                    distance = currentDistance;
                    nearest = targetable;
                }
            }

            return nearest;
        }
    }
}
