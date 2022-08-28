using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Model.Parts
{
    public class TurretDef : BasePartDef<Turret>
    {
        [SerializeField, SelectChildPrefab]
        private GameObject m_Part;

        [SerializeField]
        private float m_IdleRotationSpeed = 30f;
        [SerializeField]
        private float m_IdleCorrectionTime = 2.0f;
        [SerializeField]
        private float m_IdleWaitTime = 2.0f;
        [SerializeField]
        private float2 m_RotationRange = new float2(-100, 100);

        public float IdleRotationSpeed => m_IdleRotationSpeed;
        public float IdleCorrectionTime => m_IdleCorrectionTime;
        public float IdleWaitTime => m_IdleWaitTime;
        public float2 RotationRange => m_RotationRange;

        protected override void InitializeDataConvert(ref Turret value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.InitializeDataConvert(ref value, entity, manager, conversionSystem);
            if (m_Part)
                value.Entity = conversionSystem.GetPrimaryEntity(m_Part);
        }
    }
}