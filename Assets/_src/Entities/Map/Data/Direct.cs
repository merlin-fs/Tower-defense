using System;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial class Map
    {
        public enum Direct
        {
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }
    }

    public static class DirectExtension
    {
        public static int2 Position(this Map.Direct self, int x, int y) => self.Position(new int2(x, y));

        public static int2 Position(this Map.Direct self, int2 posistion)
        {
            return self switch
            {
                Map.Direct.Left => posistion + new int2(-1, 0),
                Map.Direct.Top => posistion + new int2(0, 1),
                Map.Direct.Right => posistion + new int2(1, 0),
                Map.Direct.Bottom => posistion + new int2(0, -1),
                Map.Direct.TopLeft => posistion + new int2(-1, 1),
                Map.Direct.TopRight => posistion + new int2(1, 1),
                Map.Direct.BottomRight => posistion + new int2(1, -1),
                Map.Direct.BottomLeft => posistion + new int2(-1, -1),
                _ => posistion,
            };
        }

        public static int Bit(this Map.Direct self)
        {
            return self switch
            {
                Map.Direct.TopLeft => 1,
                Map.Direct.Top => 2,
                Map.Direct.TopRight => 4,
                Map.Direct.Right => 16,
                Map.Direct.BottomRight => 128,
                Map.Direct.Bottom => 64,
                Map.Direct.BottomLeft => 32,
                Map.Direct.Left => 8,
                _ => 0,
            };
        }
    }
}
