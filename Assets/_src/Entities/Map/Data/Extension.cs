using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Threading.Tasks;

namespace Game.Model.World
{
    public static class Extension
    {
        public static int2 Int2Null = new int2(-1);
        public static int2 Null(this int2 _)
        {
            return Int2Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this int2 self)
        {
            return (self.x == Int2Null.x && self.y == Int2Null.y);
        }
    }

    public partial class Map
    {

        public partial struct Data
        {
            public delegate void EnumTile(int x, int y);

            public int2 GetTile(int x, int y, int idx) => GetTile(x, y, (Direct)idx);
            public int2 GetTile(int x, int y, Direct direct)
            {
                var pos = direct.Position(x, y);
                return (pos.x >= 0 && pos.x < Size.x &&
                        pos.y >= 0 && pos.y < Size.y)
                    ? pos
                    : Extension.Int2Null;
            }

            public int2 GetTile(int2 pos, Direct direct) => GetTile(pos.x, pos.y, direct);

            public bool Passable(int2 pos) => Passable(pos.x, pos.y);
            public bool Passable(int x, int y)
            {
                return (x >= 0 && x < Size.x &&
                        y >= 0 && y < Size.y);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int At(int x, int y) => (y * Size.x) + x;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int At(int2 pos) => At(pos.x, pos.y);

            public float StaticHeight(HeightType.Type heightType)
            {
                return heightType switch
                {
                    Map.HeightType.Type.DeepWater => 0.02f,
                    Map.HeightType.Type.ShallowWater => 0.06f,
                    Map.HeightType.Type.Shore => 0,
                    Map.HeightType.Type.Sand => 0.15f,
                    Map.HeightType.Type.Grass => 0.19f,
                    Map.HeightType.Type.Forest => 0.38f,
                    Map.HeightType.Type.Rock => 0.79f,
                    Map.HeightType.Type.Snow => 0.8f,
                    _ => 0,
                };
            }


            public NativeList<int2> GetNeighbors(int2 source)
            {
                var en = Enum.GetValues(typeof(Map.Direct));
                var list = new NativeList<int2>(en.Length, Allocator.Temp);

                foreach (Map.Direct direct in en)
                {
                    var neighbor = GetTile(source.x, source.y, direct);
                    if (!neighbor.IsNull())
                    {
                        list.Add(neighbor);
                    }
                }
                return list;
            }

            public void ParallelForeachTiles(EnumTile action)
            {
                int localY = Size.y;
                Parallel.For(0, Size.x,
                    (x) =>
                    {
                        Parallel.For(0, localY,
                            (y) =>
                            {
                                action(x, y);
                            }
                        );
                    });
            }
        }
    }
}