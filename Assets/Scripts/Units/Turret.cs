using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using St.Common.Core;

namespace TowerDefense
{
    using Core;
    using Targetting;

    public interface ITurret: ICoreGameObject
    {
        void AnimTurret(IUnit targetable);
    }

    public class Turret : MonoBehaviour, ITurret
    {
        public float idleRotationSpeed = 39f;
        public float idleCorrectionTime = 2.0f;
        public float idleWaitTime = 2.0f;
        public Vector2 turretXRotationRange = new Vector2(0, 359);


        private float m_WaitTimer = 0.0f;
        private float m_CurrentRotationSpeed;
        private float m_XRotationCorrectionTime;

        GameObject ICoreGameObject.GameObject => gameObject;

        void ITurret.AnimTurret(IUnit targetable)
        {

            if (targetable == null) // do idle rotation
            {
                if (m_WaitTimer > 0)
                {
                    m_WaitTimer -= Time.deltaTime;
                    if (m_WaitTimer <= 0)
                    {
                        m_CurrentRotationSpeed = (Random.value * 2 - 1) * idleRotationSpeed;
                    }
                }
                else
                {
                    Vector3 euler = transform.rotation.eulerAngles;
                    euler.x = Mathf.Lerp(Wrap180(euler.x), 0, m_XRotationCorrectionTime);
                    m_XRotationCorrectionTime = Mathf.Clamp01((m_XRotationCorrectionTime + Time.deltaTime) / idleCorrectionTime);
                    euler.y += m_CurrentRotationSpeed * Time.deltaTime;

                    transform.eulerAngles = euler;
                }
            }
			else
			{
				m_WaitTimer = idleWaitTime;

				Vector3 targetPosition = targetable.GameObject.transform.position;
				//if (onlyYTurretRotation)
				{
					targetPosition.y = transform.position.y;
				}
				Vector3 direction = targetPosition - transform.position;
				Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
				Vector3 lookEuler = look.eulerAngles;
				// We need to convert the rotation to a -180/180 wrap so that we can clamp the angle with a min/max
				float x = Wrap180(lookEuler.x);
				lookEuler.x = Mathf.Clamp(x, turretXRotationRange.x, turretXRotationRange.y);
				look.eulerAngles = lookEuler;
                transform.rotation = look;
			}
        }

        static float Wrap180(float angle)
        {
            angle %= 360;
            if (angle < -180)
                angle += 360;
            else if (angle > 180)
                angle -= 360;
            return angle;
        }
    }
}