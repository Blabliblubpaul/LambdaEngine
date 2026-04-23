using LambdaEngine.Core;

namespace LambdaEngine.Rendering;

public interface IRenderCommandCollector {
    public void Setup(NewRenderSystem renderSystem, EcsWorld world);
    
    public void Execute();
}