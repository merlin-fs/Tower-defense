using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Common.Core.Loading
{
    using Progress;

    [ExecuteAlways]
    public abstract class LoadingManager: ILoadingManager
    {
        [SerializeField]
        private List<CommandItem> m_Commands = new List<CommandItem>();

        public IDIContext Context { get; private set; }

        private MultiProgress m_Progress;
        
        private bool m_Complete;

        protected abstract void DoBindCommand();

        protected void Add(ILoadingCommand command)
        {
            m_Commands.Add(new CommandItem() { Command = command });
        }

        public LoadingManager()
        {
            m_Commands.Clear();
            DoBindCommand();
        }

        #region ILoading
        void IInjectionInitable.Init(IDIContext context)
        {
            Context = context;
            SynchronizationContext = SynchronizationContext.Current;
        }

        public SynchronizationContext SynchronizationContext { get; private set; }
        string ILoadingManager.Text { get; set; }
        
        IProgress ILoadingManager.Progress => m_Progress;
        
        bool ILoadingManager.Complete => m_Complete;
        
        void ILoadingManager.Start()
        {
            m_Complete = false;
            m_Progress = new MultiProgress(m_Commands.Select((iter) => iter.Command).ToArray());
            System.Threading.EventWaitHandle @event = new System.Threading.AutoResetEvent(true);
            var context = SynchronizationContext;

            Task.Run(
                () =>
                {
                    //копия для возможности перезагрузки игры
                    List<CommandItem> commands = new List<CommandItem>(m_Commands);
                    Prepare();

                    int count = commands.Count;

                    //пока есть комманды
                    while (count > 0)
                    {
                        //получаем комманды, для которых нет зависимостей
                        ILoadingCommand command = null;
                        while ((command = GetNextCommand()) != null)
                        {
                            //запускаем выполнение комманды
                            context.Post(
                                (o) =>
                                {
                                    var cmd = o as ILoadingCommand;
                                    cmd.Exec(this,
                                        (cmd) =>
                                        {
                                            //удаляем из зависимостей
                                            RemoveDependency(cmd);
                                            @event.Set();
                                            count--;
                                        });
                                }, command);
                        }
                        
                        //ожидание выполнения комманд
                        while (!@event.WaitOne(15) && count > 0)
                        {
                            foreach (var iter in m_Commands)
                            {
                                context.Post(
                                    (o) =>
                                    {
                                        var cmd = o as ILoadingCommand;
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
                if (m_CommandsIndex == null || m_CommandsIndex.Length == 0)
                    return;
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