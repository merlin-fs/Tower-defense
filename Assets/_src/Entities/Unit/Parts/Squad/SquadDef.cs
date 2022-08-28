using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using UnityEngine;

namespace Game.Model
{
    using Logics;
    using Properties;
    using Skills;

    public partial class Squad
    {
        public interface ISquadDef : IDef
        {

        }


        [Defineable(typeof(Data))]
        public class SquadDef : ClassDef<Data>, ISquadDef
        {
            public float Radius = 3;

            [SerializeReference, Reference()]
            ILogicDef m_Logic;

            protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
            {
                base.AddComponentData(entity, manager, conversionSystem);
                //manager.AddBuffer<Map.Path.Times>(entity);
            }
        }
    }
}
