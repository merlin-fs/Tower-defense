using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Skills
{
    using World;

    public partial class Move
    {
        [Defineable(typeof(Moving))]
        public class MovingDef : BaseSkillDef
        {
            public int2 InitPosition = 0;
            public float Speed = 80;

            protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
            {
                base.AddComponentData(entity, manager, conversionSystem);
                manager.AddComponent<Commands>(entity); 

                manager.AddComponent<Map.Path.Info>(entity);
                manager.AddBuffer<Map.Path.Points>(entity);
                manager.AddBuffer<Map.Path.Times>(entity);
            }

            protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
            {
                base.AddComponentData(entity, writer, sortKey);
                writer.AddComponent<Commands>(sortKey, entity);
                writer.AddComponent<Map.Path.Info>(sortKey, entity);
                writer.AddBuffer<Map.Path.Points>(sortKey, entity);
                writer.AddBuffer<Map.Path.Times>(sortKey, entity);
            }
        }
    }
}