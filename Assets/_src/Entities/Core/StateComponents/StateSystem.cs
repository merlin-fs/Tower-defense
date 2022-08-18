using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Game.Core
{
    public abstract partial class StateSystem<T> : CallbackSystem
        where T : struct, ICallbackComponent
    {
        protected EntityCommandBufferSystem m_CommandBuffer;

        public EntityCommandBuffer.ParallelWriter Writer => m_CommandBuffer.CreateCommandBuffer().AsParallelWriter();

        public void SendData(Entity entity, T value, int sortKey)
        {
            m_CommandBuffer.CreateCommandBuffer().AsParallelWriter().SetComponent(sortKey, entity, value);
        }
    }
}