using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Root;
    [SerializeField]
    private Image m_Progress;

    private float3 m_Position;
    private float m_Value;
    private Canvas m_Canvas;
    private bool m_Destroing = false;
    private bool m_Initialize = false;

    private void Awake()
    {
        m_Root.SetActive(false);
    }

    void Start()
    {
        m_Canvas = GetComponentInParent<Canvas>();
    }

    public void SetPosition(float3 value)
    {
        m_Position = value;
        m_Initialize = true;
    }

    public void SetValue(float value)
    {
        m_Value = value;
    }

    public void SetDestroy()
    {
        m_Destroing = true;
    }

    private void Update()
    {
        if (m_Destroing)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        if (m_Initialize && !m_Root.activeSelf)
            m_Root.SetActive(true);

        float3 value = Camera.main.WorldToScreenPoint(m_Position);
        value.z = transform.position.z;
        value *= m_Canvas.transform.localScale;
        transform.position = value;
        m_Progress.fillAmount = m_Value;
    }
}
