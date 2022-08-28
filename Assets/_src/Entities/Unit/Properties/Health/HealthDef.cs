using System;
using Unity.Entities;
using Common.Entities.Tools;
using Common.Core;
using UnityEngine;

namespace Game.Model.Properties
{
    public class HealthDef : BasePropertyDef<Health>
    {
        [SerializeField]
        HealthComponent m_Prefab;

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            //manager.AddComponent<HealthView>(entity);
        }

        public static void Initialize(Canvas canvasParent, HealthComponent prefab)
        {
            var system = Bootstrap.AddSystemToGroup<HealthAddSystem, GameSpawnSystemGroup>();
            system.CanvasParent = canvasParent;
            system.Prefab = prefab;

            Bootstrap.AddSystemToGroup<HealthDelSystem, GameDoneSystemGroup>();
        }

        protected override void InitializeDataConvert(ref Health value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.InitializeDataConvert(ref value, entity, manager, conversionSystem);
            value.Value = value.Def.Link.Value;
        }
    }
}