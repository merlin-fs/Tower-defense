//using Sirenix.OdinInspector;

using System;
using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Core.View;
using UnityEngine;


public interface ICollation<out I> where I : ISlice
{
}

[System.Serializable]
public class CollationConfig : ICollation<ISlice>
{
    //[ShowInInspector, ValueDropdown("GetTypes")]
    public Type Value;
    //[ShowInInspector, ValueDropdown("GetVisualizers")]
    [SerializeReference]
    public ISliceVisualizer<ISlice> Visualizer;
#if UNITY_EDITOR
    private List<Type> m_Types = null;
    private IEnumerable<Type> GetTypes()
    {
        if (m_Types == null)
        {
            m_Types = new List<Type>();
            foreach (var type in typeof(ISlice).GetFilteredTypeList())
                m_Types.Add(type);
        }
        return m_Types;
    }
    private List<Type> m_Visualizers = null;
    private IEnumerable<Type> GetVisualizers()
    {
        if (m_Visualizers == null)
        {
            m_Visualizers = new List<Type>();
            foreach (var type in typeof(ISliceVisualizer).GetFilteredTypeList())
                m_Visualizers.Add(type);
        }
        return m_Visualizers;
    }
#endif
}

public class ProperiesConfig : MonoBehaviour//Serialized
{
    public Dictionary<string, List<CollationConfig>> CollationConfigs = new Dictionary<string, List<CollationConfig>>();
    public static ProperiesConfig Inst { get; set; }
    private void Awake()
    {
        if (Inst != null)
            return;
        Inst = this;
    }
    private void Start()
    {
    }
    private void Reset()
    {
    }
}
