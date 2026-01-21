using Example.Components;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Physics;
using LambdaEngine.System;
using LambdaEngine.Types;

namespace Example.Systems;

public class PlayerCollisionSystem : EcsSystem {
    private int _player;

    public override void OnStartup() {
        _player = InitSystem.Player;
    }
    
    public override void OnExecute() {
        ref ColorComponent playerColor = ref World.GetComponent<ColorComponent>(_player);

        bool collided = false;
        foreach (ref readonly Collision collision in Physics.Collisions()) {
            if (collision.HasParticipant(_player) &&
                Physics.IsCollisionParticipant<BlockTagComponent>(World, collision, out _)) {
                collided = true;
            }
        }

        if (collided) {
            playerColor.Color = new ColorRgb(255, 0, 0);
        }
        else {
            playerColor.Color = new ColorRgb(0, 255, 0);
        }
    }
    
    public override void OnShutdown() { }
}