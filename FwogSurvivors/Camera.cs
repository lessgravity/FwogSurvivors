using EngineKit.Mathematics;

namespace FwogSurvivors;

internal class Camera
{
    public Vector3 Position;

    public Matrix ViewMatrix;

    public Matrix ProjectionMatrix;

    public Viewport Viewport;

    public Camera(Viewport viewport)
    {
        Viewport = viewport;
        Position = Vector3.Zero;
    }

    public void UpdateMatrices()
    {
        ProjectionMatrix = Matrix.OrthoRH(Viewport.Width, Viewport.Height, -1, 1);
        ViewMatrix = Matrix.LookAtRH(Position, Position - Vector3.UnitZ, Vector3.Up);
    }
}