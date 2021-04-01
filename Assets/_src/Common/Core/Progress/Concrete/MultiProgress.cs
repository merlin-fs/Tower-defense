using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace St.Common.Core.Progress
{
    public class MultiProgress : IProgressWritable
    {
        private bool m_Done;
        private Dictionary<object, float> m_Progress = new Dictionary<object, float>();
        private event IProgress.OnProgressChange m_OnProgressChange;
        private IProgressWritable Self => this;

        public MultiProgress(params object[] objects)
        {
            foreach(object iter in objects)
                m_Progress.Add(iter, 0);
        }


        public void SetProgress(object obj, float value)
        {
            if (m_Progress.TryGetValue(obj, out float objValue))
            {
                value = Mathf.Clamp(value, 0, 1);
                if (objValue != value)
                {
                    m_Progress[obj] = value;
                    m_OnProgressChange?.Invoke(Self.Value);
                }
            }
        }

        public void SetDone()
        {
            Self.SetDone();
        }

        #region IProgress
        float IProgressWritable.SetDone()
        {
            m_Done = true;
            return Self.Value;
        }

        float IProgress.Value
        {
            get
            {
                if (m_Done) return 1;
                if (m_Progress.Count == 0) return 0;

                return m_Progress.Values.Sum() / m_Progress.Count;
            }
        }

        float IProgressWritable.SetProgress(float value)
        {
            return Self.Value;
        }

        event IProgress.OnProgressChange IProgress.OnChange
        {
            add => m_OnProgressChange += value;
            remove => m_OnProgressChange -= value;
        }
        #endregion
    }
}
