using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Game.Model.Units.Turrets
{
    [UpdateInGroup(typeof(GameTransformSystemGroup))]
    public partial class TurretSystem : SystemBase
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadWrite<Turret>(),
                ComponentType.ReadOnly<Target>()
            );
            RequireForUpdate(m_Query);
        }

        struct TurretIdleJob : IJobEntityBatch
        {
            [ReadOnly]
            public float Delta;
            [ReadOnly]
            public float Rnd;
            public ComponentTypeHandle<Turret> InputTurret;
            [ReadOnly]
            public ComponentTypeHandle<Target> InputTarget;

            public ComponentDataFromEntity<Rotation> InputRotation;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var targets = batchInChunk.GetNativeArray(InputTarget);
                var turrets = batchInChunk.GetNativeArray(InputTurret);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Turret turret = turrets[i];
                    try
                    {
                        if (targets[i].Value != Entity.Null)
                            continue;
                        var rotation = InputRotation[turret.Entity];

                        float speed = turret.Def.Link.IdleRotationSpeed * 1f;
                        if (turret.WaitTimer > 0)
                        {
                            turret.WaitTimer -= Delta;
                            if (turret.WaitTimer <= 0)
                                RandomSpeed(Rnd, speed);

                            turret.Time = Mathf.InverseLerp(turret.Def.Link.RotationRange.x, turret.Def.Link.RotationRange.y, math.degrees(((Quaternion)rotation.Value).eulerAngles.y));
                            turret.RndTime = 1f;
                        }
                        else
                        {
                            if (turret.Time >= turret.RndTime)
                            {
                                turret.Direct = false;
                                RandomSpeed(Rnd, speed);
                            }
                            else if (turret.Time <= (1 - turret.RndTime))
                            {
                                turret.Direct = true;
                                RandomSpeed(Rnd, speed);
                            }
                            else if (turret.WaitRndTimer <= 0)
                            {
                                var dif = Mathf.Abs(turret.Time) - 0.5f;
                                if (Mathf.Abs(dif) < 0.005f)
                                {
                                    turret.RndTime = Mathf.Lerp(0.6f, 1f, Rnd);
                                    turret.WaitRndTimer = 0.005f;
                                }
                            }
                            else if (turret.WaitRndTimer > 0)
                            {
                                turret.WaitRndTimer -= Delta;
                            }

                            var value = Mathf.Lerp(turret.Def.Link.RotationRange.x, turret.Def.Link.RotationRange.y, turret.Time);

                            value = Mathf.Lerp(value, 0, turret.RotationCorrectionTime);
                            turret.RotationCorrectionTime = Mathf.Clamp01((turret.RotationCorrectionTime + Delta) / turret.Def.Link.IdleCorrectionTime);

                            var delta = turret.CurrentRotationSpeed * Delta * 0.01f;
                            if (turret.Direct)
                                turret.Time += delta;
                            else
                                turret.Time -= delta;

                            InputRotation[turret.Entity] = new Rotation() { Value = quaternion.RotateY(math.radians(value)) };
                        }
                    }
                    finally
                    {
                        turrets[i] = turret;
                    }

                    void RandomSpeed(float rnd, float speed)
                    {
                        turret.CurrentRotationSpeed = Mathf.Lerp(0.3f, 1.5f, rnd) * speed;
                    }
                }
            }
        }


        struct TurretTargetJob : IJobEntityBatch
        {
            [ReadOnly]
            public float Delta;
            public ComponentTypeHandle<Turret> InputTurret;
            [ReadOnly]
            public ComponentTypeHandle<Target> InputTarget;
            public ComponentDataFromEntity<Rotation> InputRotation;

            [ReadOnly]
            public ComponentDataFromEntity<Translation> InputTranslation;
            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> InputLocalToWorld;
            [ReadOnly]
            public ComponentDataFromEntity<LocalToParent> InputLocalToParent;
            public ComponentTypeHandle<WeaponReady> InputStateReady;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var targets = batchInChunk.GetNativeArray(InputTarget);
                var turrets = batchInChunk.GetNativeArray(InputTurret);
                var states = batchInChunk.GetNativeArray(InputStateReady);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Turret turret = turrets[i];
                    try
                    {
                        if (targets[i].Value == Entity.Null)
                            continue;
                        float3 target = InputTranslation[targets[i].Value].Value;

                        var localToWorld = InputLocalToWorld[turret.Entity];
                        var localToParent = InputLocalToParent[turret.Entity];
                        var rotation = InputRotation[turret.Entity];

                        if (turret.WaitTimer <= 0)
                            turret.RotationCorrectionTime = 1f;
                        turret.WaitTimer = turret.Def.Link.IdleWaitTime;

                        var direction = math.normalize(target - localToWorld.Position);
                        if (math.any(math.isnan(direction)))
                            return;

                        var targetQua = quaternion.LookRotation(direction, math.up());

                        var wordQua = quaternion.LookRotation(localToWorld.Forward, localToWorld.Up);
                        var parentQua = quaternion.LookRotation(localToParent.Forward, localToParent.Up);
                        quaternion look = math.mul(wordQua, math.inverse(parentQua));

                        targetQua = math.mul(targetQua, math.inverse(look));
                        var time = turret.CurrentRotationSpeed * Delta;

                        bool dontWork = false;

                        if (turret.RotationCorrectionTime > 0)
                        {
                            dontWork = true;
                            turret.RotationCorrectionTime -= time;
                        }


                        look = math.nlerp(rotation.Value, targetQua, time);
                        unsafe
                        {
                            bool clamp = false;
                            look = look.ClampRotationY(turret.Def.Link.RotationRange.x, turret.Def.Link.RotationRange.y, &clamp);
                            if (clamp)
                                dontWork = true;
                        }

                        InputRotation[turret.Entity] = new Rotation() { Value = look };
                        states[i] = !dontWork;
                    }
                    finally
                    {
                        turrets[i] = turret;
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var jobIdle = new TurretIdleJob()
            {
                Delta = Time.DeltaTime,
                Rnd = UnityEngine.Random.value,

                InputTurret = GetComponentTypeHandle<Turret>(false),
                InputTarget = GetComponentTypeHandle<Target>(true),
                InputRotation = GetComponentDataFromEntity<Rotation>(false),

            };
            Dependency = jobIdle.Schedule(m_Query, Dependency);

            var job = new TurretTargetJob()
            {
                Delta = Time.DeltaTime,

                InputTurret = GetComponentTypeHandle<Turret>(false),
                InputRotation = GetComponentDataFromEntity<Rotation>(false),
                InputStateReady = GetComponentTypeHandle<WeaponReady>(false),

                InputTarget = GetComponentTypeHandle<Target>(true),
                InputTranslation = GetComponentDataFromEntity<Translation>(true),
                InputLocalToWorld = GetComponentDataFromEntity<LocalToWorld>(true),
                InputLocalToParent = GetComponentDataFromEntity<LocalToParent>(true),
            };
            //Dependency = job.ScheduleParallel(m_Query, Dependency);
            Dependency = job.Schedule(m_Query, Dependency);
        }
    }
}