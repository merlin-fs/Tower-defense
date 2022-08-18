using System;
using System.Runtime.CompilerServices;

namespace Unity.Mathematics
{
    public static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float magnitude(this float3 self)
        {
            return (float)math.sqrt(self.x * self.x + self.y * self.y + self.z * self.z);
        }

        public static float magnitude(this int2 self)
        {
            return (float)math.sqrt(self.x * self.x + self.y * self.y);
        }

        public static quaternion ClampRotation(this quaternion q, float3 bounds)
        {
            q = q.norm();
            float angleX = 2.0f * math.degrees(math.atan(q.value.x));
            angleX = math.clamp(angleX, -bounds.x, bounds.x);
            q.value.x = math.tan(0.5f * math.radians(angleX));

            float angleY = 2.0f * math.degrees(math.atan(q.value.y));
            angleY = math.clamp(angleY, -bounds.y, bounds.y);
            q.value.y = math.tan(0.5f * math.radians(angleY));

            float angleZ = 2.0f * math.degrees(math.atan(q.value.z));
            angleZ = math.clamp(angleZ, -bounds.z, bounds.z);
            q.value.z = math.tan(0.5f * math.radians(angleZ));

            return math.normalize(q);
        }

        private static quaternion norm(this quaternion q)
        {
            q.value.x /= q.value.w;
            q.value.y /= q.value.w;
            q.value.z /= q.value.w;
            q.value.w = 1.0f;
            return q;
        }

        public unsafe static quaternion ClampRotationX(this quaternion q, float min, float max, bool* isClamp = null)
        {
            q = q.norm();
            float input = 2.0f * math.degrees(math.atan(q.value.x));
            float angleX = math.clamp(input, min, max);
            if (isClamp != null)
                *isClamp = input != angleX;
            q.value.x = math.tan(0.5f * math.radians(angleX));
            return math.normalize(q);
        }

        public unsafe static quaternion ClampRotationY(this quaternion q, float min, float max, bool* isClamp = null)
        {
            q = q.norm();
            float input = 2.0f * math.degrees(math.atan(q.value.y));
            float angleY = math.clamp(input, min, max);
            if (isClamp != null)
                *isClamp = input != angleY;
            q.value.y = math.tan(0.5f * math.radians(angleY));
            return math.normalize(q);
        }

        public unsafe static quaternion ClampRotationZ(this quaternion q, float min, float max, bool* isClamp = null)
        {
            q = q.norm();
            float input = 2.0f * math.degrees(math.atan(q.value.z));
            float angleZ = math.clamp(input, min, max);
            if (isClamp != null)
                *isClamp = input != angleZ;
            q.value.z = math.tan(0.5f * math.radians(angleZ));
            return math.normalize(q);
        }
    }
}
