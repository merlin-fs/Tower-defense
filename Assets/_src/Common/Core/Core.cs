using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Core
{
    using Loading;

    public interface ICore
    {
        ILoadingManager Loading { get; }

        event Action OnReloadGame;
    }

    public class Core : MonoBehaviour, ICore
    {
        private static Core m_Inst;
        private static readonly IDIContextContainer m_DI = new DIContextContainer();

        [SerializeReference, SubclassSelector(typeof(ILoadingManager))]
        private ILoadingManager m_Loading;

        public static ICore Inst => m_Inst;

        private void Awake()
        {
            if (m_Inst == null)
                m_Inst = this;
            else
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (m_Inst == this)
                m_Inst = null;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            StartGame();
        }

        void OnApplicationQuit()
        {
            Destroy(gameObject);
        }


        public static void Bind<T>(T instance, object id = null) where T : class => m_DI.Bind(instance, id);
        public static void UnBind<T>(T instance, object id = null) where T : class => m_DI.UnBind(instance, id);
        public static void UnBindAll() => m_DI.UnBindAll();
        public static T Get<T>(object id = null) where T : class => m_DI.TryGet<T>(id);

        public static void ReloadGame()
        {
            m_Inst?.OnReloadGame?.Invoke();
            UnBindAll();
            m_Inst?.StartGame();
            SceneManager.LoadSceneAsync(0);
        }

        public static void QuitGame()
        {
            Application.Quit();
        }

        public static void PauseGame()
        {
            Time.timeScale = 0;
        }

        public static void ResumeGame()
        {
            Time.timeScale = 1;
        }
        

        private void StartGame()
        {
            m_Loading?.Start();
        }

        #region ICore
        ILoadingManager ICore.Loading => m_Loading;

        public event Action OnReloadGame;
        #endregion
    }
}