using LambdaEngine;
using LambdaEngine.Components;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.Debug;
using LambdaEngine.System;

namespace Example.Systems;

public class MovementSystem : EcsSystem {
    EcsQuery _query;

    private float speed = 60;
    
    public override void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        base.OnSetup(engine, world);
        
        _query = EcsQuery.Create(World).Include<PositionComponent>().Include<VelocityComponent>().Build();
        
        LDebug.Log("UpdateSystem Setup");
    }
    
    public override void OnExecute() {
        QueryCollection<PositionComponent, VelocityComponent> entities = _query.Execute<PositionComponent, VelocityComponent>();

        foreach (ComponentRef<PositionComponent, VelocityComponent> entity in entities.GetComponents()) {
            entity.Item0.Position += entity.Item1.Velocity * GameLoop.DeltaTime;
        }
    }
    
    public override void OnShutdown() {
        LDebug.Log("UpdateSystem Shutdown");
    }

    public override void OnStartup() {
        LDebug.Log("UpdateSystem Startup");
    }
}