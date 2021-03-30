using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Common.Core.Loading
{
    using Progress;

    public interface ILoadingManager
    {
        IProgress Progress { get; }
        string Text { get; set; }
        bool Complete { get; }
        void Start();
        event Action OnLoadComplete;
    }

    public abstract class LoadingManager: ILoadingManager
    {
        [SerializeField]
        private List<CommandItem> m_Commands = new List<CommandItem>();

        private MultiProgress m_Progress;
        private bool m_Complete;

        #region ILoading
        string ILoadingManager.Text { get; set; }
        IProgress ILoadingManager.Progress => m_Progress;
        bool ILoadingManager.Complete => m_Complete;
        void ILoadingManager.Start()
        {
            m_Complete = false;

            m_Progress = new MultiProgress(m_Commands.Select((iter) => iter.Command).ToArray());

            System.Threading.EventWaitHandle @event = new System.Threading.AutoResetEvent(true);
            var context = System.Threading.SynchronizationContext.Current;

            Task.Run(
                () =>
                {
                    //копия для возможности перезагрузки игры
                    List<CommandItem> commands = new List<CommandItem>(m_Commands);
                    Prepare();

                    //пока есть комманды
                    while (commands.Count > 0)
                    {
                        //получаем комманды, для которых нет зависимостей
                        ILoadingCommand cmd = null;
                        while ((cmd = GetNextCommand()) != null)
                        {
                            //запускаем выполнение комманды
                            context.Post(
                                (postCommand) =>
                                {
                                    ((ILoadingCommand)postCommand).Exec(this,
                                        (cmd) =>
                                        {
                                            //удаляем из зависимостей
                                            RemoveDependency(cmd);
                                            @event.Set();
                                        });
                                }, cmd);
                        }
                        
                        //ожидание выполнения комманд
                        while (!@event.WaitOne(15))
                        {
                            foreach (var iter in m_Commands)
                            {
                                context.Post(
                                    (postCommand) =>
                                    {
                                        cmd = postCommand as ILoadingCommand;
                                        m_Progress.SetProgress(cmd, cmd.GetProgress());
                                    }, iter.Command);
                            }
                        }
                    }

                    //финализация прогресса
                    context.Post(
                        (state) =>
                        {
                            m_Progress.SetDone();
                            m_Complete = true;
                            m_OnLoadComplete?.Invoke();
                        }, null);


                    ILoadingCommand GetNextCommand()
                    {
                        var item = commands
                            .FirstOrDefault((iter) => !iter.Dependency.HasDependency);
                        if (item != null)
                            commands.Remove(item);
                        return item?.Command ?? null;
                    }

                    void RemoveDependency(ILoadingCommand command)
                    {
                        foreach (var iter in commands)
                            iter.Dependency.Remove(command);
                    }

                    void Prepare()
                    {
                        foreach (var iter in commands)
                            iter.Dependency.Rebuild(this);
                    }
                });
        }

        private event Action m_OnLoadComplete;

        event Action ILoadingManager.OnLoadComplete
        {
            add 
            {
                m_OnLoadComplete += value;
                if (m_Complete)
                    m_OnLoadComplete?.Invoke();
            }
            remove => m_OnLoadComplete -= value;
        }
        #endregion

        [Serializable]
        public struct Dependency
        {
            [SerializeField]
            private int[] m_CommandsIndex;

            private HashSet<ILoadingCommand> m_Commands;
            public bool HasDependency => m_Commands?.Count > 0;

            public void Rebuild(LoadingManager manager)
            {
                m_Commands = new HashSet<ILoadingCommand>();
                foreach (int iter in m_CommandsIndex)
                    Add(manager.m_Commands[iter].Command);
            }

            public void Add(ILoadingCommand command) 
            {
                m_Commands?.Add(command);
            }
            public void Remove(ILoadingCommand command) 
            {
                m_Commands?.Remove(command);
            }
        }

        [Serializable]
        private class CommandItem
        {
            [SerializeReference, SubclassSelector(typeof(ILoadingCommand))]
            public ILoadingCommand Command;
            [SerializeField]
            public Dependency Dependency = new Dependency();
        }

        public abstract class Command: ILoadingCommand
        {
            void ILoadingCommand.Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete) => Exec(manager, onComplete);
            float ILoadingCommand.GetProgress() => GetProgress();

            protected abstract void Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete);
            protected abstract float GetProgress();
        }

    }
}