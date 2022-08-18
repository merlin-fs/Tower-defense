using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Windows
{
	public interface IWindowExitHandler
	{
		void SendClose();
	}

	public class WindowExitHandler : MonoBehaviour, IWindowExitHandler
	{
		private void Awake()
		{
			IWindowExitHandlerPool pool = WindowExitHandlerPool.Instance;

			if (pool != null)
				pool.Push(this);
		}

		private void OnDestroy()
		{
			IWindowExitHandlerPool pool = WindowExitHandlerPool.Instance;

			if (pool != null)
				pool.Pop(this);
		}

		void IWindowExitHandler.SendClose()
		{
			if (GetComponent<Window>() != null)
				GetComponent<Window>().CloseWindow();
			else
			{
				PointerEventData pointer = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.pointerClickHandler);
			}
		}
	}
}