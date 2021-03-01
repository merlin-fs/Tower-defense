using System;
using UnityEngine;

namespace System
{
	[Serializable]
	public abstract class ATypedContainer
	{
		[JetBrains.Annotations.CanBeNull]
		[SerializeField]
		protected MonoBehaviour m_Obj = default;

		public void Validate()
		{
			OnValidate();
		}

		public void SetObject(MonoBehaviour obj)
        {
			m_Obj = obj;
		}

		protected abstract void OnValidate();
	}

	[Serializable]
	public class TypedContainer<T> : ATypedContainer where T : class
	{
		[JetBrains.Annotations.CanBeNull]
		public T Value => m_Obj as T;

		protected override void OnValidate()
		{
			T tmp = m_Obj as T;
			if (tmp == null && m_Obj != null)
				m_Obj = m_Obj.GetComponent<T>() as MonoBehaviour;
			if (Value == null)
				m_Obj = null;
		}
	}
}
