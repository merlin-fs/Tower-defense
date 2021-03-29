using System;

namespace UnityEngine
{
    public static class GameObjectExt
    {
        public static bool IsPrefab(this GameObject self)
        {
            return (self.scene.buildIndex < 0);
        }
    }
}