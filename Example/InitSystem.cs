using Example.Components;
using Example.Systems;
using LambdaEngine.Components;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Components.Rendering.PrimitiveComponents;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Archetypes;
using LambdaEngine.Interfaces;
using LambdaEngine.Physics;
using LambdaEngine.Types;

namespace Example;

public class InitSystem : IStagelessSystem {
    public static int Player {
        get => _player;
    }
    
    private static int _player;
    
    
    EcsWorld _world;
    
    public void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
        _world = world;
    }

    public void OnStartup() {
        ArchetypeHandle blockArchetype = _world.GetOrCreateArchetype(typeof(PositionComponent), typeof(ScaleComponent), typeof(VelocityComponent),
            typeof(ColliderComponent), typeof(RectPrimitiveComponent), typeof(ColorComponent), typeof(BlockTagComponent));
        
        ArchetypeHandle playerArchetype = _world.GetOrCreateArchetype(typeof(PositionComponent), typeof(ScaleComponent), typeof(VelocityComponent),
            typeof(ColliderComponent), typeof(SpriteComponent), typeof(PlayerTagComponent), typeof(ColorComponent));
        
        ArchetypeHandle staticBlockArchetype = _world.GetOrCreateArchetype(typeof(PositionComponent), typeof(ScaleComponent), typeof(ColliderComponent),
            typeof(RectPrimitiveComponent), typeof(ColorComponent), typeof(BlockTagComponent));
        
        int player = _world.CreateEntity(playerArchetype);
        _world.SetComponent(player, new PositionComponent(0, 0));
        _world.SetComponent(player, new ScaleComponent(0.75f, 0.75f));
        _world.SetComponent(player, new ColliderComponent(new BoxCollider(50, 50)));
        _world.SetComponent(player, new ColorComponent(new ColorRgb(0, 0, 0)));
        _world.SetComponent(player, new SpriteComponent(TextureSystem.Instance.PlayerTexture));
        // _world.SetComponent(player, new RectPrimitiveComponent(50, 50));
        
        _player = player;

        int staticBlock = _world.CreateEntity(staticBlockArchetype);
        _world.SetComponent(staticBlock, new PositionComponent(0, -200));
        _world.SetComponent(staticBlock, new ScaleComponent(1, 1));
        _world.SetComponent(staticBlock, new ColliderComponent(new BoxCollider(200, 50)));
        _world.SetComponent(staticBlock, new ColorComponent(ColorRgb.White));
        _world.SetComponent(staticBlock, new RectPrimitiveComponent(200, 50, -1));

        for (int i = 0; i < 10; i++) {
            int entity = _world.CreateEntity(blockArchetype);
            
            _world.SetComponent(entity, new PositionComponent((i * 35) - 400, (-i * 35) + 400));
            _world.SetComponent(entity, new ScaleComponent(1, 1));
            _world.SetComponent(entity, new ColliderComponent(new BoxCollider(50, 50)));
            _world.SetComponent(entity, new VelocityComponent(60 * MathF.Sqrt(i + 1), -60 * MathF.Sqrt(i + 1)));
            _world.SetComponent(entity, new ColorComponent(new ColorRgb((byte)Random.Shared.Next(50, 200), (byte)Random.Shared.Next(50, 200), (byte)Random.Shared.Next(50, 200))));
            _world.SetComponent(entity, new RectPrimitiveComponent(50, 50));
        }
    }

    public void OnShutdown() { }
}