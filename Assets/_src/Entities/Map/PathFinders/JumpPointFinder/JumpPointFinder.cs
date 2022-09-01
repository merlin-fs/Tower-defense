using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;
using System.Text;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.World
{

    public partial class Map
    {
        public delegate float HeuristicDelegate(int iDx, int iDy);

        public struct JumpPointFinder
        {
            public static NativeArray<int2> FindPath(GetCostTile getCostTile, Map.Data map, Entity entity, int2 source, int2 target, HeuristicMode iMode = HeuristicMode.EUCLIDEAN)
            {
                return new JumpPointFinder(getCostTile, source, target, iMode)
                {
                    m_Entity = entity,
                    m_Map = map,
                }
                .Search(source, target);
            }

            private static Array m_Directs = Enum.GetValues(typeof(Direct));
            private HeuristicDelegate m_Heuristic;
            private bool m_UseRecursive;
            private NativeParallelHashMap<int2, int2> m_Hierarchy;
            private GetCostTile m_GetCostTile;
            private Entity m_Entity;
            private Map.Data m_Map;

            private int2 m_EndNode, m_StartNode;
            private NativeParallelHashMap<int2, Node.Cost> m_Costs;
            private SortedNativeHashMap<Node.Cost, int2> m_Queue;
            private NativeParallelHashMap<int2, bool> m_WalkableCache;

            public JumpPointFinder(GetCostTile getCostTile, int2 source, int2 target, HeuristicMode iMode)
            {
                m_StartNode = source;
                m_EndNode = target;
                m_UseRecursive = false;
                m_Map = default;
                switch (iMode)
                {
                    case HeuristicMode.MANHATTAN:
                        m_Heuristic = new HeuristicDelegate(Heuristic.Manhattan);
                        break;
                    case HeuristicMode.EUCLIDEAN:
                        m_Heuristic = new HeuristicDelegate(Heuristic.Euclidean);
                        break;
                    case HeuristicMode.CHEBYSHEV:
                        m_Heuristic = new HeuristicDelegate(Heuristic.Chebyshev);
                        break;
                    default:
                        m_Heuristic = new HeuristicDelegate(Heuristic.Euclidean);
                        break;
                }
                m_Entity = Entity.Null;
                m_GetCostTile = getCostTile;
                m_Hierarchy = default;
                m_Costs = default;
                m_Queue = default;
                m_WalkableCache = default;
            }

            public NativeArray<int2> Search(int2 source, int2 target)
            {
                //var capacity = pathLimit ?? 100;
                bool revertEndNodeWalkable = false;
                m_Hierarchy = new NativeParallelHashMap<int2, int2>(100, Allocator.TempJob);
                m_WalkableCache = new NativeParallelHashMap<int2, bool>(100, Allocator.TempJob);
                m_Costs = new NativeParallelHashMap<int2, Node.Cost>(100, Allocator.TempJob)
                {
                    {
                        source,
                        new Node.Cost(source, target)
                        {
                            Value = 0.0
                        }
                    }
                };
                m_Queue = new SortedNativeHashMap<Node.Cost, int2>(100, Allocator.TempJob,
                    (Node.Cost i1, Node.Cost i2) =>
                    {
                        float result = i1.heuristicStartToEndLen - i2.heuristicStartToEndLen;
                        return Math.Sign(result);
                    }
                );

                // set the `g` and `f` value of the start node to be 0
                //tStartNode.startToCurNodeLen = 0;
                ///tStartNode.heuristicStartToEndLen = 0;

                // push the start node into the open list
                //tOpenList.Add(tStartNode);
                //tStartNode.isOpened = true;

                /*
                if (iParam.AllowEndNodeUnWalkable && !iParam.SearchGrid.IsWalkableAt(tEndNode.x, tEndNode.y))
                {
                    //iParam.SearchGrid.SetWalkableAt(tEndNode.x, tEndNode.y, true);
                    revertEndNodeWalkable = true;
                }
                */
                m_Queue.Push(m_Costs[source], source);
                try
                {
                    while (m_Queue.Pop(out (Node.Cost cost, int2 value) node))
                    {

                        if (node.value.Equals(target))
                        {
                            if (revertEndNodeWalkable)
                            {
                                //iParam.SearchGrid.SetWalkableAt(tEndNode.x, tEndNode.y, false);
                            }
                            return ShortestPath(node.value); // rebuilding path
                        }

                        IdentifySuccessors(node);
                    }
                    return new NativeArray<int2>(1, Allocator.TempJob);
                }
                finally
                {
                    m_WalkableCache.Dispose();
                    m_Hierarchy.Dispose();
                    m_Costs.Dispose();
                    m_Queue.Dispose();
                }
            }

            private NativeArray<int2> ShortestPath(int2 v)
            {
                var path = new NativeList<int2>(m_Hierarchy.Count(), Allocator.TempJob); 
                try
                {
                    while (!v.Equals(m_StartNode))
                    {
                        if (!m_Hierarchy.TryGetValue(v, out int2 test))
                        {
                            return new NativeArray<int2>(1, Allocator.TempJob);
                        }
                        else
                        {
                            path.Add(v);
                            v = test;
                        }

                        if (path.Length > m_Hierarchy.Count())
                        {
                            break;
                        }

                    };
                    path.Add(m_StartNode);
                    path.Reverse();
                    return path.ToArray(Allocator.TempJob);
                }
                finally 
                { 
                    path.Dispose(); 
                }
            }

            private bool IsWalkableAt(int x, int y)
            {
                var pt = new int2(x, y);
                if (!m_WalkableCache.TryGetValue(pt, out bool result))
                {
                    var weight = m_GetCostTile(m_Entity, Extension.Int2Null, pt);
                    result = weight > 0;
                    m_WalkableCache.Add(pt, result);
                }
                return result;
            }

            private void IdentifySuccessors((Node.Cost cost, int2 value) node)
            {

                int tEndX = m_EndNode.x;
                int tEndY = m_EndNode.y;
                
                int2 neighbor;
                int2? tJumpPoint;

                using (var tNeighbors = FindNeighbors(node.value))
                {
                    for (int i = 0; i < tNeighbors.Length; i++)
                    {
                        neighbor = tNeighbors[i];
                        if (m_UseRecursive)
                            tJumpPoint = Jump(neighbor.x, neighbor.y, node.value.x, node.value.y);
                        else
                            tJumpPoint = JumpLoop(neighbor.x, neighbor.y, node.value.x, node.value.y);

                        if (tJumpPoint != null)
                        {
                            if (!m_Costs.TryGetValue(tJumpPoint.Value, out Node.Cost cost))
                            {
                                cost = new Node.Cost(tJumpPoint.Value, node.value);
                                m_Costs.Add(tJumpPoint.Value, cost);
                            }

                            /*
                            if (tJumpNode == null)
                            {
                                if (m_EndNode.x == tJumpPoint.Value.x && m_EndNode.y == tJumpPoint.Value.y)
                                    tJumpNode = iParam.SearchGrid.GetNodeAt(tJumpPoint.Value);
                            }
                            */
                            /*
                            if (tJumpPoint.Value.Equals(m_EndNode))
                            {
                                UnityEngine.Debug.LogWarning($"tJumpPoint == m_EndNode");
                                m_Queue.Push(cost, tJumpPoint.Value);
                                m_Hierarchy[tJumpPoint.Value] = node.value;
                                return;
                            }
                            */

                            //!!!if (tJumpNode.isClosed)
                            if (m_Hierarchy.ContainsKey(tJumpPoint.Value) || m_Queue.Has(cost))
                                continue;
                                

                            // include distance, as parent may not be immediately adjacent:
                            float tCurNodeToJumpNodeLen = m_Heuristic(Math.Abs(tJumpPoint.Value.x - node.value.x), Math.Abs(tJumpPoint.Value.y - node.value.y));
                            float tStartToJumpNodeLen = node.cost.startToCurNodeLen + tCurNodeToJumpNodeLen; // next `startToCurNodeLen` value

                            if (!cost.Value.HasValue || (tStartToJumpNodeLen < cost.startToCurNodeLen))
                            //if (!tJumpNode.isOpened || tStartToJumpNodeLen < tJumpNode.startToCurNodeLen)
                            {
                                cost.startToCurNodeLen = tStartToJumpNodeLen;
                                cost.heuristicCurNodeToEndLen = (cost.heuristicCurNodeToEndLen == null ? m_Heuristic(Math.Abs(tJumpPoint.Value.x - tEndX), Math.Abs(tJumpPoint.Value.y - tEndY)) : cost.heuristicCurNodeToEndLen);
                                cost.heuristicStartToEndLen = cost.startToCurNodeLen + cost.heuristicCurNodeToEndLen.Value;

                                if (!m_Queue.Has(cost))
                                {
                                    m_Queue.Push(cost, tJumpPoint.Value);
                                    m_Hierarchy[tJumpPoint.Value] = node.value;
                                }
                            }
                        }
                    }
                }
            }

            private class JumpSnapshot
            {
                public int iX;
                public int iY;
                public int iPx;
                public int iPy;
                public int tDx;
                public int tDy;
                public int2? jx;
                public int2? jy;
                public int stage;
                public JumpSnapshot()
                {

                    iX = 0;
                    iY = 0;
                    iPx = 0;
                    iPy = 0;
                    tDx = 0;
                    tDy = 0;
                    jx = null;
                    jy = null;
                    stage = 0;
                }
            }

            private int2? JumpLoop(int iX, int iY, int iPx, int iPy)
            {
                int2? retVal = null;
                Stack<JumpSnapshot> stack = new Stack<JumpSnapshot>();

                JumpSnapshot currentSnapshot = new JumpSnapshot();
                JumpSnapshot newSnapshot = null;
                currentSnapshot.iX = iX;
                currentSnapshot.iY = iY;
                currentSnapshot.iPx = iPx;
                currentSnapshot.iPy = iPy;
                currentSnapshot.stage = 0;

                stack.Push(currentSnapshot);
                while (stack.Count != 0)
                {
                    currentSnapshot = stack.Pop();
                    switch (currentSnapshot.stage)
                    {
                        case 0:
                            if (!IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY))
                            {
                                retVal = null;
                                continue;
                            }
                            else if (new int2(currentSnapshot.iX, currentSnapshot.iY).Equals(m_EndNode))
                            {
                                retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                continue;
                            }

                            currentSnapshot.tDx = currentSnapshot.iX - currentSnapshot.iPx;
                            currentSnapshot.tDy = currentSnapshot.iY - currentSnapshot.iPy;
                            currentSnapshot.jx = null;
                            currentSnapshot.jy = null;
                            if (true)//iParam.CrossCorner
                            {
                                // check for forced neighbors
                                // along the diagonal
                                if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                                {
                                    if ((IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && !IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY)) ||
                                        (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY - currentSnapshot.tDy) && !IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - currentSnapshot.tDy)))
                                    {
                                        retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                                // horizontally/vertically
                                else
                                {
                                    if (currentSnapshot.tDx != 0)
                                    {
                                        // moving along x
                                        if ((IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + 1) && !IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + 1)) ||
                                            (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY - 1) && !IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - 1)))
                                        {
                                            retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if ((IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY + currentSnapshot.tDy) && !IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY)) ||
                                            (IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY + currentSnapshot.tDy) && !IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY)))
                                        {
                                            retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                            continue;
                                        }
                                    }
                                }
                                // when moving diagonally, must check for vertical/horizontal jump points
                                if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                                {
                                    currentSnapshot.stage = 1;
                                    stack.Push(currentSnapshot);

                                    newSnapshot = new JumpSnapshot();
                                    newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                    newSnapshot.iY = currentSnapshot.iY;
                                    newSnapshot.iPx = currentSnapshot.iX;
                                    newSnapshot.iPy = currentSnapshot.iY;
                                    newSnapshot.stage = 0;
                                    stack.Push(newSnapshot);
                                    continue;
                                }

                                // moving diagonally, must make sure one of the vertical/horizontal
                                // neighbors is open to allow the path

                                // moving diagonally, must make sure one of the vertical/horizontal
                                // neighbors is open to allow the path
                                if (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) || IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                                {
                                    newSnapshot = new JumpSnapshot();
                                    newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                    newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                    newSnapshot.iPx = currentSnapshot.iX;
                                    newSnapshot.iPy = currentSnapshot.iY;
                                    newSnapshot.stage = 0;
                                    stack.Push(newSnapshot);
                                    continue;
                                }
                                else if (true)//iParam.CrossAdjacentPoint
                                {
                                    newSnapshot = new JumpSnapshot();
                                    newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                    newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                    newSnapshot.iPx = currentSnapshot.iX;
                                    newSnapshot.iPy = currentSnapshot.iY;
                                    newSnapshot.stage = 0;
                                    stack.Push(newSnapshot);
                                    continue;
                                }
                            }
                            else //if (!iParam.CrossCorner)
                            {
                                // check for forced neighbors
                                // along the diagonal
                                if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                                {
                                    if ((IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy) && !IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY)) ||
                                        (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && !IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy)))
                                    {
                                        retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                                // horizontally/vertically
                                else
                                {
                                    if (currentSnapshot.tDx != 0)
                                    {
                                        // moving along x
                                        if ((IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + 1) && !IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY + 1)) ||
                                            (IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - 1) && !IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY - 1)))
                                        {
                                            retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if ((IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY) && !IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY - currentSnapshot.tDy)) ||
                                            (IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY) && !IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY - currentSnapshot.tDy)))
                                        {
                                            retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                            continue;
                                        }
                                    }
                                }


                                // when moving diagonally, must check for vertical/horizontal jump points
                                if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                                {
                                    currentSnapshot.stage = 3;
                                    stack.Push(currentSnapshot);

                                    newSnapshot = new JumpSnapshot();
                                    newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                    newSnapshot.iY = currentSnapshot.iY;
                                    newSnapshot.iPx = currentSnapshot.iX;
                                    newSnapshot.iPy = currentSnapshot.iY;
                                    newSnapshot.stage = 0;
                                    stack.Push(newSnapshot);
                                    continue;
                                }

                                // moving diagonally, must make sure both of the vertical/horizontal
                                // neighbors is open to allow the path
                                if (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                                {
                                    newSnapshot = new JumpSnapshot();
                                    newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                    newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                    newSnapshot.iPx = currentSnapshot.iX;
                                    newSnapshot.iPy = currentSnapshot.iY;
                                    newSnapshot.stage = 0;
                                    stack.Push(newSnapshot);
                                    continue;
                                }
                            }
                            retVal = null;
                            break;
                        case 1:
                            currentSnapshot.jx = retVal;

                            currentSnapshot.stage = 2;
                            stack.Push(currentSnapshot);

                            newSnapshot = new JumpSnapshot();
                            newSnapshot.iX = currentSnapshot.iX;
                            newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                            newSnapshot.iPx = currentSnapshot.iX;
                            newSnapshot.iPy = currentSnapshot.iY;
                            newSnapshot.stage = 0;
                            stack.Push(newSnapshot);
                            break;
                        case 2:
                            currentSnapshot.jy = retVal;
                            if (currentSnapshot.jx != null || currentSnapshot.jy != null)
                            {
                                retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                continue;
                            }

                            // moving diagonally, must make sure one of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) || IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                            {
                                newSnapshot = new JumpSnapshot();
                                newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                newSnapshot.iPx = currentSnapshot.iX;
                                newSnapshot.iPy = currentSnapshot.iY;
                                newSnapshot.stage = 0;
                                stack.Push(newSnapshot);
                                continue;
                            }
                            else if (true)//iParam.CrossAdjacentPoint
                            {
                                newSnapshot = new JumpSnapshot();
                                newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                newSnapshot.iPx = currentSnapshot.iX;
                                newSnapshot.iPy = currentSnapshot.iY;
                                newSnapshot.stage = 0;
                                stack.Push(newSnapshot);
                                continue;
                            }
                            retVal = null;
                            break;
                        case 3:
                            currentSnapshot.jx = retVal;

                            currentSnapshot.stage = 4;
                            stack.Push(currentSnapshot);

                            newSnapshot = new JumpSnapshot();
                            newSnapshot.iX = currentSnapshot.iX;
                            newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                            newSnapshot.iPx = currentSnapshot.iX;
                            newSnapshot.iPy = currentSnapshot.iY;
                            newSnapshot.stage = 0;
                            stack.Push(newSnapshot);
                            break;
                        case 4:
                            currentSnapshot.jy = retVal;
                            if (currentSnapshot.jx != null || currentSnapshot.jy != null)
                            {
                                retVal = new int2(currentSnapshot.iX, currentSnapshot.iY);
                                continue;
                            }

                            // moving diagonally, must make sure both of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                            {
                                newSnapshot = new JumpSnapshot();
                                newSnapshot.iX = currentSnapshot.iX + currentSnapshot.tDx;
                                newSnapshot.iY = currentSnapshot.iY + currentSnapshot.tDy;
                                newSnapshot.iPx = currentSnapshot.iX;
                                newSnapshot.iPy = currentSnapshot.iY;
                                newSnapshot.stage = 0;
                                stack.Push(newSnapshot);
                                continue;
                            }
                            retVal = null;
                            break;
                    }
                }

                return retVal;

            }
            private int2? Jump(int iX, int iY, int iPx, int iPy)
            {
                if (!IsWalkableAt(iX, iY))
                {
                    return null;
                }
                else if (new int2(iX, iY).Equals(m_EndNode))
                {
                    return new int2(iX, iY);
                }

                int tDx = iX - iPx;
                int tDy = iY - iPy;
                int2? jx = null;
                int2? jy = null;

                if (true)//iParam.CrossCorner
                {
                    // check for forced neighbors
                    // along the diagonal
                    if (tDx != 0 && tDy != 0)
                    {
                        if ((IsWalkableAt(iX - tDx, iY + tDy) && !IsWalkableAt(iX - tDx, iY)) ||
                            (IsWalkableAt(iX + tDx, iY - tDy) && !IsWalkableAt(iX, iY - tDy)))
                        {
                            return new int2(iX, iY);
                        }
                    }
                    // horizontally/vertically
                    else
                    {
                        if (tDx != 0)
                        {
                            // moving along x
                            if ((IsWalkableAt(iX + tDx, iY + 1) && !IsWalkableAt(iX, iY + 1)) ||
                                (IsWalkableAt(iX + tDx, iY - 1) && !IsWalkableAt(iX, iY - 1)))
                            {
                                return new int2(iX, iY);
                            }
                        }
                        else
                        {
                            if ((IsWalkableAt(iX + 1, iY + tDy) && !IsWalkableAt(iX + 1, iY)) ||
                                (IsWalkableAt(iX - 1, iY + tDy) && !IsWalkableAt(iX - 1, iY)))
                            {
                                return new int2(iX, iY);
                            }
                        }
                    }
                    // when moving diagonally, must check for vertical/horizontal jump points
                    if (tDx != 0 && tDy != 0)
                    {
                        jx = Jump(iX + tDx, iY, iX, iY);
                        jy = Jump(iX, iY + tDy, iX, iY);
                        if (jx != null || jy != null)
                        {
                            return new int2(iX, iY);
                        }
                    }

                    // moving diagonally, must make sure one of the vertical/horizontal
                    // neighbors is open to allow the path
                    if (IsWalkableAt(iX + tDx, iY) || IsWalkableAt(iX, iY + tDy))
                    {
                        return Jump(iX + tDx, iY + tDy, iX, iY);
                    }
                    else if (true)//iParam.CrossAdjacentPoint
                    {
                        return Jump(iX + tDx, iY + tDy, iX, iY);
                    }
                    else
                    {
                        return null;
                    }
                }
                else //if (!iParam.CrossCorner)
                {
                    // check for forced neighbors
                    // along the diagonal
                    if (tDx != 0 && tDy != 0)
                    {
                        if ((IsWalkableAt(iX + tDx, iY + tDy) && IsWalkableAt(iX, iY + tDy) && !IsWalkableAt(iX + tDx, iY)) ||
                            (IsWalkableAt(iX + tDx, iY + tDy) && IsWalkableAt(iX + tDx, iY) && !IsWalkableAt(iX, iY + tDy)))
                        {
                            return new int2(iX, iY);
                        }
                    }
                    // horizontally/vertically
                    else
                    {
                        if (tDx != 0)
                        {
                            // moving along x
                            if ((IsWalkableAt(iX, iY + 1) && !IsWalkableAt(iX - tDx, iY + 1)) ||
                                (IsWalkableAt(iX, iY - 1) && !IsWalkableAt(iX - tDx, iY - 1)))
                            {
                                return new int2(iX, iY);
                            }
                        }
                        else
                        {
                            if ((IsWalkableAt(iX + 1, iY) && !IsWalkableAt(iX + 1, iY - tDy)) ||
                                (IsWalkableAt(iX - 1, iY) && !IsWalkableAt(iX - 1, iY - tDy)))
                            {
                                return new int2(iX, iY);
                            }
                        }
                    }


                    // when moving diagonally, must check for vertical/horizontal jump points
                    if (tDx != 0 && tDy != 0)
                    {
                        jx = Jump(iX + tDx, iY, iX, iY);
                        jy = Jump(iX, iY + tDy, iX, iY);
                        if (jx != null || jy != null)
                        {
                            return new int2(iX, iY);
                        }
                    }

                    // moving diagonally, must make sure both of the vertical/horizontal
                    // neighbors is open to allow the path
                    if (IsWalkableAt(iX + tDx, iY) && IsWalkableAt(iX, iY + tDy))
                    {
                        return Jump(iX + tDx, iY + tDy, iX, iY);
                    }
                    else
                    {
                        return null;
                    }
                }

            }

            private NativeList<int2> FindNeighbors(int2 value)
            {
                int tX = value.x;
                int tY = value.y;
                int tPx, tPy, tDx, tDy;

                var tNeighbors = new NativeList<int2>(m_Directs.Length, Allocator.TempJob);

                // directed pruning: can ignore most neighbors, unless forced.
                if (m_Hierarchy.TryGetValue(value, out int2 parent))
                {
                    tPx = parent.x;
                    tPy = parent.y;
                    // get the normalized direction of travel
                    tDx = (tX - tPx) / Math.Max(Math.Abs(tX - tPx), 1);
                    tDy = (tY - tPy) / Math.Max(Math.Abs(tY - tPy), 1);

                    if (true) //iParam.CrossCorner
                    {
                        // search diagonally
                        if (tDx != 0 && tDy != 0)
                        {
                            if (IsWalkableAt(tX, tY + tDy))
                            {
                                tNeighbors.Add(new int2(tX, tY + tDy));
                            }
                            if (IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new int2(tX + tDx, tY));
                            }

                            if (IsWalkableAt(tX + tDx, tY + tDy))
                            {
                                if (IsWalkableAt(tX, tY + tDy) || IsWalkableAt(tX + tDx, tY))
                                {
                                    tNeighbors.Add(new int2(tX + tDx, tY + tDy));
                                }
                                else if (true)//iParam.CrossAdjacentPoint
                                {
                                    tNeighbors.Add(new int2(tX + tDx, tY + tDy));
                                }
                            }

                            if (IsWalkableAt(tX - tDx, tY + tDy))
                            {
                                if (IsWalkableAt(tX, tY + tDy) && !IsWalkableAt(tX - tDx, tY))
                                {
                                    tNeighbors.Add(new int2(tX - tDx, tY + tDy));
                                }
                            }

                            if (IsWalkableAt(tX + tDx, tY - tDy))
                            {
                                if (IsWalkableAt(tX + tDx, tY) && !IsWalkableAt(tX, tY - tDy))
                                {
                                    tNeighbors.Add(new int2(tX + tDx, tY - tDy));
                                }
                            }
                        }
                        // search horizontally/vertically
                        else
                        {
                            if (tDx == 0)
                            {
                                if (IsWalkableAt(tX, tY + tDy))
                                {
                                    tNeighbors.Add(new int2(tX, tY + tDy));

                                    if (IsWalkableAt(tX + 1, tY + tDy) && !IsWalkableAt(tX + 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX + 1, tY + tDy));
                                    }
                                    if (IsWalkableAt(tX - 1, tY + tDy) && !IsWalkableAt(tX - 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX - 1, tY + tDy));
                                    }
                                }
                                else if (true)//iParam.CrossAdjacentPoint
                                {
                                    if (IsWalkableAt(tX + 1, tY + tDy) && !IsWalkableAt(tX + 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX + 1, tY + tDy));
                                    }
                                    if (IsWalkableAt(tX - 1, tY + tDy) && !IsWalkableAt(tX - 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX - 1, tY + tDy));
                                    }
                                }
                            }
                            else
                            {
                                if (IsWalkableAt(tX + tDx, tY))
                                {
                                    tNeighbors.Add(new int2(tX + tDx, tY));

                                    if (IsWalkableAt(tX + tDx, tY + 1) && !IsWalkableAt(tX, tY + 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY + 1));
                                    }
                                    if (IsWalkableAt(tX + tDx, tY - 1) && !IsWalkableAt(tX, tY - 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY - 1));
                                    }
                                }
                                else if (true)//iParam.CrossAdjacentPoint
                                {
                                    if (IsWalkableAt(tX + tDx, tY + 1) && !IsWalkableAt(tX, tY + 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY + 1));
                                    }
                                    if (IsWalkableAt(tX + tDx, tY - 1) && !IsWalkableAt(tX, tY - 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY - 1));
                                    }
                                }
                            }
                        }
                    }
                    else // if(!iParam.CrossCorner)
                    {
                        // search diagonally
                        if (tDx != 0 && tDy != 0)
                        {
                            if (IsWalkableAt(tX, tY + tDy))
                            {
                                tNeighbors.Add(new int2(tX, tY + tDy));
                            }
                            if (IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new int2(tX + tDx, tY));
                            }

                            if (IsWalkableAt(tX + tDx, tY + tDy))
                            {
                                if (IsWalkableAt(tX, tY + tDy) && IsWalkableAt(tX + tDx, tY))
                                    tNeighbors.Add(new int2(tX + tDx, tY + tDy));
                            }

                            if (IsWalkableAt(tX - tDx, tY + tDy))
                            {
                                if (IsWalkableAt(tX, tY + tDy) && IsWalkableAt(tX - tDx, tY))
                                    tNeighbors.Add(new int2(tX - tDx, tY + tDy));
                            }

                            if (IsWalkableAt(tX + tDx, tY - tDy))
                            {
                                if (IsWalkableAt(tX, tY - tDy) && IsWalkableAt(tX + tDx, tY))
                                    tNeighbors.Add(new int2(tX + tDx, tY - tDy));
                            }


                        }
                        // search horizontally/vertically
                        else
                        {
                            if (tDx == 0)
                            {
                                if (IsWalkableAt(tX, tY + tDy))
                                {
                                    tNeighbors.Add(new int2(tX, tY + tDy));

                                    if (IsWalkableAt(tX + 1, tY + tDy) && IsWalkableAt(tX + 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX + 1, tY + tDy));
                                    }
                                    if (IsWalkableAt(tX - 1, tY + tDy) && IsWalkableAt(tX - 1, tY))
                                    {
                                        tNeighbors.Add(new int2(tX - 1, tY + tDy));
                                    }
                                }
                                if (IsWalkableAt(tX + 1, tY))
                                    tNeighbors.Add(new int2(tX + 1, tY));
                                if (IsWalkableAt(tX - 1, tY))
                                    tNeighbors.Add(new int2(tX - 1, tY));
                            }
                            else
                            {
                                if (IsWalkableAt(tX + tDx, tY))
                                {
                                    tNeighbors.Add(new int2(tX + tDx, tY));

                                    if (IsWalkableAt(tX + tDx, tY + 1) && IsWalkableAt(tX, tY + 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY + 1));
                                    }
                                    if (IsWalkableAt(tX + tDx, tY - 1) && IsWalkableAt(tX, tY - 1))
                                    {
                                        tNeighbors.Add(new int2(tX + tDx, tY - 1));
                                    }
                                }
                                if (IsWalkableAt(tX, tY + 1))
                                    tNeighbors.Add(new int2(tX, tY + 1));
                                if (IsWalkableAt(tX, tY - 1))
                                    tNeighbors.Add(new int2(tX, tY - 1));
                            }
                        }
                    }

                }
                // return all neighbors
                else
                {
                    var map = m_Map;
                    var neighbors = tNeighbors.AsParallelWriter();
                    Parallel.For(0, m_Directs.Length,
                        (idx) =>
                        {
                            var neighbor = map.GetTile(value.x, value.y, idx);
                            if (!neighbor.IsNull())
                                neighbors.AddNoResize(neighbor);
                        });
                }
                return tNeighbors;
            }

            public enum HeuristicMode
            {
                MANHATTAN,
                EUCLIDEAN,
                CHEBYSHEV,

            };

            public class Heuristic
            {
                public static float Manhattan(int iDx, int iDy)
                {
                    return (float)iDx + iDy;
                }

                public static float Euclidean(int iDx, int iDy)
                {
                    float tFdx = (float)iDx;
                    float tFdy = (float)iDy;
                    return (float)Math.Sqrt((double)(tFdx * tFdx + tFdy * tFdy));
                }

                public static float Chebyshev(int iDx, int iDy)
                {
                    return (float)Math.Max(iDx, iDy);
                }

            }
        }
    }
}