using System.Reflection;
using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Core.Common;
using LambdaEngine.Rendering;

namespace LambdaEngine;

public class LambdaEngine {
    private EcsWorld _world;
    
    public EcsWorld World {
        get => _world;
    }
    
    public string AppName { get; }
    public string AppVersion { get; }
    public string AppIdentifier { get; }
    
    public LambdaEngine(string appName, string appVersion, string appIdentifier) {
        AppName = appName;
        AppVersion = appVersion;
        AppIdentifier = appIdentifier;
    }

    public void Initialize(nuint ecsInitBufferSize, params Assembly[] assemblies) {
        // Initialize and start the Debug system first, to allow its usage as soon as possible.
        LDebug.Initialize();
        LDebug.Start();
        
        LDebug.Log("Initializing engine...");

        if (!WindowManager.Initialize(this, AppName, AppVersion, AppIdentifier)) {
            LDebug.Log("Unable to initialize window manager; aborting engine initialization.", LogLevel.FATAL);
            return;
        }
        
        EcsWorld.EcsWorldBuilder worldBuilder = EcsWorld.Create(ecsInitBufferSize);
        worldBuilder.AddAssembly(Assembly.GetExecutingAssembly());

        foreach (Assembly assembly in assemblies) {
            worldBuilder.AddAssembly(assembly);
        } 

        _world = worldBuilder.Build();
        
        SystemManager.Initialize(this);
        SystemManager.SystemSetup(_world);

        // Make those proper systems and register them via the SystemManager
        GameLoop.OnPreDestroy += () => _world.AddDestructionTags();
        GameLoop.OnFrameEnd += () => _world.DestroyMarkedEntities();
    }

    public void Run() {
        WindowManager.CreateWindow(AppName);
        
        // TODO: DO NOT hardcode this, as it is not replaceable anymore
        NewRenderSystem.Instance.InitSdl();
        
        SystemManager.SystemStartup();
        
        GameLoop.StartGameLoop();
    }

    public void Stop() {
        LDebug.Log("Stopping engine...");
        
        GameLoop.StopGameLoop();
        
        SystemManager.SystemShutdown();
        
        WindowManager.DestroyWindow();
    }
    
    ~LambdaEngine() {
        LDebug.Stop();
    }
}