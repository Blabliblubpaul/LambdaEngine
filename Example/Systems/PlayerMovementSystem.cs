using System.Numerics;
using Example.Components;
using LambdaEngine;
using LambdaEngine.Components;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.System;
using LambdaEngine.Types;

namespace Example.Systems;

public class PlayerMovementSystem : EcsSystem {
    private const float speed = 300;
    
    private EcsQuery _query;
    
    public override void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        base.OnSetup(engine, world);

        _query = EcsQuery.Create(World)
            .Include<VelocityComponent>()
            .Include<PlayerTagComponent>()
            .Build();
    }
    
    public override void OnStartup() { }
    
    public override void OnExecute() {
        Vector2 direction = Vector2.Zero;

        if (Input.GetKeyDown(Keys.A)) {
            direction.X = -1;
        }
        else if (Input.GetKeyDown(Keys.D)) {
            direction.X = 1;
        }
        
        if (Input.GetKeyDown(Keys.W)) {
            direction.Y = 1;
        }
        else if (Input.GetKeyDown(Keys.S)) {
            direction.Y = -1;
        }

        if (direction != Vector2.Zero) {
            direction = Vector2.Normalize(direction);
        }
        
        QueryCollection<VelocityComponent> entities = _query.Execute<VelocityComponent>();

        foreach (ComponentRef<VelocityComponent> entity in entities.GetComponents()) {
            entity.Item0.Velocity = direction * speed;
        }
    }
    
    public override void OnShutdown() { }
}