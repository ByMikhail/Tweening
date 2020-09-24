using UnityEngine;
using AppInGap.Managers;
using AppInGap.Tweening.EasingClips;

namespace AppInGap.Tweening
{
    /// <summary>
    /// Plays tweens
    /// </summary>
    public sealed class Tweener<T> : IUpdatable
    {
        struct DelegateBasedInterpolator<U> : IInterpolator<U>
        {
            private readonly Interpolator<U> m_Interpolator;

            public DelegateBasedInterpolator(Interpolator<U> interpolator)
            {
                m_Interpolator = interpolator;
            }

            public U Interpolate(U from, U to, float interpolant)
            {
                return m_Interpolator(from, to, interpolant);
            }
        }

        #region Private fields

        private bool m_IsPlaying;
        private ValueSetter<T> m_Setter;
        private IInterpolator<T> m_Interpolator;
        private float m_AnimatePhysics;
        private TweenParams<T> m_TweenParams;
        private IEasingClip m_EasingClip;
        private float m_Elapsed;

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

        public Tweener(Interpolator<T> interpolator, bool animatePhysics, ValueSetter<T> setter)
        : this(new DelegateBasedInterpolator<T>(interpolator), animatePhysics, setter) {}

        public void Start(TweenParams<T> tweenParams, IEasingClip easingClip)
        {
            m_TweenParams = tweenParams;
            m_EasingClip = easingClip;
            m_Elapsed = 0f;

            if (!m_IsPlaying)
            {
                Updater.defaultUpdater.AddTarget(this, m_AnimatePhysics == 1f);
            }

            m_IsPlaying = true;
        }

        public void Start(TweenParams<T> tweenParams, float duration)
        {
            Start(tweenParams, new SmoothstepClip(duration));
        }

        public void Stop()
        {
            m_IsPlaying = false;

            Updater.defaultUpdater.RemoveTarget(this);
        }

        #endregion

        #region Private methods

        private bool HasReachedEnd()
        {
            return m_Elapsed >= m_EasingClip.duration;
        }

        private void IncreaseElapsedTime()
        {
            float dt = Time.unscaledDeltaTime;
            float fdx = Time.fixedUnscaledDeltaTime;
            float deltaTime = m_AnimatePhysics * fdx + (1f - m_AnimatePhysics) * dt;

            m_Elapsed += deltaTime;
        }

        private T CalculateTweenState()
        {
            float interpolant = m_EasingClip.ValueAt(m_Elapsed);
            return m_Interpolator.Interpolate(m_TweenParams.from, m_TweenParams.to, interpolant);
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

                T tweenState = CalculateTweenState();
                m_Setter(tweenState);
            }
        }

        #endregion
    }
}