using Example.Components;
using LambdaEngine;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.System;

namespace Example.Systems;

public class BlockMovementSystem : EcsSystem {
    EcsQuery _query;
    
    public override void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        base.OnSetup(engine, world);
        
        _query = EcsQuery.Create(World)
            .Include<PositionComponent>()
            .Include<BlockTagComponent>()
            .Build();
    }

    public override void OnStartup() { }

    public override void OnExecute() {
        QueryCollection<PositionComponent> entities = _query.Execute<PositionComponent>();
        
        float halfWidth = WindowManager.WindowWidth * 0.5f;
        float halfHeight = WindowManager.WindowHeight * 0.5f;

        foreach (ComponentRef<PositionComponent> entity in entities.GetComponents()) {
            // Shift position to top-left origin for wrapping
            float x = entity.Item0.Position.X + halfWidth;
            float y = entity.Item0.Position.Y + halfHeight;

            // Wrap around
            x = x % WindowManager.WindowWidth;
            y = y % WindowManager.WindowHeight;

            // Handle negative modulo results
            if (x < 0) x += WindowManager.WindowWidth;
            if (y < 0) y += WindowManager.WindowHeight;

            // Shift back to centered origin
            entity.Item0.Position.X = x - halfWidth;
            entity.Item0.Position.Y = y - halfHeight;
        }
    }
    
    public override void OnShutdown() { }
}