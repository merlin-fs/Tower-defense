using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI.Windows
{
    public class DefaultAnim : MonoBehaviour, IAdditionalBehaviour
    {
        public void Play(IWindow.AnimationMode mode, float time)
        {
            switch (mode)
            {
                case IWindow.AnimationMode.Hide:
                case IWindow.AnimationMode.Close:
                    WindowManager.Instance.SetDarkVisible(false);
                    break;
                case IWindow.AnimationMode.Open:
                case IWindow.AnimationMode.Show:
                    WindowManager.Instance.SetDarkVisible(true);
                    break;
            }
        }
    }
}
