using UnityEngine;

namespace St.Common.Core.DI
{
    public interface IDIContext
    {
        T TryGet<T>(object id = null) where T : new();
    }


    public abstract class DIContext : MonoBehaviour, IDIContext
    {
        private readonly IDIContextContainer m_Container = new DIContextContainer();
        #region IDIContext
        T IDIContext.TryGet<T>(object id) => m_Container.TryGet<T>(id);
        #endregion
        private void Awake()
        {
            DI.Push(m_Container);
            OnBind();
        }
        private void OnDestroy() => DI.Pop(m_Container);
        protected abstract void OnBind();
        protected void Bind<T>(T instance, object id = null) 
            where T : new()
        { 
            m_Container.Bind<T>(instance, id); 
        }
    }
}