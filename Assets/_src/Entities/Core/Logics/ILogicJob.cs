using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

namespace Game.Core
{
    using Game.Model.Units.Skills;

    public unsafe interface ILogicPart
    {
        void Init(LogicSystem system);
        void Execute(ExecuteContext context);
        float Weight { get; }
    }

    public struct ExecuteContext
    {
        public Entity Entity { get; }
        public EntityCommandBuffer.ParallelWriter Writer { get; }
        public int SortKey { get; }
        public FunctionPointer<StateCallback> Callback { get; }

        private int m_Index;
        private ArchetypeChunk m_BatchInChunk;


        public T GetData<T>(ComponentTypeHandle<T> handle)
            where T : struct, IComponentData
        {
            return m_BatchInChunk.GetNativeArray(handle)[m_Index];
        }

        public ExecuteContext(EntityCommandBuffer.ParallelWriter writer, ArchetypeChunk batchInChunk, Entity entity, int index, FunctionPointer<StateCallback> callback, int sortKey)
        {
            Writer = writer;
            Entity = entity;
            SortKey = sortKey;
            m_Index = index;
            m_BatchInChunk = batchInChunk;
            Callback = callback;
        }
    }

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
            Move.Place(context.Entity, moving.Def.Link.InitPosition, context.Callback, context.SortKey);
        }
    }
} 