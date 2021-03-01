using System;
using UnityEngine;
using UnityEditor;

namespace Words.Main.TabloLogicEditor
{
	using Words.Main;

	public class TestConfigViewsEditor : EditorWindow
	{
		[MenuItem("Cheats/TestConfigViews")]
		public static void ShowNavigator()
		{
			EditorWindow.GetWindow<TestConfigViewsEditor>();
		}

		void OnGUI()
		{
			if (!EditorApplication.isPlaying)
			{
				GUILayout.Label("Work in runtime...");
				return;
			}

			/*
			overlay = (OverlayType)EditorGUILayout.EnumPopup("Overlay:", overlay);
			inputText = EditorGUILayout.TextField("Value:", inputText);
			if (int.TryParse(inputText, out int value))
				_int = value;
			*/

			EditorGUILayout.Space();

			if (GUILayout.Button("test"))
				TestConfigViews.Inst.TestInitView();
		}

		void Update()
		{ Repaint(); }
	}
}