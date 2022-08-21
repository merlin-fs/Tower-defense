using System;

namespace Game.Model.World
{
    public partial class Map
    {
        public struct Bitmask
        {
            public int Value;
            public static implicit operator Bitmask(int value) => new Bitmask { Value = value };
        }
    }
}
