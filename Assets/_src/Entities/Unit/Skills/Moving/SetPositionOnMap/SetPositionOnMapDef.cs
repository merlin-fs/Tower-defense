using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Units
{
    using Defs;
    using World;

    [Defineable(typeof(SetPositionOnMap))]
    public class SetPositionOnMapDef : BaseSkillDef
    {
        public int2 InitPosition = 0;
        public int2 TargetPosition = 0;
        public float Speed = 80;

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddBuffer<Map.Path.Points>(entity);
            manager.AddBuffer<Map.Path.Times>(entity);
        }
    }
}