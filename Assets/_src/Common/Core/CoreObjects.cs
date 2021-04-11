using System;
using UnityEngine;

namespace St.Common.Core
{
    public interface ICoreObject
    {
    }

    public interface ICoreMonoObject : ICoreObject
    {
        GameObject GameObject { get; }
    }

    public interface ICoreDisposable
    {
        event Action<ICoreDisposable> OnDispose;
        void Dispose();
    }

    public interface ICoreInstantiate : ICoreObject, ICoreDisposable
    {
        ICoreInstantiate Instantiate();
        T Instantiate<T>() where T : ICoreObject;
    }

    public interface ICoreGameObjectInstantiate : ICoreInstantiate, ICoreMonoObject
    {
    }


    [Serializable]
    public class CoreObjectContainer : TypedContainer<ICoreObject> { }
    [Serializable]
    public class CoreGameObjectContainer : TypedContainer<ICoreMonoObject> { }
    [Serializable]
    public class CoreObjectInstantiateContainer : TypedContainer<ICoreInstantiate> { }

    [Serializable]
    public class CoreGameObjectInstantiateContainer : TypedContainer<ICoreGameObjectInstantiate> { }
}