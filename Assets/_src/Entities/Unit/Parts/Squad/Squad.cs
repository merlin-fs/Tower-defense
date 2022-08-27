using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial class Squad
    {
        public struct Data : IComponentData
        {
            public int2 Radius;
            public Entity Leader;
        }
    }
}
