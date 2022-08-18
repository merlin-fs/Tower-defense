using System.Collections.Generic;
using UnityEngine;

namespace Common.UI.Windows
{
	using Singletons;

	public interface IWindowExitHandlerPool
	{
		void Push(IWindowExitHandler window);
		void Pop(IWindowExitHandler window);
	}

	public class WindowExitHandlerPool : MonoSingleton<WindowExitHandlerPool>, IWindowExitHandlerPool
	{
		public static IWindowExitHandlerPool Instance => Inst;

		List<IWindowExitHandler> m_WidowStack = new List<IWindowExitHandler>();

		protected virtual void OnTryTerminate() { }

		public void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Escape))
				return;

			if (m_WidowStack.Count > 0)
				m_WidowStack[m_WidowStack.Count - 1].SendClose();
			else
			{
				if (WindowManager.Instance.IsBusy())
					return;

				OnTryTerminate();
			}
		}

		void IWindowExitHandlerPool.Push(IWindowExitHandler window)
		{ 
			m_WidowStack.Add(window); 
		}

		void IWindowExitHandlerPool.Pop(IWindowExitHandler window)
		{ 
			m_WidowStack.Remove(window); 
		}
	}
}