using System;
using System.Threading;

namespace Common.Core.Loading
{
    public interface ILoadingManager : IInjectionInitable
    {
        IDIContext Context { get; }
        SynchronizationContext SynchronizationContext { get; }
        IProgress Progress { get; }
        string Text { get; set; }
        bool Complete { get; }
        void Start();
        event Action OnLoadComplete;
    }
}
