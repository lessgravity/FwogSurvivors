using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using Microsoft.Extensions.Options;
using Serilog;

namespace FwogSurvivors;

internal sealed class FwogSurvivorsApplication : GraphicsApplication
{
    private readonly IApplicationContext _applicationContext;
    private readonly IRenderer _renderer;

    private Camera? _camera;

    public FwogSurvivorsApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        ILimits limits,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IRenderer renderer)
        : base(logger,
            windowSettings,
            contextSettings,
            applicationContext,
            metrics,
            limits,
            inputProvider,
            graphicsContext,
            uiRenderer)
    {
        _applicationContext = applicationContext;
        _renderer = renderer;
    }

    protected override bool Initialize()
    {
        return base.Initialize();
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            return false;
        }
        
        _camera = new Camera(new Viewport(0, 0, _applicationContext.WindowSize.X, _applicationContext.WindowSize.Y));
        
        if (!_renderer.Load())
        {
            return false;
        }

        return true;
    }

    protected override void Render(float deltaTime)
    {
        _renderer.RenderWorld(_camera);
    }

    protected override void Update(float deltaTime)
    {
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
        base.Update(deltaTime);
    }

    protected override void Unload()
    {
        base.Unload();
    }
}