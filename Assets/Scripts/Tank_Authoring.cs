using System;
using Unity.Entities;


namespace Game.Model.Units
{
    public struct ReferencePrefab : IComponentData
    {
        public Entity Prefab;
    }
}