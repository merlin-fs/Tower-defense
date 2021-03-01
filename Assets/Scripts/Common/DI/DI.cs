using System;
using System.Collections.Generic;

namespace St.Common
{
    public static class DI
    {
        private static readonly IDIContextContainer m_DefaultContainer = new DIContextContainer();
        private static readonly HashSet<IDIContextContainer> m_Containers = new HashSet<IDIContextContainer>();
        #region DI
        public static void Push(IDIContextContainer context) => m_Containers.Add(context);
        public static void Pop(IDIContextContainer context) => m_Containers.Remove(context);
        public static void Bind<T>(object instance, object id = null) 
        {
            m_DefaultContainer.Bind<T>(instance, id);
        }
        public static void UnBind<T>(object instance, object id = null) 
        {
            m_DefaultContainer.UnBind<T>(instance, id);
        }

        public static void UnBindAll()
        {
            m_DefaultContainer.UnBindAll();
            m_Containers.Clear();
        }

        public static T Get<T>(object id = null) 
        {
            T result = m_DefaultContainer.TryGet<T>(id);
            if (result != null)
                return result;

            foreach (IDIContextContainer curContainer in m_Containers)
            {
                result = curContainer.TryGet<T>(id);
                if (result != null)
                    return result;
            }
            return default;
        }
        public static T Get<T>(Type type, object id = null)
        {
            T result = m_DefaultContainer.TryGet<T>(type, id);
            if (result != null)
                return result;

            foreach (IDIContextContainer curContainer in m_Containers)
            {
                result = curContainer.TryGet<T>(type, id);
                if (result != null)
                    return result;
            }
            return default;
        }
        #endregion
    }
}