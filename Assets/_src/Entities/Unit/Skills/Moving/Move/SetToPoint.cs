using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model.Units.Skills
{
    using World;

    public partial class Move
    {
        public static bool SetToPoint(Map map, ref Moving moving,
            ref Translation translation, ref Rotation rotation)
        {

            float3 position = map.MapToWord(moving.TargetPosition);
            moving.CurrentPosition = moving.TargetPosition;
            //rotation.Value = quaternion.LookRotation(math.normalize(position - translation.Value), UP);
            translation.Value = position;
            
            return true;
        }
    }
}