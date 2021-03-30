using System;
using UnityEngine;
using UnityEngine.SceneManagement;

using Common.Core.Loading;

namespace Game.Loading
{
    public class LoadScene : LoadingManager.Command
    {
        [SerializeField, SceneSelectorField]
        private string m_Scene;

        AsyncOperation m_AsyncOperation = null;

        protected override void Exec(ILoadingManager loading, Action<ILoadingCommand> onComplete)
        {
            m_AsyncOperation = SceneManager.LoadSceneAsync(m_Scene);
            m_AsyncOperation.completed += OnCompleted;

            void OnCompleted(AsyncOperation op)
            {
                op.completed -= OnCompleted;
                onComplete.Invoke(this);
            }

        }

        protected override float GetProgress()
        {
            return m_AsyncOperation?.progress ?? 0;
        }
    }
}
