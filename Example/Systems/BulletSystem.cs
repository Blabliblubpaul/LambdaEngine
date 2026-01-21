using Example.Components;
using LambdaEngine.Physics;
using LambdaEngine.System;

namespace Example.Systems;

public class BulletSystem : EcsSystem {
    public override void OnStartup() { }
    
    public override void OnExecute() {
        foreach (ref readonly Collision entity in Physics.Collisions()) {
            if (!(Physics.IsCollisionParticipant<BulletTagComponent>(World, entity, out _) &&
                 Physics.IsCollisionParticipant<BlockTagComponent>(World, entity, out _))) {
                continue;
            }

            World.MarkEntityForDestruction(entity.IdEntityA);
            World.MarkEntityForDestruction(entity.IdEntityB);
        }
    }
    
    public override void OnShutdown() { }
}