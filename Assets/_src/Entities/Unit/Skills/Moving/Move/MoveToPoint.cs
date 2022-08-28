using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model.Skills
{
    using World;

    public partial class Move
    {
        private static float3 UP = new float3(0f, 1f, 0f);

        public static bool MoveToPoint(float delta, ref Moving moving,
            Map.Path.Info info, DynamicBuffer<Map.Path.Points> points, DynamicBuffer<Map.Path.Times> times,
            ref Translation translation, ref Rotation rotation)
        {
            if (moving.PathPrecent >= 1f)
            {
                moving.CurrentPosition = moving.TargetPosition;
                return true;
            }

            float speed = moving.Def.Link.Speed * 0.01f * info.DeltaTime;
            moving.PathPrecent += delta * speed;

            //float time = moving.PathPrecent;
            float time = Map.Path.ConvertToConstantPathTime(moving.PathPrecent, info.Length, times.AsNativeArray());
            float3 position = Map.Path.GetPosition(time, false, points.AsNativeArray(), info.DeltaTime);

            var look = math.normalize(position - translation.Value);
            //look.y = -0.5f;

            rotation.Value = quaternion.LookRotation(look, UP);
            //rotation.Value = quaternion.LookRotation(position - translation.Value, UP);
            translation.Value = position;
            return false;
        }
    }
}