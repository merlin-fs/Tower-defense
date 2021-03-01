using System;
using System.Collections.Generic;

namespace St.Common
{
    public interface IDIContextContainer
    {
        void Bind<T>(object instance, object id = null);
        //where T : new();
        void UnBind<T>(object instance, object id = null);
        //where T : new();
        void UnBindAll();
        T TryGet<T>(object id = null);
        //where T : new();
        T TryGet<T>(Type type, object id = null);
        //where T : new();
    }

    public class DIContextContainer : IDIContextContainer
    {
        private readonly Dictionary<ContainerType, object> m_Instances = new Dictionary<ContainerType, object>();

        #region IDIContextContainer
        void IDIContextContainer.Bind<T>(object instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            if (m_Instances.ContainsKey(t))
                Debug.Assert.Check(false, "DIContextContainer.Bind: instance already exists");
            m_Instances[t] = instance;
        }
        
        void IDIContextContainer.UnBind<T>(object instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            m_Instances.Remove(t);
        }

        void IDIContextContainer.UnBindAll() => m_Instances.Clear();

        T IDIContextContainer.TryGet<T>(object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            return !m_Instances.TryGetValue(t, out object result) 
                ? default 
                : (T)result;
        }
        T IDIContextContainer.TryGet<T>(Type type, object id)
        {
            ContainerType t = new ContainerType(type, id);
            return !m_Instances.TryGetValue(t, out object result)
                ? default
                : (T)result;
        }

        #endregion
        struct ContainerType
        {
            public ContainerType(Type obj, object id = null)
            {
                Obj = obj;
                Id = id;
            }

            public Type Obj { get; private set; }
            
            public object Id { get; private set; }
            
            public override int GetHashCode() => Obj.GetHashCode();
            
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                ContainerType other = (ContainerType)obj;
                return other.Obj == this.Obj && other.Id == this.Id;
            }
        }
    }
}