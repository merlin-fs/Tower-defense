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
        public struct PathFinder
        {
            public delegate double GetCostTile(Entity entity, int2 source, int2 target);

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
                var previous = new NativeParallelHashMap<int2, int2>(capacity, Allocator.Temp);
                var costs = new NativeParallelHashMap<int2, Node.Cost>(capacity, Allocator.Temp)
                {
                    { 
                        source, 
                        new Node.Cost(source, target) { Value = 0.0 } 
                    }
                };

                var queue = new SortedNativeHashMap<Node.Cost, int2>(capacity, Allocator.Temp,
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
                var connections = new NativeArray<Node.Edge>(en.Length, Allocator.Temp);
                int2 limitTarget = target;

                while (queue.Pop(out (Node.Cost cost, int2 value) values))
                {
                    limitTarget = values.value;

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
                
                var path = ShortestPath(limitTarget);
                //var path = ShortestPath(target);
                connections.Dispose();
                previous.Dispose();
                costs.Dispose();

                return path;

                NativeArray <int2> ShortestPath(int2 v)
                {
                    var path = new NativeList<int2>(previous.Count(), Allocator.Temp);
                    while (!v.Equals(source))
                    {
                        if (!previous.TryGetValue(v, out int2 test))
                            return path.ToArray(Allocator.Temp);
                        else
                        {
                            path.Add(v);
                            v = test;
                        }
                    };
                    path.Add(source);
                    path.Reverse();
                    return path.ToArray(Allocator.Temp);
                }
            }

            private struct SortedNativeHashMap<TKey, TValue>
                where TKey : unmanaged, IEquatable<TKey>
                where TValue : struct
            {
                public delegate int Compare<T>(T x, T y) where T : struct;
                private readonly Compare<TKey> m_Comparer;
                private NativeList<TKey> m_Sorted;
                private NativeParallelHashMap<TKey, TValue> m_Values;

                private struct Value
                {
                    public TValue Data;
                    public int Index;
                }

                public SortedNativeHashMap(int length, Allocator allocator, Compare<TKey> comparer)
                {
                    m_Comparer = comparer;
                    m_Sorted = new NativeList<TKey>(length, allocator);
                    m_Values = new NativeParallelHashMap<TKey, TValue>(length, allocator);
                }

                int Insert(NativeList<TKey> values, TKey key)
                {
                    values.Length++;
                    bool found = false;
                    int i;
                    values[values.Length - 1] = key;
                    for (i = values.Length - 1; i >= 0; i--)
                    {
                        var cmp = m_Comparer.Invoke(key, values[i]);
                        if (cmp < 0)
                        {
                            found = true;
                            values[i + 1] = values[i];
                        }
                        else if (found)
                        {
                            values[i + 1] = key;
                            break;
                        }
                    }
                    if (found && i == -1)
                        values[i + 1] = key;
                    return i + 1;
                }

                void Delete(NativeList<TKey> values, int index)
                {
                    int i;
                    for (i = index; i < values.Length - 1; i++)
                        values[i] = values[i + 1];
                    values.Length--;
                }

                public bool Pop(out (TKey, TValue) values)
                {
                    if (m_Values.Count() <= 0)
                    {
                        values = (default(TKey), default(TValue));
                        return false;
                    }

                    TKey key = m_Sorted[0];
                    TValue value = m_Values[key];
                    Delete(m_Sorted, 0);
                    m_Values.Remove(key);
                    values = (key, value);
                    return true;
                }

                public void Push(TKey key, TValue value)
                {
                    if (m_Values.ContainsKey(key))
                        Delete(m_Sorted, m_Sorted.IndexOf(key));

                    Insert(m_Sorted, key);
                    m_Values[key] = value;
                }

                public int Count { get => m_Values.Count(); }

                public void Dispose()
                {
                    m_Sorted.Dispose();
                    m_Values.Dispose();
                }
            }

            private struct Node
            {
                public struct Cost: IEquatable<Cost>
                {
                    private readonly int m_Hash;
                    public double Distance { get; }
                    //public double StaticCost { get; }
                    public double? Value { get; set; }
                    public Cost(int2 source, int2 target)
                    {
                        var diff = (target - source);
                        m_Hash = source.GetHashCode();
                        Distance = math.distance(target, source);
                        //Distance = math.abs(diff).magnitude();
                        Value = null;
                    }

                    public override int GetHashCode()
                    {
                        return m_Hash;
                    }
                    public bool Equals(Cost other)
                    {
                        return m_Hash == other.m_Hash;
                    }
                }

                public struct Edge
                {
                    private double? m_Cost;
                    public int2 Source { get; }
                    public int2 Target { get; }
                    public double Cost(ref PathFinder finder)
                    {
                        if (!m_Cost.HasValue)
                            m_Cost = finder.m_GetCostTile(finder.Entity, Source, Target);
                        return m_Cost.Value;
                    }
                    public Edge(ref int2 source, ref int2 target)
                    {
                        Source = source;
                        Target = target;
                        m_Cost = null;
                    }
                }
            }
        }

    }
}