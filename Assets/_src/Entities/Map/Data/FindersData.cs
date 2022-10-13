using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.World
{
    public partial class Map
    {
        public delegate double GetCostTile(Entity entity, int2 source, int2 target);
        public struct SortedNativeHashMap<TKey, TValue>
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
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

            public bool Has(TKey value)
            {
                return m_Values.ContainsKey(value);
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

        public struct Node
        {
            public struct Cost : IEquatable<Cost>
            {
                private readonly int m_Hash;
                public double Distance { get; }
                public double? Value { get; set; }

                public float heuristicStartToEndLen; // which passes current node
                public float startToCurNodeLen;
                public float? heuristicCurNodeToEndLen;

                public Cost(int2 source, int2 target)
                {
                    var diff = (target - source);
                    m_Hash = source.GetHashCode();
                    Distance = math.distance(target, source);
                    //Distance = math.abs(diff).magnitude();
                    Value = null;
                    heuristicStartToEndLen = 0;
                    startToCurNodeLen = 0;
                    heuristicCurNodeToEndLen = null;
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
