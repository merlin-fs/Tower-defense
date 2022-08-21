using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Model.Units.Skills
{
    using World;

    /*
    [UpdateInGroup(typeof(GameTransformSystemGroup))]
    public partial class RotateToTargetSystem : SystemBase
    {
        private EntityQuery m_Query;
        private static float3 UP = new float3(0f, 1f, 0f);

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<SetPositionOnMap>(),
                ComponentType.ReadOnly<FindTarget.Target>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadWrite<Translation>()
            );

            RequireForUpdate(m_Query);
        }


        struct LookToTarhetJob : IJobEntityBatch
        {
            [ReadOnly] public float Delta;
            [ReadOnly] public ComponentDataFromEntity<Translation> InputTranslation;
            [ReadOnly] public EntityTypeHandle InputEntity;
            [ReadOnly] public ComponentTypeHandle<SetPositionOnMap> InputPosition;
            [ReadOnly] public ComponentTypeHandle<FindTarget.Target> InputTarget;

            public ComponentTypeHandle<Rotation> InputRotation;
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var rotations = batchInChunk.GetNativeArray(InputRotation);
                var pathInfos = batchInChunk.GetNativeArray(InputPosition);
                var targets = batchInChunk.GetNativeArray(InputTarget);
                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var path = pathInfos[i];
                    if (path.PathPrecent >= 1f && targets[i].Value != Entity.Null)
                    {
                        var rotation = rotations[i];
                        try
                        {
                            var positionTarget = InputTranslation[targets[i].Value].Value;
                            var positionSelf = InputTranslation[entities[i]].Value;
                            float time = path.Def.Link.Speed * 0.1f * Delta;

                            quaternion rotationTarget = quaternion.LookRotation(math.normalize(positionTarget - positionSelf), UP);
                            rotation.Value = math.nlerp(rotation.Value, rotationTarget, time);
                        }
                        finally
                        {
                            rotations[i] = rotation;
                        }
                    }
                }
            }
        }


        protected override void OnUpdate()
        {
            var job = new LookToTarhetJob()
            {
                Delta = Time.DeltaTime,
                InputPosition = GetComponentTypeHandle<SetPositionOnMap>(true),
                InputEntity = GetEntityTypeHandle(),
                InputTarget = GetComponentTypeHandle<FindTarget.Target>(true),
                InputTranslation = GetComponentDataFromEntity<Translation>(true),
                InputRotation = GetComponentTypeHandle<Rotation>(false),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }
    */
}