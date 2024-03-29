﻿using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using St.Common.Core.Loading;

namespace Game.Loading
{
    public class TestCommand1 : LoadingManager.Command
    {
        private long m_Tick;
        private const int SLEEP = 2000;

        protected override void Exec(ILoadingManager loading, Action<ILoadingCommand> onComplete)
        {
            Task.Run(
                () =>
                {
                    m_Tick = DateTime.Now.Ticks;
                    Thread.Sleep(SLEEP);
                    OnCompleted();
                });

            void OnCompleted()
            {
                onComplete.Invoke(this);
            }
        }

        protected override float GetProgress()
        {
            var time = new TimeSpan(DateTime.Now.Ticks - m_Tick);
            return (float)time.TotalMilliseconds / SLEEP;
        }
    }
}
