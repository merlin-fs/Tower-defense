using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

namespace Game.Model.World
{
    public partial class Map
    {
        //TODO: чтото с поиском не так :( Нужно нати другой.
        public struct PathFinder
        {
            internal IReadOnlyList<HeightType> m_HeightsType;
            internal IReadOnlyList<Height> m_Heights;
            internal Map.Data m_Map;
            internal Entity Entity;

            internal GetCostTile m_GetCostTile;

            public static NativeArray<int2> Execute(GetCostTile getCostTile, Entity entity, int2 source, int2 target, Map.Data map,
                int? pathLimit = null)
            {
                var finder = new PathFinder()
                {
                    m_Map = map,
                    m_HeightsType = map.Tiles.HeightTypes,
                    m_Heights = map.Tiles.Heights,
                    m_GetCostTile = getCostTile,
                    Entity = entity,
                };

                var path = finder.Search(source, target, pathLimit);
                return path;
            }

            internal double GetCost(int2 source, int2 target)
            {
                var idx = m_Map.At(target);
                var value = m_Heights[idx].Value;
                switch (m_HeightsType[idx].Value)
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
                /*
                if (math.all(math.abs(target - source) == 1))
                    value -= 0.1f;
                */
                return value * 100;

            }

            private void GetNeighbors(int2 source, Map.Data map, Array en, NativeArray<Node.Edge> inList)
            {
                var list = inList;
                Parallel.For(0, en.Length,
                    (idx) =>
                    {
                        var neighbor = map.GetTile(source.x, source.y, idx);
                        list[idx] = !neighbor.IsNull()
                            ? new Node.Edge(ref source, ref neighbor)
                            : default;
                    });

                /*
                foreach (Tile.Direct direct in en)
                {
                    var neighbor = data.GetTile(source.x, source.y, direct);
                    if (!neighbor.IsNull())
                    {
                        list.Add(new Node.Edge(ref source, ref neighbor));
                    }
                }
                */
                var finder = this;
                //Array.Sort(list, Comparer<Node.Edge>.Create(
                list.Sort(Comparer<Node.Edge>.Create(
                    (Node.Edge i1, Node.Edge i2) =>
                    {
                        if (i1.Source.IsNull())
                            return -1;
                        if (i2.Source.IsNull())
                            return 1;

                        double res = i1.Cost(ref finder) - i2.Cost(ref finder);
                        return Math.Sign(res);
                    }));
            }

            private NativeArray<int2> Search(int2 source, int2 target, int? pathLimit)
            {
                if (!m_Map.Passable(target))
                    return new NativeArray<int2>();

                var capacity = pathLimit ?? 100;
                var previous = new NativeParallelHashMap<int2, int2>(capacity, Allocator.TempJob);
                var costs = new NativeParallelHashMap<int2, Node.Cost>(capacity, Allocator.TempJob)
                {
                    {
                        source,
                        new Node.Cost(source, target) { Value = 0.0 }
                    }
                };

                var queue = new SortedNativeHashMap<Node.Cost, int2>(capacity, Allocator.TempJob,
                    (Node.Cost i1, Node.Cost i2) =>
                    {
                        if (!i1.Value.HasValue)
                            return -1;
                        if (!i2.Value.HasValue)
                            return 1;

                        double res = i1.GetHashCode() - i2.GetHashCode();
                        if (res == 0)
                            return 0;

                        res = (i1.Value.Value + i1.Distance) - (i2.Value.Value + i2.Distance);
                        return Math.Sign(res);
                    }
                );

                queue.Push(costs[source], source);

                var en = Enum.GetValues(typeof(Direct));
                var connections = new NativeArray<Node.Edge>(en.Length, Allocator.TempJob);
                //int2 limitTarget = target;

                while (queue.Pop(out (Node.Cost cost, int2 value) values))
                {
                    //limitTarget = values.value;

                    if (values.value.Equals(target))
                        break;

                    if (pathLimit.HasValue)
                        pathLimit--;

                    GetNeighbors(values.value, m_Map, en, connections);

                    for (int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].Source.IsNull())
                            continue;

                        var iter = connections[i];
                        var directCost = iter.Cost(ref this);
                        if (directCost < 0)
                            continue;

                        if (!costs.TryGetValue(iter.Target, out Node.Cost cost))
                        {
                            cost = new Node.Cost(iter.Target, target);
                            costs.Add(iter.Target, cost);
                        }

                        if (cost.Value.HasValue || previous.ContainsKey(iter.Target))
                            continue;

                        if (!cost.Value.HasValue ||
                           (values.cost.Value + directCost < cost.Value))
                        {
                            cost.Value = values.cost.Value + directCost;
                            previous[iter.Target] = values.value;
                            queue.Push(cost, iter.Target);
                        }
                    }

                    if (pathLimit.HasValue && pathLimit.Value <= 0)
                        break;
                }

                //var path = ShortestPath(limitTarget);
                var path = ShortestPath(target);
                connections.Dispose();
                previous.Dispose();
                costs.Dispose();

                return path;

                NativeArray<int2> ShortestPath(int2 v)
                {
                    var path = new NativeList<int2>(previous.Count(), Allocator.TempJob);
                    while (!v.Equals(source))
                    {
                        if (!previous.TryGetValue(v, out int2 test))
                        {
                            path.Dispose();
                            return new NativeList<int2>(0, Allocator.TempJob);
                        }
                        else
                        {
                            path.Add(v);
                            v = test;
                        }
                    };
                    path.Add(source);
                    path.Reverse();
                    return path.ToArray(Allocator.TempJob);
                }
            }
        }
    }
}