using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Logics
{
    using Core;
    using Skills;
    using Unity.Burst;
    using World;

    public struct InitPlaceJob : ILogicJob
    {
        public float Weight => 1;

        [ReadOnly]
        private ComponentTypeHandle<Move.Moving> m_MoveHandle;

        public InitPlaceJob(LogicSystem system)
        {
            m_MoveHandle = system.GetComponentTypeHandle<Move.Moving>(true);
        }

        public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
        {
            context.Writer.RemoveComponent<Disabled>(context.SortKey, context.Entity);
            var moving = context.GetData<Move.Moving>(m_MoveHandle);

            var pos = moving.Def.Link.InitPosition;
            Map.GeneratePosition(Map.Singleton, ref pos);
            Move.Place(context.Entity, pos, callback);
        }
    }
}

