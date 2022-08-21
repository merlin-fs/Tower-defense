using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Units.Logics
{
    using Core;
    using Skills;
    using World;

    public unsafe class InitPlaceJob : ILogicPart
    {
        public float Weight => 1;

        [ReadOnly]
        private ComponentTypeHandle<Move.Moving> m_MoveHandle;

        public void Init(LogicSystem system)
        {
            m_MoveHandle = system.GetComponentTypeHandle<Move.Moving>(true);
        }

        public void Execute(ExecuteContext context)
        {
            context.Writer.RemoveComponent<Disabled>(context.SortKey, context.Entity);
            var moving = context.GetData<Move.Moving>(m_MoveHandle);
            
            var pos = moving.Def.Link.InitPosition;
            Map.GeneratePosition(Map.Singleton, ref pos);
            Move.Place(context.Entity, pos, context.Callback, context.SortKey);
        }
    }
}

