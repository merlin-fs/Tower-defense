using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Skills
{
    using Model.Damages;


    [Defineable(typeof(Cannon))]
    public class CannonDef : BaseSkillDef
    {
        public float Distance = 50f;
        public float FindDelay = 1f;
        public float Frequency = 2f;

        [SerializeField, SelectChildPrefab]
        private GameObject m_Particle;
        public ParticleSystem ParticleSystem => m_Particle.GetComponent<ParticleSystem>();

        [SerializeReference, Reference()]
        private IDamageDef[] m_Damages;
        public IReadOnlyCollection<IDamageDef> Damages => m_Damages;

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            var child = conversionSystem.GetPrimaryEntity(m_Particle);
            manager.AddComponentData<ParticleSpawner>(entity, child);
        }

    }
}