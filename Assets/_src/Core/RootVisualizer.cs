using System;
using UnityEngine;
using St.Common.Core;

namespace TowerDefense.Core.View
{
    public interface IRootVisualizer: ICoreGameObjectInstantiate, ISliceVisualizer 
    { }

    public class RootVisualizer : MonoBehaviour, IRootVisualizer
    {
        private ICoreGameObjectInstantiate Self => this;

        private ISliceVisualizer[] m_Childs;

        private void Awake()
        {
            m_Childs = GetComponents<ISliceVisualizer>();
        }

        #region ICoreObjectInstantiate
        ICoreObjectInstantiate ICoreObjectInstantiate.Instantiate()
        {
            return Instantiate(gameObject).GetComponent<RootVisualizer>();
        }

        T ICoreObjectInstantiate.Instantiate<T>()
        {
            return (T)Self.Instantiate();
        }

        GameObject ICoreGameObject.GameObject => gameObject;

        void IDisposable.Dispose()
        {
            Destroy(gameObject);
        }
        #endregion
        #region  ISliceVisualizer
        void ISliceVisualizer.UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            foreach (var iter in m_Childs)
                if (iter != (ISliceVisualizer)this)
                    iter.UpdateView(unit, slice, deltaTime);
        }
        #endregion

    }

    [Serializable]
    public class VisualizerContainer : TypedContainer<IRootVisualizer> 
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_Obj != null)
                if (m_Obj.gameObject.GetComponent<IRootVisualizer>() == null)
                    m_Obj = null;
        }
    }
}