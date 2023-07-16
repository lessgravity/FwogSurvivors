using EngineKit.Mathematics;

namespace FwogSurvivors.Extensions;

internal static class ViewportExtensions
{
    public static Vector4 ToVector4(this Viewport viewport)
    {
        return new Vector4(viewport.X, viewport.Y, viewport.Width, viewport.Height);
    }
}