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

        protected override void Exec(ILoadingManager loading, Action<ILoadingCommand> onComplete)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(m_Scene);
            asyncOperation.completed += OnCompleted;
            void OnCompleted(AsyncOperation op)
            {
                op.completed -= OnCompleted;
                onComplete.Invoke(this);
            }
        }
    }
}
