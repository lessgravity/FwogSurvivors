using System;

namespace FwogSurvivors;

internal interface IRenderer : IDisposable
{
    bool Load();
    
    void RenderWorld(Camera camera);
}