using System;
using Unity.Entities;

namespace Common.Defs
{
    public interface IDef
    {
        void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem);
        void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
        void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
    }

    public interface IDef<T>: IDef where T : IDefineable
    {
    }
}