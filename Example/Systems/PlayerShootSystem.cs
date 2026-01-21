using System.Numerics;
using Example.Components;
using LambdaEngine;
using LambdaEngine.Components;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Components.Rendering.PrimitiveComponents;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Archetypes;
using LambdaEngine.Physics;
using LambdaEngine.Rendering;
using LambdaEngine.System;
using LambdaEngine.Types;

namespace Example.Systems;

public class PlayerShootSystem : EcsSystem {
    private const float bulletSpeed = 1000;

    private ArchetypeHandle _bulletArchetype;
    
    private int _player;
    
    public override void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        base.OnSetup(engine, world);

        _bulletArchetype = World.GetOrCreateArchetype(typeof(PositionComponent), typeof(VelocityComponent), typeof(ColliderComponent),
            typeof(ScaleComponent), typeof(RectPrimitiveComponent), typeof(ColorComponent), typeof(BulletTagComponent));
    }

    public override void OnStartup() {
        _player = InitSystem.Player;
    }
    
    public override void OnExecute() {
        if (Input.GetKeyPressed(Keys.SPACE)) {
            Vector2 pos = World.GetComponent<PositionComponent>(_player).Position;
            Vector2 target = Camera.ScreenToWorldSpace(Input.GetMousePosition());
            
            Vector2 direction = Vector2.Normalize(target - pos);
            
            int bullet = World.CreateEntity(_bulletArchetype);
            
            World.SetComponent(bullet, new PositionComponent(pos));
            World.SetComponent(bullet, new ScaleComponent(1, 1));
            World.SetComponent(bullet, new ColliderComponent(new BoxCollider(10, 10)));
            World.SetComponent(bullet, new VelocityComponent(direction * bulletSpeed));
            World.SetComponent(bullet, new RectPrimitiveComponent(10, 10));
            World.SetComponent(bullet, new ColorComponent(ColorRgb.Black));
        }
    }
    
    public override void OnShutdown() { }
}