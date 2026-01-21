using Example.Components;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.System;

namespace Example.Systems;

public class BulletDestroySystem : EcsSystem {
    EcsQuery _query;
    
    public override void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        base.OnSetup(engine, world);
        
        _query = EcsQuery.Create(World)
            .Include<PositionComponent>()
            .Include<BulletTagComponent>()
            .Build();
    }
    
    public override void OnStartup() { }
    
    public override void OnExecute() {
        QueryCollection<PositionComponent> result = _query.Execute<PositionComponent>();
        
        // TODO: This is very fragile sicne a Camera exists.
        foreach (ComponentRef<PositionComponent> entity in result.GetComponents()) {
            if (entity.Item0.Position.X is < -500 or > 500 || entity.Item0.Position.Y is < -550 or > 500) {
                World.MarkEntityForDestruction(entity.Id);
            }
        }
    }
    
    public override void OnShutdown() { }
}