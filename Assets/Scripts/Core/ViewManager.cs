using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using St.Common;

namespace TowerDefense.Core.View
{
    public interface IViewFactory
    {
        ISliceVisualizer<I> Get<I>()
            where I : ISlice;
    }

    public interface IViewManager
    {
        void Add<I>(ISliceVisualizer<I> visualizer)
            where I : ISlice;

        IReadOnlyCollection<ISliceVisualizer<I>> Get<I>(I slice)
            where I : ISlice;
    }


    public class ViewManager : IViewManager
    {
        private Dictionary<Type, List<ISliceVisualizer>> m_Views = new Dictionary<Type, List<ISliceVisualizer>>();

        void IViewManager.Add<I>(ISliceVisualizer<I> visualizer)
        {
            List<ISliceVisualizer> list = GetList<I>(true);
            list.Add(visualizer);
        }

        IReadOnlyCollection<ISliceVisualizer<I>> IViewManager.Get<I>(I slice)
        {
            List<ISliceVisualizer> list = GetList<I>(false);
            return list
                .Cast<ISliceVisualizer<I>>()
                .ToList();
        }

        List<ISliceVisualizer> GetList<I>(bool need)
        {
            Type type = typeof(I);
            if (!m_Views.TryGetValue(type, out List<ISliceVisualizer> list) && need)
            {
                list = new List<ISliceVisualizer>();
                m_Views.Add(type, list);
            }
            return list; 
        }
    }
}