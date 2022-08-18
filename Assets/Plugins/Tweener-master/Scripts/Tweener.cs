using UnityEngine;
using System.Collections;

public class Tweener
{
    public delegate void CallBackDelegate();
    public delegate float TweenDelegate(float t, float b, float c, float d);

    public Vector3 From { get; private set; } = Vector3.zero;
    public Vector3 To { get; private set; } = Vector3.zero;
    public Vector3 Progression { get; private set; } = Vector3.zero;

    public float ProgressPct { get; private set; } = 0.0f;

    public bool Animating { get; private set; } = false;


    private CallBackDelegate m_Callback = null;
    private TweenDelegate m_Easing = null;
    private float m_TimeElapsed = 0.0f;
    private float m_Duration = 1.0f;

    /// <summary>
    /// Eases from value to value.
    /// </summary>
    /// <param name="from">From.</param>
    /// <param name="to">To.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="easing">Easing.</param>
    public void EaseFromTo(Vector3 from, Vector3 to, float duration = 1f, TweenDelegate easing = null, CallBackDelegate callback = null)
    {
        if (easing == null)
        {
            easing = Easing.Linear;
        }

        m_Easing = easing;
        m_Callback = callback;

        From = from;
        To = to;

        m_Duration = duration;
        m_TimeElapsed = 0f;
        ProgressPct = 0f;
        Animating = true;
    }

    public static float bounceOut(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = -1f, float deltaTime = -1f, float friction = 2f, float accelRate = 0.5f, float hitDamping = 0.9f)
    {
        if (deltaTime < 0f)
            deltaTime = Time.deltaTime;

        float diff = target - current;

        currentVelocity += deltaTime / smoothTime * accelRate * diff;

        currentVelocity *= (1f - deltaTime * friction);

        if (maxSpeed > 0f && maxSpeed < Mathf.Abs(currentVelocity))
            currentVelocity = maxSpeed * Mathf.Sign(currentVelocity);

        float returned = current + currentVelocity;

        bool targetGreater = (target > current);
        float returnPassed = returned - target;
        if ((targetGreater && returnPassed > 0) || !targetGreater && returnPassed < 0)
        { // Start a bounce
            currentVelocity = -currentVelocity * hitDamping;
            returned = current + currentVelocity;
        }

        return returned;
    }

    public void Update(float deltaTime, bool callCallBack = true)
    {
        if (Animating)
        {
            if (m_TimeElapsed < m_Duration)
            {
                if (m_Easing != null)
                {
                    /*
                    Progression = new Vector3()
                    {
                        x = m_Easing.Invoke(m_TimeElapsed, From.x, (To.x - From.x), m_Duration),
                        y = m_Easing.Invoke(m_TimeElapsed, From.y, (To.y - From.y), m_Duration),
                        z = m_Easing.Invoke(m_TimeElapsed, From.z, (To.z - From.z), m_Duration),
                    };
                    */
                    float x = From.x;
                    float y = From.y;
                    float z = From.z;

                    bounceOut(From.x, (To.x - From.x), ref x, m_Duration);
                    bounceOut(From.y, (To.y - From.y), ref y, m_Duration);
                    bounceOut(From.z, (To.z - From.z), ref z, m_Duration);
                    Progression = new Vector3()
                    {
                        x = x,
                        y = y,
                        z = z,
                    };
                    

                    ProgressPct = m_TimeElapsed / m_Duration;

                    m_TimeElapsed += deltaTime;
                }
            }
            else
            {
                Progression = To;

                Animating = false;
                m_TimeElapsed = 0f;
                ProgressPct = 1f;

                if (callCallBack && m_Callback != null)
                {
                    m_Callback.Invoke();
                }
            }
        }
    }

    public void Update(float deltaTime, ref Vector2 whatToTween)
    {
        bool wasAnimating = Animating;
        Update(deltaTime, false);
        whatToTween = Progression;

        if (wasAnimating && !Animating && m_Callback != null)
        {
            m_Callback.Invoke();
        }

    }
}
