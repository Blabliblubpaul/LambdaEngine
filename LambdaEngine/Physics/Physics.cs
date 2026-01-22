using System.Numerics;
using LambdaEngine.Core;
namespace LambdaEngine.Physics;

// TODO: Add collision information (penetration depth, normal, intersection rect)
public static class Physics {
    internal static Collision[] collisions = new  Collision[64];
    internal static int collisionCount = 0;

    public static CollisionSystem CollisionSystem { get; } = new();
    
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

    public static bool Raycast(Vector2 origin, Vector2 direction, float distance, out RaycastHit hit) {
        const float cellSize = CollisionSystem.cellSize;
        
        direction = Vector2.Normalize(direction);
        
        int cellX = (int)MathF.Floor(origin.X / cellSize);
        int cellY = (int)MathF.Floor(origin.Y / cellSize);

        int stepX = MathF.Sign(direction.X);
        int stepY = MathF.Sign(direction.Y);

        float nextGridX, nextGridY;

        if (direction.X > 0) {
            nextGridX = (cellX + 1) * cellSize;
        }
        else {
            nextGridX = cellX * cellSize;
        }

        if (direction.Y > 0) {
            nextGridY = (cellY + 1) * cellSize;
        }
        else {
            nextGridY = cellY * cellSize;
        }

        float tMaxX, tMaxY, tDeltaX, tDeltaY;

        if (direction.X == 0) {
            tMaxX = tDeltaX = float.PositiveInfinity;
        }
        else {
            tMaxX = (nextGridX - origin.X) / direction.X;
            tDeltaX = cellSize / MathF.Abs(direction.X);
        }

        if (direction.Y == 0) {
            tMaxY = tDeltaY = float.PositiveInfinity;
        }
        else {
            tMaxY = (nextGridY - origin.Y) / direction.Y;
            tDeltaY = cellSize / MathF.Abs(direction.Y);   
        }

        Dictionary<(int cx, int cy), List<int>> grid = CollisionSystem._grid;
        
        float t = 0;
        while (t <= distance) {
            if (grid.TryGetValue((cellX, cellY), out List<int> colliders)) {
                // ...?
            }

            if (tMaxX < tMaxY) {
                cellX += stepX;
                t = tMaxX;
                tMaxX += tDeltaX;
            }
            else {
                cellY += stepY;
                t = tMaxY;
                tMaxY += tDeltaY;
            }
        }
    }
}