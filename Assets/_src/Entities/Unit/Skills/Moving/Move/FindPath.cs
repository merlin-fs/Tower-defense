using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Units.Skills
{
    using World;

    public partial class Move
    {
        public static bool FindPath(Map map, Entity entity, ref Moving moving, ref Map.Path.Info info, DynamicBuffer<Map.Path.Points> points, DynamicBuffer<Map.Path.Times> times)
        {
            var path = Map.PathFinder.Execute(map.GetCostTile, entity, moving.CurrentPosition, moving.TargetPosition, map);
            try
            {
                if (path.Length < 2)
                {
                    return false;
                }

                moving.TargetPosition = path[0];

                points.ResizeUninitialized(path.Length);

                int timeLen = path.Length;
                float step = 1f / timeLen;
                times.ResizeUninitialized(timeLen);

                for (int j = 0; j < path.Length; j++)
                    points[j] = map.MapToWord(path[path.Length - (j + 1)]);

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
            finally
            {
                path.Dispose();
            }
        }
    }
}