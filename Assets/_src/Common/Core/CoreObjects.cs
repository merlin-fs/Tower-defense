using System;
using UnityEngine;

namespace St.Common.Core
{
    public interface ICoreObject
    {
    }

    public interface ICoreGameObject : ICoreObject
    {
        GameObject GameObject { get; }
    }

    public interface ICoreDisposable
    {
        event Action<ICoreDisposable> OnDispose;
        void Dispose();
    }

    public interface ICoreObjectInstantiate : ICoreObject, ICoreDisposable
    {
        ICoreObjectInstantiate Instantiate();
        T Instantiate<T>() where T : ICoreObject;
    }

    public interface ICoreGameObjectInstantiate : ICoreObjectInstantiate, ICoreGameObject
    {
    }


    [Serializable]
    public class CoreObjectContainer : TypedContainer<ICoreObject> { }
    [Serializable]
    public class CoreGameObjectContainer : TypedContainer<ICoreGameObject> { }
    [Serializable]
    public class CoreObjectInstantiateContainer : TypedContainer<ICoreObjectInstantiate> { }

    [Serializable]
    public class CoreGameObjectInstantiateContainer : TypedContainer<ICoreGameObjectInstantiate> { }
}