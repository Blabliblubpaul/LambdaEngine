using LambdaEngine.Core;
using LambdaEngine.Interfaces;

namespace LambdaEngine.System;

public abstract class EcsSystem : ISystem {
    protected EcsWorld World { get; private set; }

    public virtual void OnSetup(LambdaEngine engine, EcsWorld world) {
        World = world;
    }
    
    public abstract void OnStartup();
    public abstract void OnExecute();
    public abstract void OnShutdown();
}