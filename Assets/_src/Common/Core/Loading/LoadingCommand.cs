using System;

namespace St.Common.Core.Loading
{
    public interface ILoadingCommand
    {
        float GetProgress();
        void Exec(ILoadingManager manager, Action<ILoadingCommand> onComplete);
    }
}