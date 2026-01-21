using LambdaEngine.Core;

namespace LambdaEngine.Interfaces;

public interface IStagelessSystem {
    public void OnSetup(LambdaEngine engine, EcsWorld world);
    
    public void OnStartup();
    
    public void OnShutdown();
}