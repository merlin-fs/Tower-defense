using System;
using Unity.Mathematics;
using Unity.Entities;

namespace Game.Model.World
{
    public partial class Map
    {
        public partial struct Data
        {

            public unsafe double GetCostTile(Entity entity, int2 source, int2 target)
            {
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
