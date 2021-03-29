using System;

namespace Common.Core.Loading
{
    public interface ILoadingCommand
    {
        void Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete);
    }
}