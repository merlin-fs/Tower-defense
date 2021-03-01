using System;
using System.Collections.Generic;
using St.Common.Core;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense
{
    using Targetting;
    using Core.View;

    [Serializable]
    public class Shooting : BaseSkill
    {
        [Serializable]
        private class ViewContainer : TypedContainer<ISliceVisualizer<ISlice>> { }
        [SerializeField]
        private ViewContainer m_View;

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

            if (m_Targetter is ITargetProviderDesign design)
                design.OnTargetDrawGizmos += OnTargetDrawGizmos;
        }

        private void OnTargetDrawGizmos()
        {
            if (m_CurrrentTargetable != null && m_Unit != null)
                Gizmos.DrawLine(m_Unit.GameObject.transform.position, m_CurrrentTargetable.GameObject.transform.position);
        }

        private void OnTargetEnterRange(ITargetable targetable)
        {

        }

        public override void Done(IUnit unit) 
        {
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

            if (m_EffectTimer <= 0.0f && m_CurrrentTargetable != null)
            {
                foreach (IInfluence iter in Effects)
                {
                    m_CurrrentTargetable.AddInfluence(iter);
                    iter.Apply(m_CurrrentTargetable);
                    m_View?.Value?.UpdateView(m_Unit, this, deltaTime);
                }
                m_EffectTimer = m_EffectRate;
            }
            unit.Turret?.AnimTurret(m_CurrrentTargetable);
        }

        public override void FillFrom(ISlice other)
        {
            base.FillFrom(other);
            if (other is Shooting shooting)
            {
                m_TargetterPrefab = shooting.m_TargetterPrefab;
                m_SearchRate = shooting.m_SearchRate;
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
