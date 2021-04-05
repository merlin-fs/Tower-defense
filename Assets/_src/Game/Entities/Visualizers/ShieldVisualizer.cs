using UnityEngine;

namespace Game.Entities.View
{
    /// <summary>
    /// Class to visualizer the health of a damageable
    /// </summary>
    public class ShieldVisualizer : BaseVisualizer<Shield>
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

        /// <summary>
        /// Показывать или нет прогресс, когда он полный
        /// </summary>
        public bool showWhenFull;
        /// <summary>
        /// Камера для которой будет показан прогресс (поворот всегда к ней)
        /// </summary>
        protected Transform m_CameraToFace;

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

        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            if (slice is IProperty prop)
                UpdateValue(prop);
        }

        /// <summary>
        /// Updates the visualization of the health
        /// </summary>
        /// <param name="value">IProperty</param>
        private void UpdateValue(IProperty value)
        {
            m_Bar?.material.SetFloat("_Cutoff", value.Normalize);
            SetVisible(showWhenFull || value.Normalize < 1.0f);
        }

        private void SetVisible(bool visible)
        {
            //gameObject.SetActive(visible);
        }
    }
}