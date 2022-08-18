using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace Game.UI
{
    using Game.Model.Units;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateInGroup(typeof(GameTransformSystemGroup))]
    
    public partial class TestUISystem : SystemBase
    {
        private EntityQuery m_Query;

        public Canvas Canvas;
        public GameObject Prefab;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<HealthView>()
            );
            RequireForUpdate(m_Query);
        }

        struct PositionUIJob : IJobEntityBatch
        {
            [ReadOnly] public ComponentTypeHandle<Health> InputHealth;
            [ReadOnly] public ComponentTypeHandle<Translation> InputTranslation;
            [ReadOnly] public ComponentTypeHandle<HealthView> InputHealthView;

            [ReadOnly]
            public float Delta;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var healths = batchInChunk.GetNativeArray(InputHealth);
                var positions = batchInChunk.GetNativeArray(InputTranslation);
                var views = batchInChunk.GetNativeArray(InputHealthView);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var pos = positions[i].Value;
                    views[i].Value.SetPosition(pos);
                    views[i].Value.SetValue(healths[i].Property.Normalize);
                }
            }
        }


        protected override void OnUpdate()
        {

            var job = new PositionUIJob()
            {
                Delta = Time.DeltaTime,
                InputHealth = GetComponentTypeHandle<Health>(true),
                InputTranslation = GetComponentTypeHandle<Translation>(true),
                InputHealthView = GetComponentTypeHandle<HealthView>(),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            Dependency.Complete();

        }
    }
}