using UnityEngine;

namespace Game.Entities.View
{
    /// <summary>
    /// Class to visualizer the health of a damageable
    /// </summary>
    public class HealthVisualizer : BaseVisualizer<Health>
    {
        /// <summary>
        /// Основной объект-контейнер, в котором находится прогресс
        /// </summary>
        public Transform rootObject;
        /// <summary>
        /// Объект - полоса жизни
        /// </summary>
        [SerializeField]
        private MeshRenderer m_Bar;

        [SerializeField]
        private TMPro.TMP_Text m_Value;

        /// <summary>
        /// Показывать или нет прогресс, когда он полный
        /// </summary>
        public bool showWhenFull;
        /// <summary>
        /// Камера для которой будет показан прогресс (поворот всегда к ней)
        /// </summary>
        protected Transform m_CameraToFace;

        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            if (slice is IProperty prop)
                UpdateHealth(prop);
        }

        /// <summary>
        /// Updates the visualization of the health
        /// </summary>
        /// <param name="value">IProperty</param>
        public void UpdateHealth(IProperty value)
        {
            if (m_Value != null)
                m_Value.text = $"{Mathf.Ceil(value.Value)}";
            m_Bar?.material.SetFloat("_Cutoff", value.Normalize);
            SetVisible(showWhenFull || value.Normalize < 1.0f);
        }

        /// <summary>
        /// Sets the visibility status of this visualiser
        /// </summary>
        public void SetVisible(bool visible)
        {
            //gameObject.SetActive(visible);
        }

        /// <summary>
        /// Turns us to face the camera
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            Vector3 direction = m_CameraToFace.transform.forward;
            rootObject.forward = -direction;
        }

        /// <summary>
        /// Caches the main camera
        /// </summary>
        private void Start()
        {
            m_CameraToFace = UnityEngine.Camera.main.transform;
        }
    }
}