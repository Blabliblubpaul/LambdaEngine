using LambdaEngine.Core;

namespace LambdaEngine.Interfaces;

// EcsSystem as abstract class
public interface ISystem {
    public void OnSetup(LambdaEngine engine, EcsWorld world);
    
    public void OnStartup();
    
    public void OnExecute();
    
    public void OnShutdown();
}