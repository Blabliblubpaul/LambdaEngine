using LambdaEngine.Core;
using LambdaEngine.Core.Queries.ComponentEnumerators;

namespace LambdaEngine.Physics;

// TODO: Add collision information (penetration depth, normal, intersection rect)
public static class Physics {
    internal static Collision[] collisions = new  Collision[64];
    internal static int collisionCount = 0;
    
    public static ReadOnlySpan<Collision> Collisions() {
        return collisions.AsSpan(0, collisionCount);
    }
        
    public static bool IsCollisionParticipant<T>(EcsWorld world, in Collision collision, out int entity)
        where T : unmanaged, IEcsComponent {
        if (world.HasComponent<T>(collision.IdEntityA)) {
            entity = collision.IdEntityA;
            return true;
        }

        if (world.HasComponent<T>(collision.IdEntityB)) {
            entity = collision.IdEntityB;
            return true;
        }

        entity = -1;
        return false;
    }
}