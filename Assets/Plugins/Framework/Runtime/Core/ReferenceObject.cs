using System;
using System.Runtime.InteropServices;

namespace Common.Core
{
    public struct ReferenceObject<T> : IDisposable
    {
        private GCHandle m_Handle;

        public T Link => (T)m_Handle.Target;

        public GCHandle Info => m_Handle;

        public ReferenceObject(object target)
        {
            m_Handle = GCHandle.Alloc(target);
        }

        public ReferenceObject(GCHandle handle)
        {
            m_Handle = handle;
        }

        public void Dispose()
        {
            m_Handle.Free();
        }
    }
}