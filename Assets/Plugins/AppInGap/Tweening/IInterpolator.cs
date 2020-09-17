namespace AppInGap.Tweening
{
    public interface IInterpolator<T>
    {
        T Interpolate(T from, T to, float interpolant);
    }
}