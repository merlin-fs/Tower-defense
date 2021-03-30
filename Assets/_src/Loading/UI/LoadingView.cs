using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.Core;
using TMPro;


namespace Game.Loading.View
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField]
        private Image m_Progress = default;

        [SerializeField]
        private Image m_Flare = default;

        [SerializeField]
        TMP_Text m_Version = default;

        void Start()
        {
            m_Version.text = $"v.{Application.version}";
        }

        // Update is called once per frame
        void Update()
        {
            float progress = Core.Inst.Loading?.Progress.Value ?? 0;
            m_Progress.fillAmount = progress;
            float x = m_Progress.GetComponent<RectTransform>().rect.width * progress;

            var rect = m_Flare.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(x, rect.anchoredPosition.y);
        }
    }
}