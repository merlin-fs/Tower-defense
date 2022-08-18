using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;

namespace Game.Model.Units.Skills
{
    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial class TestParticleSystem : SystemBase
    {
        private EntityCommandBufferSystem m_CommandBuffer;
        private EntityQuery m_Query;
        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<ParticleSpawner>(),
                ComponentType.ReadOnly<StateShot>()
            );
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            var childs = GetBufferFromEntity<Child>();
            var entities = m_Query.ToEntityArray(Allocator.Temp);
            var writer = m_CommandBuffer.CreateCommandBuffer();
            try
            {
                foreach (var iter in entities)
                {
                    RecursiveChilds(iter, childs,
                        (child) =>
                        {
                            if (EntityManager.HasComponent<ParticleSystem>(child))
                            {
                                var ps = EntityManager.GetComponentObject<ParticleSystem>(child);
                                ps.Play();
                            }
                        });

                    writer.RemoveComponent<StateShot>(iter);
                    //var ps = EntityManager.GetComponentObject<ParticleSystem>(iter);
                }
            }
            finally
            {
                entities.Dispose();
            }
        }

        void RecursiveChilds(Entity entity, BufferFromEntity<Child> childs, Action<Entity> action)
        {
            action?.Invoke(entity);
            if (!childs.HasComponent(entity))
                return;
            var child = childs[entity];
            for (var i = 0; i < child.Length; ++i)
            {
                var iter = child[i].Value;
                RecursiveChilds(iter, childs, action);
            }
        }
    }
}