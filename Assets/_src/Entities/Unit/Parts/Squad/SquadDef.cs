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

            [SerializeReference, Reference()]
            Move.MovingDef m_Move;

            protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
            {
                base.AddComponentData(entity, manager, conversionSystem);
                (m_Move as IDef).AddComponentData(entity, manager, conversionSystem);
            }

            protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
            {
                base.AddComponentData(entity, writer, sortKey);
                (m_Move as IDef).AddComponentData(entity, writer, sortKey);
            }
        }
    }
}
