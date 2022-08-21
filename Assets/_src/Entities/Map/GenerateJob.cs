using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Entities;
using AccidentalNoise;


namespace Game.Model.World
{
    public partial class Map
    {
        public delegate float GetHeight(float x, float y);
           
        //[BurstCompile]
        struct InitJob : IJob
        {
            public GenerateMapTag Config;
            public EntityCommandBuffer.ParallelWriter Writer;

            public Map.Data Map;

            public void Execute()
            {
                //var tt = new System.Diagnostics.Stopwatch();
                //tt.Start();

                //new AccidentalNoise.ImplicitBlend()
                var heightMap = new ImplicitFractal(
                    FractalType.FRACTIONALBROWNIANMOTION, //FractalType.RIDGEDMULTI, //FractalType.FRACTIONALBROWNIANMOTION,//FractalType.BILLOW,//FractalType.MULTI,
                    BasisType.SIMPLEX,
                    InterpolationType.QUINTIC,
                    Config.TerrainOctaves,           //Settings.TerrainOctaves,
                    Config.TerrainFrequency,         //Settings.TerrainFrequency,
                    Config.Seed);                    //seed 12023

                float max = float.MinValue;
                float min = float.MaxValue;

                InitHeights(Map.Tiles.WriteHeights,
                    (x, y) => (float)heightMap.Get(x, y),
                    Map, ref min, ref max);

                InitHeightTypes(Map.Tiles.WriteHeightTypes, Map.Tiles.WriteHeights, Map, min, max);
                InitBitmask(Map.Tiles.WriteBitmasks, Map.Tiles.WriteHeightTypes, Map);

                var entity = Writer.CreateEntity(0);
                Writer.AddComponent<Map.Data>(0, entity, Map);
                //tt.Stop();
                //Debug.Log("Fill: " + tt.Elapsed);
            }
        }

        internal static void InitHeights(IList<Height> tiles, GetHeight getHeight, Map.Data map, ref float min, ref float max)
        {
            float localMin = min;
            float localMax = max;
            map.ParallelForeachTiles(
                (x, y) =>
                {
                    float x1 = (float)x / map.Size.x;
                    float y1 = (float)y / (float)map.Size.y;
                    float value = getHeight(x1, y1);
                    if (value < localMin) localMin = value;
                    if (value > localMax) localMax = value;

                    int idx = map.At(x, y);
                    tiles[idx] = value;
                }
            );
            min = localMin;
            max = localMax;
        }

        internal static void InitHeightTypes(IList<HeightType> tiles, IList<Height> height, Map.Data map, float min, float max)
        {
            map.ParallelForeachTiles(
                (x, y) =>
                {
                    int idx = map.At(x, y);
                    float value = height[idx].Value;
                    
                    value = (value - min) / (max - min);

                    height[idx] = value;

                    tiles[idx] = HeightType.Type.Snow;
                    foreach (HeightType.Type h in Enum.GetValues(typeof(HeightType.Type)))
                    {
                        if (value < map.StaticHeight(h))
                        {
                            tiles[idx] = h;
                            break;
                        }
                    }

                    //Установить фиксированную высоту
                    if (value > map.StaticHeight(HeightType.Type.Snow))
                        height[idx] = 1f;

                    var a = (HeightType.Type)math.clamp((int)tiles[idx].Value - 1, 0, (int)HeightType.Type.Snow);
                    var b = (HeightType.Type)math.clamp((int)tiles[idx].Value + 1, 0, (int)HeightType.Type.Snow);

                    height[idx] = math.clamp(value, map.StaticHeight(a), map.StaticHeight(b));
                    //height[idx] = data.StaticHeight(tiles[idx].Value);
                }
            );
        }

        internal static void InitBitmask(IList<Bitmask> tiles, IList<HeightType> height, Map.Data map)
        {
            map.ParallelForeachTiles(
                (x, y) =>
                {
                    int count = 0;
                    int idx = map.At(x, y);
                    HeightType.Type heightType = height[idx].Value;

                    foreach (Direct direct in Enum.GetValues(typeof(Direct)))
                    {
                        var neighbor = map.GetTile(x, y, direct);
                        if (!neighbor.IsNull() && height[map.At(neighbor)].Value == heightType)
                            count += direct.Bit();
                    }
                    tiles[idx] = count;
                }
            );
            
        }

        /*
        protected static void UpdateNeighbors(NativeArray<Tile.Neighbor> tiles, Data data)
        {
            var denum = Enum.GetValues(typeof(Tile.Neighbor.Direct));
            Extension.EnumTiles(data,
                (x, y) =>
                {
                    foreach (Tile.Neighbor.Direct direct in denum)
                    {
                        int idx = data.Neighbor(x, y, direct);
                        tiles[idx] = data.GetTile(x, y, direct);
                    }
                }
            );
        }
        */

    }
}