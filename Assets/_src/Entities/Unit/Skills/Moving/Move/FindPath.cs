using System;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Model.Units.Skills
{
    using World;

    public partial class Move
    {

        public static void FindPath(Map.Data map, Entity entity, Moving moving, Action<NativeArray<int2>> callback)
        {
            var m = moving;
            Task<NativeArray<int2>>.Run(() =>
            {
                var param = new EpPathFinding.JumpPointParam(new EpPathFinding.Grid(map, entity),
                    new EpPathFinding.GridPos(m.CurrentPosition.x, m.CurrentPosition.y),
                    new EpPathFinding.GridPos(m.TargetPosition.x, m.TargetPosition.y));

                try
                {
                    var list = EpPathFinding.JumpPointFinder.FindPath(param);
                    return new NativeArray<int2>(list.Select(i => new int2(i.x, i.y)).ToArray(), Allocator.TempJob);
                }
                catch (Exception e)
                {
                    throw e;
                }

                //return Map.PathFinder.Execute(map.GetCostTile, entity, m.CurrentPosition, m.TargetPosition, map);
            })
                .ContinueWith((task) =>
                {
                    callback(task.Result);
                });
        }

        public static bool FindPath(Map.Data map, ref Moving moving, ref Map.Path.Info info, DynamicBuffer<Map.Path.Points> points, DynamicBuffer<Map.Path.Times> times)
        {
            var path = points;
            UnityEngine.Debug.Log($"thread: {Thread.CurrentThread.ManagedThreadId}");
            if (path.Length < 2)
            {
                return false;
            }

            moving.TargetPosition = new int2((int)path[path.Length-1].Value.x, (int)path[path.Length-1].Value.y);
            points.ResizeUninitialized(path.Length);

            int timeLen = path.Length;
            float step = 1f / timeLen;
            times.ResizeUninitialized(timeLen);

            for (int j = 0; j < path.Length; j++)
                points[j] = map.MapToWord(new int2((int)path[j].Value.x, (int)path[j].Value.y));

            info.DeltaTime = 1f / (points.Length - 1);
            var pts = points.AsNativeArray();

            float len = 0f;
            float3 vector = Map.Path.GetPosition(0f, false, pts, info.DeltaTime);
            for (int j = 0; j < timeLen; j++)
            {
                float pos = step * (j + 1);
                float3 point = Map.Path.GetPosition(pos, false, pts, info.DeltaTime);
                len += math.distance(vector, point);
                vector = point;

                times[j] = new Map.Path.Times()
                {
                    Time = pos,
                    Length = len,
                };
            };
            info.Length = len;
            /*
            float diff = 0;
            vector = pts[0].Value;
            len = 0;
            for (int j = 0; j < timeLen; j++)
            {
                var time = times[j];
                var staticTime = step * (j + 1);

                int p = (int)(staticTime / info.DeltaTime);
                var point = pts[p].Value;
                len += math.distancesq(vector, point);
                vector = point;
                var staticLen = info.Length * staticTime;

                diff = (time.Time / time.Length) * (time.Length - staticLen);

                time.Time += diff;
                time.Length = len;

                times[j] = time;
            }
            */
            return true;
        }
    }
}