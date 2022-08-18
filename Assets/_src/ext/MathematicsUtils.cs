using System;

namespace Unity.Mathematics
{
    public static partial class utils
    {
        public static bool SpheresIntersect(float3 sphere1, float radius1, float3 sphere2, float radius2, out float3 ip, float threshold = 0.1f)
        {
            // vector from sphere 1 -> sphere 2
            float3 ab = sphere1 - sphere2;

            // Calculate radius from Unity built-in sphere.
            // Unity spheres are unit spheres (diameter = 1)
            // So diameter = scale, thus radius = scale / 2.0f.
            // **Presumes uniform scaling.

            // When spheres are too close or too far apart, ignore intersection.
            var magnitude = ab.magnitude();

            float diff = radius1 + radius2 - magnitude;
            if (diff < threshold)
            {
                ip = float3.zero;
                return false;
            }
            // Intersection is the distance along the vector between
            // the 2 spheres as a function of the sphere's radius.
            ip = sphere1 + ab * radius1 / magnitude;
            return true;
        }
    }
}
