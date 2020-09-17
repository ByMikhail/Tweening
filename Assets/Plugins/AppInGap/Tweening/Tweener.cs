using UnityEngine;
using AppInGap.Managers;

namespace AppInGap.Tweening
{
    public sealed class Tweener<T> : IUpdatable
    {
        #region Private fields
        
        private bool m_IsPlaying;
        private ValueSetter<T> m_Setter;
        private IInterpolator<T> m_Interpolator;
        private float m_AnimatePhysics;
        private TweenParams<T> m_TweenParams;
        private float m_Elapsed;
        private float m_IsLooped;
        private static Updater m_Updater;

        #endregion

        #region Public methods

        public Tweener(IInterpolator<T> interpolator, bool animatePhysics, ValueSetter<T> setter)
        {
            Debug.AssertFormat(setter != null, "[Tweener<{0}>] Attempt to create tweener with null setter", typeof(T));
            Debug.AssertFormat(interpolator != null, "[Tweener<{0}>] Attempt to create tweener with null interpolator", typeof(T));

            m_Setter = setter;
            m_Interpolator = interpolator;
            m_AnimatePhysics = animatePhysics ? 1f : 0f;
        }

        public void Start(TweenParams<T> tweenParams)
        {
            m_TweenParams = tweenParams;
            m_Elapsed = 0f;

            if (!m_IsPlaying)
            {
                Updater.defaultUpdater.AddTarget(this, m_AnimatePhysics == 1f);
            }

            m_IsPlaying = true;
        }

        public void Stop()
        {
            m_IsPlaying = false;
            m_IsLooped = 0f;

            Updater.defaultUpdater.RemoveTarget(this);
        }

        #endregion

        #region Private methods

        private bool HasReachedEnd()
        {
            return m_Elapsed >= m_TweenParams.easingClip.duration && m_IsLooped == 0f;
        }

        private void IncreaseElapsedTime()
        {
            float dt = Time.unscaledDeltaTime;
            float fdx = Time.fixedUnscaledDeltaTime;
            float deltaTime = m_AnimatePhysics * fdx + (1f - m_AnimatePhysics) * dt;

            m_Elapsed += deltaTime;
            float looped = m_Elapsed % m_TweenParams.easingClip.duration;
            float unlooped = Mathf.Min(m_Elapsed, m_TweenParams.easingClip.duration);
            m_Elapsed = m_IsLooped * looped + (1f - m_IsLooped) * unlooped;
        }

        private void SendValue()
        {
            float interpolant = m_TweenParams.easingClip.ValueAt(m_Elapsed);
            T interpolatedValue = m_Interpolator.Interpolate(m_TweenParams.from, m_TweenParams.to, interpolant);
            m_Setter(interpolatedValue);
        }

        #endregion

        #region IUpdatable implementation

        void IUpdatable.DoUpdate()
        {
            if (HasReachedEnd())
            {
                Stop();
            }
            else
            {
                IncreaseElapsedTime();
                SendValue();
            }
        }

        #endregion
    }
}