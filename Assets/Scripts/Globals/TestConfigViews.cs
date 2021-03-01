using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using St.Common.Core;
using TowerDefense.Core.View;


public class TestConfigViews : MonoBehaviour
{
	[SerializeField]
	VisualizerContainer m_Obj = default;

    public static TestConfigViews Inst { get; private set; }


	public void TestInitView()
    {
		ICoreObjectInstantiate clone = m_Obj.Value.Instantiate();

		//ISliceVisualizer[] views = m_Obj.GetComponents<ISliceVisualizer>();
		//Instantiate()
		//m_Obj.IsPrefab();

	}

	void Awake()
	{
		if (Inst == null)
			Inst = this;
		else
			Destroy(gameObject);
	}

	void OnDestroy()
	{
		if (Inst == this)
			Inst = null;
	}
}
