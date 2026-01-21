using System.Reflection;
using Example.Systems;
using LambdaEngine;
using LambdaEngine.Core.Common;
using LambdaEngine.Physics;
using LambdaEngine.Rendering;
using LambdaEngine.Types;

namespace Example;

class Program {
    static void Main() {
        SystemManager.SystemManagerConfigurator configurator = new();
        configurator
            .RegisterStagelessSystem(TextureManager.Instance, 0)
            .RegisterStagelessSystem(TextureSystem.Instance, 1)
            
            .RegisterStagelessSystem<InitSystem>(2)
            
            .RegisterSystem(Input.Instance, SystemStage.FRAME_START, 0)
            
            // TODO: Add a dedicated event for this
            // .RegisterSystem<CollisionCleanupSystem>(SystemStage.FIXED_UPDATE, 0)
            .RegisterSystem<CollisionSystem>(SystemStage.FIXED_UPDATE, 0)
            
            .RegisterSystem<PlayerMovementSystem>(SystemStage.UPDATE, 0)
            .RegisterSystem<CamControlSystem>(SystemStage.UPDATE, 1)
            .RegisterSystem<PlayerShootSystem>(SystemStage.UPDATE, 2)
            .RegisterSystem<MovementSystem>(SystemStage.UPDATE, 3)
            .RegisterSystem<BlockMovementSystem>(SystemStage.UPDATE, 4)
            .RegisterSystem<BulletSystem>(SystemStage.UPDATE, 5)
            .RegisterSystem<PlayerCollisionSystem>(SystemStage.UPDATE, 6)
            
            .RegisterSystem<BulletDestroySystem>(SystemStage.UPDATE, 100)
            
            .RegisterSystem<RenderSystem>(SystemStage.RENDER, 0)
            .Configure();
        
        WindowManager.SetWindowSize(800, 600);
        WindowManager.BackgroundColor = ColorRgb.Cyan;
        
        LambdaEngine.LambdaEngine engine = new("LambdaEngine Example", "0.0.1", "LambdaEngine");
        engine.Initialize(MemSize.FromMBytes(32), Assembly.GetExecutingAssembly());
        
        engine.Run();
    }
}