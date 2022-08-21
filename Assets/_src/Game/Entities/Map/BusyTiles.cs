using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

namespace Game.Model.World
{
    public partial class Map
    {
        public static ConcurrentDictionary<int2, Entity> Entities;

        public partial class TilesData
        {
            public unsafe bool EntityExist(int2 position, Entity* entity = null)
            {
                var result = Entities.TryGetValue(position, out Entity value);
                if (entity != null)
                    *entity = value;
                return result;
            }

            public void AddEntity(int2 position, Entity entity)
            {
                Entities[position] = entity;
            }

            public void DelEntity(int2 position)
            {
                Entities.TryRemove(position, out Entity entity);
            }

            public void InitBusyTiles(int2 size)
            {
                Entities = new ConcurrentDictionary<int2, Entity>(1, size.x * size.y);
            }
        }
    }
}
