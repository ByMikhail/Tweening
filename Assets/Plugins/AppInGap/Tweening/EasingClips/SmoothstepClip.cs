using UnityEngine;

namespace AppInGap.Tweening.EasingClips
{
    public class SmoothstepClip : IEasingClip
    {
        public float duration { get; private set; }

        public SmoothstepClip(float duration)
        {
            this.duration = duration;
        }

        public float ValueAt(float time)
        {
            return Mathf.SmoothStep(0f, 1f, time / duration);
        }
    }
}