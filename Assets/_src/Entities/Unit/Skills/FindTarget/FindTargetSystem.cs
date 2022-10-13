using System;
using Common.Core;
using Common.Defs;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

namespace Game.Model.Skills
{
    using Core;

    using Game.Model.World;

    public partial class FindTarget
    {
        struct TempFindTarget
        {
            public Entity Entity;
            public float Magnitude;
        }

        public static Entity FindEnemy(uint findTeams, float3 selfPosition, float selfRadius,
            ComponentLookup<LocalToWorldTransform> translations, ComponentLookup<Teams> teams)
        {
            TempFindTarget find = new TempFindTarget { Entity = Entity.Null, Magnitude = float.MaxValue };
            var CounterLock = new object();
            var entities = Map.Entities.Values;
            Parallel.ForEach(entities, (target) => 
            {
                var team = teams[target];
                //�������� ������ ��������
                if ((team.Team & findTeams) == 0)
                    return;

                var targetPos = translations[target].Value;
                var magnitude = (selfPosition - targetPos.Position).magnitude();
                //�������� ����������� ���� ����
                if (magnitude < find.Magnitude &&
                    utils.SpheresIntersect(selfPosition, selfRadius, targetPos.Position, 5f, out float3 vector))
                {
                    lock (CounterLock)
                    {
                        find.Magnitude = magnitude;
                        find.Entity = target;
                    }
                }
            });
            return find.Entity;
        }
    }
}