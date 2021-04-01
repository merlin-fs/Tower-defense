using System;
using System.Collections.Generic;

namespace St.Common.Core
{
    public interface IDIContextContainer
    {
        void Bind<T>(T instance, object id = null) where T : class;
        void UnBind<T>(T instance, object id = null) where T : class;
        void UnBindAll();
        T TryGet<T>(object id = null) where T : class;
    }

    public class DIContextContainer : IDIContextContainer
    {
        readonly Dictionary<ContainerType, object> m_Instances = new Dictionary<ContainerType, object>();

        #region IDIContextContainer

        void IDIContextContainer.Bind<T>(T instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            m_Instances[t] = instance;
        }

        void IDIContextContainer.UnBind<T>(T instance, object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);
            m_Instances.Remove(t);
        }

        void IDIContextContainer.UnBindAll()
        { 
            m_Instances.Clear(); 
        }

        T IDIContextContainer.TryGet<T>(object id)
        {
            ContainerType t = new ContainerType(typeof(T), id);

            return !m_Instances.TryGetValue(t, out object result) 
                ? null 
                : result as T;
        }

        #endregion

        class ContainerType
        {
            public ContainerType(Type obj, object id = null)
            {
                Obj = obj;
                Id = id;
            }

            public Type Obj { get; private set; }
            public object Id { get; private set; }

            public override int GetHashCode()
            { return Obj.GetHashCode(); }

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