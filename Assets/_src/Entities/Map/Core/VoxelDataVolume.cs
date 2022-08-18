using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


namespace Game.Model.World.VoxelData
{
    public struct DataVolume
    {
        public delegate bool TryGetVoxelData(DataVolume data, int3 localPosition, out float voxelData);

        [NativeDisableUnsafePtrRestriction]
        private readonly IntPtr m_Method;

        public int Width;
        public int Depth;
        public int Height;
        public int3 Size;
        public int Length;
        
        public TryGetVoxelData GetVoxelData => System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<TryGetVoxelData>(m_Method);

        public DataVolume(TryGetVoxelData getVoxelData, int width, int depth, int height)
        {
            Width = width;
            Depth = depth;
            Height = height;
            Size = new int3(width, height, depth);
            Length = width * height * depth;
            m_Method = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(getVoxelData);
        }
    }

}