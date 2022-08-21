using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Mathematics;

namespace Game.Model.Units.Skills
{
    /*
    [UpdateInGroup(typeof(GameLogicInitSystemGroup))]
    public partial class CannonInitSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();

            m_Query = GetEntityQuery(
                ComponentType.ReadWrite<Cannon>(),
                ComponentType.ReadOnly<Teams>(),
                ComponentType.ReadOnly<Translation>()
            );
        }

        struct CannonInitJob : IJobEntityBatch
        {
            [ReadOnly] public float Delta;
            [ReadOnly] public ComponentTypeHandle<Translation> InputTranslation;
            [ReadOnly] public ComponentTypeHandle<Teams> InputTeams;
            [ReadOnly] public EntityTypeHandle InputEntity;

            public ComponentTypeHandle<Cannon> InputWeapon;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var weapons = batchInChunk.GetNativeArray(InputWeapon);
                var translation = batchInChunk.GetNativeArray(InputTranslation);
                var teams = batchInChunk.GetNativeArray(InputTeams);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var iter = weapons[i];
                    var entity = entities[i];
                    try
                    {
                        iter.TimeFind += Delta;
                        if (iter.Def.Link.FindDelay > iter.TimeFind)
                            continue;

                        iter.TimeFind = 0;
                        FindTarget.FindEnemy(entity, teams[i].EnemyTeams, translation[i].Value, 5f, default, batchIndex);
                    }
                    finally
                    {
                        weapons[i] = iter;
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new CannonInitJob
            {
                InputEntity = GetEntityTypeHandle(),
                InputWeapon = GetComponentTypeHandle<Cannon>(),
                InputTeams = GetComponentTypeHandle<Teams>(true),
                InputTranslation = GetComponentTypeHandle<Translation>(true),
                Delta = Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            //m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial class CannonShotSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            m_Query = GetEntityQuery(
                ComponentType.ReadWrite<Cannon>(),
                ComponentType.ReadOnly<FindTarget.Target>(),
                ComponentType.ReadOnly<WeaponReady>()
            );
            RequireForUpdate(m_Query);
        }

        struct CannonShotJob : IJobEntityBatch
        {
            [ReadOnly] public float Delta;
            [ReadOnly] public EntityTypeHandle InputEntity;
            [ReadOnly] public ComponentTypeHandle<FindTarget.Target> InputTarget;
            public EntityCommandBuffer.ParallelWriter Writer;
            public ComponentTypeHandle<Cannon> InputWeapon;
            public ComponentTypeHandle<WeaponReady> InputReady;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var weapons = batchInChunk.GetNativeArray(InputWeapon);
                var targets = batchInChunk.GetNativeArray(InputTarget);
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var weaponReadies = batchInChunk.GetNativeArray(InputReady);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    if (!weaponReadies[i].IsReady || targets[i].Value == Entity.Null)
                        continue;

                    Cannon iter = weapons[i];
                    try
                    {
                        iter.TimeShot += Delta;
                        if (iter.TimeShot >= iter.Def.Link.Frequency)
                        {
                            iter.TimeShot = 0;
                            Writer.AddComponent<StateShot>(batchIndex, entities[i]);

                            //add to Target components
                            foreach (var damage in iter.Def.Link.Damages)
                                damage.AddComponentData(targets[i].Value, Writer, batchIndex);

                            Writer.AddComponent(batchIndex, targets[i].Value, new StateShotDone { Delay = 0.01f });
                        };
                    }
                    finally
                    {
                        weapons[i] = iter;
                    }
                }
            }
        }


        protected override void OnUpdate()
        {
            var job = new CannonShotJob()
            {
                InputEntity = GetEntityTypeHandle(),
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                InputWeapon = GetComponentTypeHandle<Cannon>(),
                InputTarget = GetComponentTypeHandle<FindTarget.Target>(true),
                InputReady = GetComponentTypeHandle<WeaponReady>(),

                Delta = Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }

    /*
    [UpdateInGroup(typeof(GameWeaponInitSystemGroup))]
    public partial class CannonSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_QueryTarget;
        private EntityQuery m_QueryShot;
        private ComponentSystemGroup m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetExistingSystem<GameWeaponInitSystemGroup>();

            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateFindTarget>()
            );

            m_QueryShot = GetEntityQuery(
                ComponentType.ReadWrite<Cannon>(),
                ComponentType.ReadOnly<StateTarget>(),
                ComponentType.Exclude<StateWeaponDontReady>()
            );

            m_QueryTarget = GetEntityQuery(
                ComponentType.ReadOnly<Unit>(),
                ComponentType.ReadWrite<StateTeam>(),
                ComponentType.ReadWrite<Translation>()
            );

            RequireForUpdate(m_Query);
        }


        struct FindTargetJob : IJobEntityBatch
        {
            [NativeDisableContainerSafetyRestriction]
            public EntityManager Manager;

            [ReadOnly]
            public EntityTypeHandle InputEntity;

            public EntityCommandBuffer.ParallelWriter Writer;
            public ComponentTypeHandle<Cannon> InputWeapon;
            public ComponentTypeHandle<Translation> InputTranslation;
            public ComponentTypeHandle<StateTarget> InputTarget;

            public SharedComponentTypeHandle<StateEnemy> InputEnemy;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Entity> Enemy1;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Entity> Enemy2;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Entity> Player;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Translation> EnemyT1;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Translation> EnemyT2;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Translation> PlayerT;

            [ReadOnly]
            public float Delta;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var weapons = batchInChunk.GetNativeArray(InputWeapon);
                var positions = batchInChunk.GetNativeArray(InputTranslation);
                var enemies = batchInChunk.GetSharedComponentData(InputEnemy, Manager);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Cannon iter = weapons[i];
                    try
                    {
                        iter.TimeFind += Delta;
                        //&& batchInChunk.Has(InputTarget)
                        //TODO: ����� �������� ��������, ���� �� ������. ���� ���, �� �������� ����� � �������� �����.
                        if (iter.TimeFind < iter.Def.FindDelay)
                            continue;

                        iter.TimeFind = 0;
                        var write = Writer;
                        var selfPos = positions[i].Value;
                        var selfRadius = iter.Def.Distance / 2f;
                        var targetRadius = 5f;

                        FindTarget find = new FindTarget { Entity = Entity.Null, Magnitude = float.MaxValue };
                        NativeArray<Entity> listEntity = default;
                        NativeArray<Translation> listPosition = default;

                        foreach (var targets in enemies.Enemies)
                        {
                            switch (targets)
                            {
                                case "Enemy":
                                    listEntity = Enemy1;
                                    listPosition = EnemyT1;
                                    break;
                                case "Enemy 1":
                                    listEntity = Enemy2;
                                    listPosition = EnemyT2;
                                    break;
                                default:
                                    listEntity = Player;
                                    listPosition = PlayerT;
                                    break;
                            }

                            var CounterLock = new object();

                            Parallel.For(0, listEntity.Length, 
                                j =>
                                {
                                    var target = listEntity[j];
                                    var targetPos = listPosition[j].Value;

                                    var magnitude = (selfPos - targetPos).magnitude();
                                    //�������� ����������� ���� ����
                                    lock (CounterLock)
                                    {
                                        if (magnitude < find.Magnitude &&
                                            utils.SpheresIntersect(selfPos, selfRadius, targetPos, targetRadius, out float3 vector))
                                        {
                                            find.Magnitude = magnitude;
                                            find.Entity = target;
                                        }
                                    }

                                });
                        }

                        if (find.Entity == Entity.Null)
                        {
                            write.RemoveComponent<StateTarget>(batchIndex, entities[i]);
                        }
                        else
                        {
                            write.AddComponent(batchIndex, entities[i], new StateTarget() { Value = find.Entity });
                        }
                    }
                    finally
                    {
                        weapons[i] = iter;
                    }
                }
            }
            struct FindTarget
            {
                public Entity Entity;
                public float Magnitude;
            }
        }


        struct ShotJob : IJobEntityBatch
        {
            [ReadOnly]
            public EntityTypeHandle InputEntity;
            public EntityCommandBuffer.ParallelWriter Writer;
            public ComponentTypeHandle<Cannon> InputWeapon;
            public ComponentTypeHandle<StateTarget> InputTarget;

            [ReadOnly]
            public float Delta;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var weapons = batchInChunk.GetNativeArray(InputWeapon);
                var targets = batchInChunk.GetNativeArray(InputTarget);
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var write = Writer;
                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Cannon iter = weapons[i];
                    try
                    {
                        iter.TimeShot += Delta;
                        if (iter.TimeShot >= iter.Def.Frequency)
                        {
                            iter.TimeShot = 0;
                            write.AddComponent<StateShot>(batchIndex, entities[i]);

                            foreach (var damage in iter.Def.Damages)
                                damage.AddComponentData(targets[i].Value, write, batchIndex);
                            write.AddComponent(batchIndex, targets[i].Value, new StateShotDone { Delay = 0.01f });
                        };
                    }
                    finally
                    {
                        weapons[i] = iter;
                    }
                }
            }
        }

        [NotBurstCompatible]
        protected override void OnUpdate()
        {
            m_QueryTarget.SetSharedComponentFilter(new StateTeam { Name = "Enemy" });
            var Enemy1 = m_QueryTarget.ToEntityArray(Allocator.TempJob);
            var EnemyT1 = m_QueryTarget.ToComponentDataArray<Translation>(Allocator.TempJob);
            m_QueryTarget.SetSharedComponentFilter(new StateTeam { Name = "Enemy 1" });
            var Enemy2 = m_QueryTarget.ToEntityArray(Allocator.TempJob);
            var EnemyT2 = m_QueryTarget.ToComponentDataArray<Translation>(Allocator.TempJob);
            m_QueryTarget.SetSharedComponentFilter(new StateTeam { Name = "Player" });
            var Player = m_QueryTarget.ToEntityArray(Allocator.TempJob);
            var PlayerT = m_QueryTarget.ToComponentDataArray<Translation>(Allocator.TempJob);


            var findJob = new FindTargetJob()
            {
                Manager = EntityManager,
                InputEntity = GetEntityTypeHandle(),
                Writer = m_CommandBuffer.PostUpdateCommands.AsParallelWriter(),
                InputWeapon = GetComponentTypeHandle<Cannon>(),
                InputTranslation = GetComponentTypeHandle<Translation>(),
                InputTarget = GetComponentTypeHandle<StateTarget>(),

                InputEnemy = GetSharedComponentTypeHandle<StateEnemy>(),
                Enemy1 = Enemy1,
                Enemy2 = Enemy2,
                Player = Player,
                EnemyT1 = EnemyT1,
                EnemyT2 = EnemyT2,
                PlayerT = PlayerT,

                Delta = Time.DeltaTime,
            };
            Dependency = findJob.Schedule(m_Query, Dependency);

            var shotJob = new ShotJob
            {
                InputEntity = GetEntityTypeHandle(),
                Writer = m_CommandBuffer.PostUpdateCommands.AsParallelWriter(),
                InputWeapon = GetComponentTypeHandle<Cannon>(),
                InputTarget = GetComponentTypeHandle<StateTarget>(),

                Delta = Time.DeltaTime,
            };
            Dependency = shotJob.Schedule(m_QueryShot, Dependency);
        }
    }
    */
}
