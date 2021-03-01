using System;
using UnityEngine;
using St.Common.Core;

namespace TowerDefense.Core.View
{
    public class RootVisualizer : MonoBehaviour, ICoreGameObjectInstantiate
    {
        private ICoreGameObjectInstantiate Self => this;

        #region ICoreObjectInstantiate
        ICoreObjectInstantiate ICoreObjectInstantiate.Instantiate()
        {
            return Instantiate(this);
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
    }

    [Serializable]
    public class VisualizerContainer : TypedContainer<ICoreGameObjectInstantiate> 
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_Obj != null)
                if (m_Obj.gameObject.GetComponent<ISliceVisualizer>() == null)
                    m_Obj = null;
        }
    }
}