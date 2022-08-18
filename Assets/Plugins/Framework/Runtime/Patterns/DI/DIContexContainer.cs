using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Core
{
    public interface IDIContextContainer
    {
        void Bind<T>(object instance, object id = null);

        void UnBind<T>(object instance, object id = null);

        void UnBindAll();

        bool TryGet<T>(out T value, object id = null);

        bool TryGet(Type type, out object value, object id = null);
    }

    public class DIContextContainer : IDIContextContainer
    {
        private readonly Dictionary<ContainerType, object> m_Instances = new Dictionary<ContainerType, object>();

        #region IDIContextContainer
        void IDIContextContainer.Bind<T>(object instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            if (m_Instances.ContainsKey(t))
                Debug.LogError("DIContextContainer.Bind: instance already exists");
            m_Instances[t] = instance;
        }
        
        void IDIContextContainer.UnBind<T>(object instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            m_Instances.Remove(t);
        }

        void IDIContextContainer.UnBindAll() => m_Instances.Clear();

        public bool TryGet<T>(out T value, object id)
        {
            value = default;
            var result = TryGet(typeof(T), out object val, id);
            if (result)
                value = (T)val;
            return result;
        }

        public bool TryGet(Type type, out object value, object id)
        {
            ContainerType t = new ContainerType(type, id);
            return m_Instances.TryGetValue(t, out value);
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