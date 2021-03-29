using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Common.Core.Loading
{
    public interface ILoadingManager
    {
        float Progress { get; }
        string Text { get; set; }
        bool Complete { get; }
        void Start();
        event Action OnLoadComplete;
    }


    public abstract class LoadingManager: ILoadingManager
    {
        [SerializeReference, SubclassSelector(typeof(ILoadingCommand))]
        private List<ILoadingCommand> m_Commands = new List<ILoadingCommand>();
        
        private float m_Progress;
        private bool m_Complete;

        #region ILoading
        string ILoadingManager.Text { get; set; }
        float ILoadingManager.Progress => m_Progress;
        bool ILoadingManager.Complete => m_Complete;
        void ILoadingManager.Start()
        {
            m_Complete = false;
            m_Progress = 0f;

            var context = System.Threading.SynchronizationContext.Current;
            Task.Run(
                () =>
                {
                    float max = m_Commands.Count;
                    //копия для возможности перезагрузки игры
                    List<ILoadingCommand> commands = new List<ILoadingCommand>(m_Commands);

                    System.Threading.EventWaitHandle @event = new System.Threading.AutoResetEvent(true);
                    while (commands.Count > 0)
                    {
                        if (@event.WaitOne())
                            NextCommand();
                    }

                    context.Post(
                        (state) =>
                        {
                            m_Complete = true;
                            m_OnLoadComplete?.Invoke();
                        }, null);

                    void NextCommand()
                    {
                        ILoadingCommand command = commands[0];
                        commands.RemoveAt(0);

                        //TODO: нужно переделать на прогрес от самих команд
                        m_Progress = (max - commands.Count) / max;
                        context.Post(
                            (state) =>
                            {
                                try
                                {
                                    command?.Exec(this,
                                        (cmd) =>
                                        {
                                            @event.Set();
                                        });
                                }
                                catch (Exception e)
                                {
                                    UnityEngine.Debug.LogError($"Loading command: exec {e.ToString()}");
                                }
                            }, null);
                    }

                });
        }

        private event Action m_OnLoadComplete;

        event Action ILoadingManager.OnLoadComplete
        {
            add {
                m_OnLoadComplete += value;
                if (m_Complete)
                    m_OnLoadComplete?.Invoke();
            }
            remove => m_OnLoadComplete -= value;
        }
        #endregion

        public abstract class Command: ILoadingCommand
        {
            void ILoadingCommand.Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete) => Exec(manager, onComplete);

            protected abstract void Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete);
        }

    }
}