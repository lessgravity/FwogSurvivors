using System;
using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Mathematics;
using FwogSurvivors.Extensions;
using Serilog;

namespace FwogSurvivors;

internal struct GpuCameraInformation
{
    public Matrix ProjectionMatrix;
    public Matrix ViewMatrix;
    public Vector4 Viewport;
    public Vector4 CameraPosition;
    public Vector4 CameraDirectionAndAspectRatio;     
}

internal class Renderer : IRenderer
{
    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IApplicationContext _applicationContext;

    private IGraphicsPipeline? _graphicsPipeline;
    private IUniformBuffer? _gpuCameraInformationBuffer;
    private GpuCameraInformation _gpuCameraInformation;

    private SwapchainDescriptor _swapchain;

    private IVertexBuffer? _vertexBuffer;
    private IIndexBuffer? _indexBuffer;

    private ITexture? _fwogTexture;

    public Renderer(ILogger logger, IGraphicsContext graphicsContext, IApplicationContext applicationContext)
    {
        _logger = logger.ForContext<Renderer>();
        _graphicsContext = graphicsContext;
        _applicationContext = applicationContext;

        _gpuCameraInformation = new GpuCameraInformation();
    }

    public void Dispose()
    {
        _graphicsPipeline?.Dispose();
        _gpuCameraInformationBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        _fwogTexture?.Dispose();
    }
    
    public bool Load()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Unlit.vs.glsl", "Shaders/Unlit.fs.glsl")
            .WithFaceWinding(FaceWinding.CounterClockwise)
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .Build("BackgroundRenderer"))
            .EnableCulling(CullMode.Back)
            .Build("BackgroundRenderer-GraphicsPipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            return false;
        }

        _graphicsPipeline = graphicsPipelineResult.Value;

        _swapchain = new SwapchainDescriptorBuilder()
            .WithViewport(_applicationContext.WindowSize.X, _applicationContext.WindowSize.Y)
            .ClearColor(Color.DarkGoldenrod)
            .ClearDepth()
            .EnableSrgb()
            .Build();

        _gpuCameraInformationBuffer = _graphicsContext.CreateUniformBuffer<GpuCameraInformation>("CameraInformation");
        _gpuCameraInformationBuffer.AllocateStorage(_gpuCameraInformation, StorageAllocationFlags.Dynamic);

        _fwogTexture = _graphicsContext.CreateTextureFromFile("Tiles/Fwog.png", Format.R8G8B8A8Srgb);

        var vertices = new[]
        {
            new VertexPositionColorUv(new Vector3(64.0f, 64.0f, 0.0f), Vector3.One, new Vector2(1, 1)),
            new VertexPositionColorUv(new Vector3(0.0f, 64.0f, 0.0f), Vector3.One, new Vector2(0, 1)),
            new VertexPositionColorUv(new Vector3(0.0f, 0.0f, 0.0f), Vector3.One, new Vector2(0, 0)),
            new VertexPositionColorUv(new Vector3(64.0f, 0.0f, 0.0f), Vector3.One, new Vector2(1, 0)),
        };

        var indices = new uint[]
        {
            0, 1, 2, 2, 3, 0
        };

        _vertexBuffer = _graphicsContext.CreateVertexBuffer<VertexPositionColorUv>("Vertices");
        _vertexBuffer.AllocateStorage(vertices, StorageAllocationFlags.None);
        _indexBuffer = _graphicsContext.CreateIndexBuffer<uint>("Indices");
        _indexBuffer.AllocateStorage(indices, StorageAllocationFlags.None);

        return true;
    }

    public void RenderWorld(Camera camera)
    {
        UpdateCamera(camera);
        
        _graphicsContext.BindGraphicsPipeline(_graphicsPipeline);
        _graphicsContext.BeginRenderToSwapchain(_swapchain);
        _graphicsPipeline.BindVertexBuffer(_vertexBuffer, 0, 0);
        _graphicsPipeline.BindIndexBuffer(_indexBuffer);
        _graphicsPipeline.BindUniformBuffer(_gpuCameraInformationBuffer, 0);
        _graphicsPipeline.BindTexture(_fwogTexture, 0);
        _graphicsPipeline.DrawElements(6);
        _graphicsContext.EndRender();        
    }

    private void UpdateCamera(Camera camera)
    {
        camera.UpdateMatrices();
        _gpuCameraInformation.ProjectionMatrix = camera.ProjectionMatrix;
        _gpuCameraInformation.ViewMatrix = camera.ViewMatrix;
        _gpuCameraInformation.Viewport = camera.Viewport.ToVector4();
        _gpuCameraInformation.CameraPosition = new Vector4(camera.Position, 0.0f);
        _gpuCameraInformation.CameraDirectionAndAspectRatio = Vector4.Zero;
        _gpuCameraInformationBuffer.Update(_gpuCameraInformation);
    }
}