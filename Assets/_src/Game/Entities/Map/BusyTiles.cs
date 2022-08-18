using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

namespace Game.Model.World
{
    public partial struct Map
    {
        public partial class TilesData
        {
            private ConcurrentDictionary<int2, Entity> m_Entities;

            public unsafe bool EntityExist(int2 position, Entity* entity = null)
            {
                var result = m_Entities.TryGetValue(position, out Entity value);
                if (entity != null)
                    *entity = value;
                return result;
            }

            public void AddEntity(int2 position, Entity entity)
            {
                m_Entities[position] = entity;
            }

            public void DelEntity(int2 position)
            {
                m_Entities.TryRemove(position, out Entity entity);
            }

            public void InitBusyTiles(int2 size)
            {
                m_Entities = new ConcurrentDictionary<int2, Entity>(1, size.x * size.y);
            }
        }
    }
}
