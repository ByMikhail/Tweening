namespace AppInGap.Tweening
{
    public interface IEasingClip
    {
        float duration { get; }
        float ValueAt(float time);
    }
}