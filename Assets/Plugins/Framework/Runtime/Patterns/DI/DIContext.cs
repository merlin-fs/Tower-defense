using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Common.Core
{
    public interface IDIContext
    {
        T Get<T>(object id = null);
        
        bool TryGet<T>(out T value, object id = null);

        IDIContext NewContext(Action<DIBindContext> onBind);
        void RemoveContext(ref IDIContext context);
    }

    public abstract class DIBindContext
    {
        public abstract void Bind<T>(T instance, object id = null);
    }

    public abstract class DIContext : MonoBehaviour, IDIContext
    {
        [SerializeField]
        private bool m_IsRoot = false;

        private static IDIContext m_Root;

        public static IDIContext Root => m_Root;

        private static readonly HashSet<IDIContext> m_Containers = new HashSet<IDIContext>();

        private readonly IDIContextContainer m_Container = new DIContextContainer();

        #region IDIContext
        public T Get<T>(object id)
        {
            if (m_Container.TryGet<T>(out T value, id))
                return value;

            foreach (var iter in m_Containers)
            {
                if (iter == (IDIContext)this)
                    continue;

                if (iter.TryGet<T>(out value, id))
                    return value;
            }
            return default;
        }

        public bool TryGet<T>(out T value, object id)
        {
            return m_Container.TryGet<T>(out value, id);
        }

        public IDIContext NewContext(Action<DIBindContext> onBind)
        {
            return new DIContextRuntime(this, onBind);
        }

        public void RemoveContext(ref IDIContext context)
        {
            if (context is DIBindContext)
            {
                Pop(context);
                context = null;
            }
        }

        #endregion

        protected virtual void Awake()
        {
            if (m_IsRoot) 
            {
                if (m_Root != null)
                    (m_Root as DIContext).m_IsRoot = false;
                m_Root = this;
            }
            Push(this);
            OnBind();

            foreach (var iter in GetComponents<IInjectionInitable>())
            {
                iter.Init(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_IsRoot)
                m_Root = null;
            Pop(this);
        }

        protected abstract void OnBind();

        private void Push(IDIContext context)
        {
            m_Containers.Add(context);
        }

        private void Pop(IDIContext context)
        { 
            m_Containers.Remove(context); 
        }
        protected void Bind<T>(T instance, object id = null)
        {
            m_Container.Bind<T>(instance, id);
        }

        private class DIContextRuntime : DIBindContext, IDIContext
        {
            private readonly DIContext m_Root;

            private readonly IDIContextContainer m_Container = new DIContextContainer();


            public IDIContext NewContext(Action<DIBindContext> onBind)
            {
                return m_Root.NewContext(onBind);
            }

            public void RemoveContext(ref IDIContext context)
            {
                m_Root.RemoveContext(ref context);
            }

            public DIContextRuntime(DIContext root, Action<DIBindContext> onBind)
            {
                m_Root = root;
                m_Root.Push(this);
                onBind.Invoke(this);
            }

            public void Remove()
            {
                m_Root.Pop(this);
            }

            #region IDIContext
            public T Get<T>(object id)
            {
                if (m_Container.TryGet<T>(out T value, id))
                    return value;
                return m_Root.Get<T>(id);
            }

            public bool TryGet<T>(out T value, object id)
            {
                return m_Container.TryGet<T>(out value, id);
            }
            #endregion

            public override void Bind<T>(T instance, object id = null)
            {
                m_Container.Bind<T>(instance, id);
            }
        }

    }
}