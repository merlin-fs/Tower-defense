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
        public Transform bar;
        /// <summary>
        /// The object whose X-scale we change to increase the health bar background. Should have a default uniform scale
        /// </summary>
        public Transform backgroundBar;
        /// <summary>
        /// Показывать или нет прогресс, когда он полный
        /// </summary>
        public bool showWhenFull;
        /// <summary>
        /// Камера для которой будет показан прогресс (поворот всегда к ней)
        /// </summary>
        protected Transform m_CameraToFace;

        /// <summary>
        /// Damageable whose health is visualized
        /// </summary>
        //protected Damageable m_Damageable;

        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            if (slice is IProperty prop)
                UpdateHealth(prop.Value);
        }
        /// <summary>
        /// Updates the visualization of the health
        /// </summary>
        /// <param name="normalizedHealth">Normalized health value</param>
        public void UpdateHealth(float normalizedHealth)
        {
            Vector3 scale = Vector3.one;

            if (bar != null)
            {
                scale.x = normalizedHealth;
                bar.transform.localScale = scale;
            }

            if (backgroundBar != null)
            {
                scale.x = 1 - normalizedHealth;
                backgroundBar.transform.localScale = scale;
            }

            SetVisible(showWhenFull || normalizedHealth < 1.0f);
        }

        /// <summary>
        /// Sets the visibility status of this visualiser
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        /// <summary>
        /// Assigns the damageable, subscribing to the damaged event
        /// </summary>
        /// <param name="damageable">Damageable to assign</param>
        public void AssignDamageable(/*Damageable damageable*/)
        {
            /*
			if (m_Damageable != null)
			{
				m_Damageable.healthChanged -= OnHealthChanged;
			}
			m_Damageable = damageable;
			m_Damageable.healthChanged += OnHealthChanged;
            */
        }

        /// <summary>
        /// Turns us to face the camera
        /// </summary>
        protected virtual void Update()
        {
            Vector3 direction = m_CameraToFace.transform.forward;
            rootObject.forward = -direction;
        }

        /// <summary>
        /// Assigns a damageable if damageableBehaviour is populated
        /// </summary>
        protected virtual void Awake()
        {
            /*
			if (damageableBehaviour != null)
			{
				AssignDamageable(damageableBehaviour.configuration);
			}
            */
        }

        /// <summary>
        /// Caches the main camera
        /// </summary>
        protected virtual void Start()
        {
            m_CameraToFace = UnityEngine.Camera.main.transform;
        }

        void OnHealthChanged(/*HealthChangeInfo healthChangeInfo*/)
        {
            UpdateHealth(0/*m_Damageable.normalisedHealth*/);
        }
    }
}