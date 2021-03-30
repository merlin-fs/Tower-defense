using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.Core;


namespace Game.Loading.View
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField]
        private Image m_Progress = default;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            m_Progress.fillAmount = Core.Inst.Loading?.Progress.Value ?? 0;
        }
    }
}