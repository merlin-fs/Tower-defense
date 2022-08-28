using System;
using Unity.Mathematics;
using Unity.Entities;

namespace Game.Model.World
{
    public partial class Map
    {
        public partial struct Data
        {

            public unsafe double GetCostTileRadius(int radius, Entity entity, int2 source, int2 target)
            {
                double result = 1f;
                var map = Singleton;
                using (var cells = Map.GetCells(target, radius,
                                    (value) =>
                                    {
                                        var weight = map.GetCostTile(source, value);
                                        if (weight < 0)
                                            result = -1f;
                                        return false;
                                    }, Unity.Collections.Allocator.TempJob))
                {
                };
                return result;
            }

            public unsafe double GetCostTile(int2 source, int2 target)
            {
                if (!Passable(target))
                    return -1f;

                var idx = At(target);
                var value = Tiles.Heights[idx].Value;
                switch (Tiles.HeightTypes[idx].Value)
                {
                    case HeightType.Type.Snow:
                    case HeightType.Type.DeepWater:
                    case HeightType.Type.ShallowWater:
                        return -1f;

                    case HeightType.Type.Shore:
                        value += 1f;
                        break;
                    case HeightType.Type.Sand:
                        value += 1f;
                        break;
                    case HeightType.Type.Forest:
                        value += 1f;
                        break;
                    case HeightType.Type.Rock:
                        value += 1f;
                        break;
                    default:
                        value += 1f;
                        break;
                }
                return value * 100;
            }

            public unsafe double GetCostTile(Entity entity, int2 source, int2 target)
            {
                if (!Passable(target))
                    return -1f;

                Entity entityTarget;
                if (Tiles.EntityExist(target, &entityTarget) && entityTarget != entity)
                    return -1f;

                var idx = At(target);
                var value = Tiles.Heights[idx].Value;
                switch (Tiles.HeightTypes[idx].Value)
                {
                    case HeightType.Type.Snow:
                    case HeightType.Type.DeepWater:
                    case HeightType.Type.ShallowWater:
                        return -1f;

                    case HeightType.Type.Shore:
                        value += 1f;
                        break;
                    case HeightType.Type.Sand:
                        value += 1f;
                        break;
                    case HeightType.Type.Forest:
                        value += 1f;
                        break;
                    case HeightType.Type.Rock:
                        value += 1f;
                        break;
                    default:
                        value += 1f;
                        break;
                }
                return value * 100;
            }
        }
    }
}
