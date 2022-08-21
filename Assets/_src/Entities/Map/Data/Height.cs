using System;

namespace Game.Model.World
{
    public partial class Map
    {
        public struct Height
        {
            public float Value;
            public static implicit operator Height(float value) => new Height { Value = value };
        }
    }
}
