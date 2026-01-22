using System.Drawing;
using System.Numerics;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.Interfaces;
using LambdaEngine.Debug;

namespace LambdaEngine.Physics;

public sealed class CollisionSystem : ISystem {
    private EcsWorld _world;
    private EcsQuery _colliderQuery;

    internal const float cellSize = 100.0f;
    internal Dictionary<(int cx, int cy), List<int>> _grid = new();
    
    internal CollisionSystem() { }

    public void OnSetup(LambdaEngine engine, EcsWorld world) {
        _world = world;

        _colliderQuery = EcsQuery.Create(_world).Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Include<ColliderComponent>()
            .Build();
    }

    public void OnStartup() { }

    public void OnExecute() {
        Physics.collisionCount = 0;
        _grid.Clear();

        QueryCollection<PositionComponent, ScaleComponent, ColliderComponent> colliders =
            _colliderQuery.Execute<PositionComponent, ScaleComponent, ColliderComponent>();

        
        // TODO: allow direct indexing into query results to avoid this kind of caching.
        int colliderCount = colliders.EntityCount;
        PositionComponent[] positions = new PositionComponent[colliderCount];
        ScaleComponent[] scales = new ScaleComponent[colliderCount];
        ColliderComponent[] collidersArr = new ColliderComponent[colliderCount];
        int[] entityIds = new int[colliderCount];

        int maxCollisions = (int)(colliders.EntityCount * (colliders.EntityCount - 1) * 0.5);

        if (Physics.collisions.Length < maxCollisions) {
            Physics.collisions = new Collision[maxCollisions];
        }
        
        Physics.collisionCount = 0;

        int idx = 0;
        foreach (var entity in colliders.GetComponents()) {
            ref var pos = ref entity.Item0;
            ref var scale = ref entity.Item1;
            ref var collider = ref entity.Item2;
        
            Vector2 size = collider.boxCollider.Size * scale.Scale;
        
            Vector2 halfSize = size * 0.5f;
        
            int minCx = (int)MathF.Floor((pos.Position.X - halfSize.X) / cellSize);
            int maxCx = (int)MathF.Floor((pos.Position.X + halfSize.X) / cellSize);
            int minCy = (int)MathF.Floor((pos.Position.Y - halfSize.Y) / cellSize);
            int maxCy = (int)MathF.Floor((pos.Position.Y + halfSize.Y) / cellSize);
        
            for (int cx = minCx; cx <= maxCx; cx++) {
                for (int cy = minCy; cy <= maxCy; cy++) {
                    if (!_grid.TryGetValue((cx, cy), out List<int> list)) {
                        list = new List<int>(4);
                        _grid[(cx, cy)] = list;
                    }
        
                    list.Add(idx);
                }
            }
            
            positions[idx] = entity.Item0;
            scales[idx] = entity.Item1;
            collidersArr[idx] = entity.Item2;
            entityIds[idx] = entity.Id;
            idx++;
        }
        
        foreach (var kv in _grid) {
            var bucket = kv.Value;
            int count = bucket.Count;
        
            for (int i = 0; i < count; i++) {
                int aIndex = bucket[i];
        
                // ref PositionComponent aPos = ref _world.GetComponent<PositionComponent>(aIndex);
                // ref ScaleComponent aScale = ref _world.GetComponent<ScaleComponent>(aIndex);
                // ref ColliderComponent aCol = ref _world.GetComponent<ColliderComponent>(aIndex);

                ref PositionComponent aPos = ref positions[aIndex];
                ref ScaleComponent aScale = ref scales[aIndex];
                ref ColliderComponent aCol = ref collidersArr[aIndex];
        
                for (int j = i + 1; j < count; j++) {
                    int bIndex = bucket[j];
        
                    // Skip the same entity as well as pairs that have already been processed.
                    // for (int j = 0; j < i + 1; j++) {
                    //     if (!e1.MoveNext()) {
                    //         done = true;
                    //     }
                    // }
        
                    // ref PositionComponent bPos = ref _world.GetComponent<PositionComponent>(bIndex);
                    // ref ScaleComponent bScale = ref _world.GetComponent<ScaleComponent>(bIndex);
                    // ref ColliderComponent bCol = ref _world.GetComponent<ColliderComponent>(bIndex);

                    ref PositionComponent bPos = ref positions[bIndex];
                    ref ScaleComponent bScale = ref scales[bIndex];
                    ref ColliderComponent bCol = ref collidersArr[bIndex];
        
                    switch (aCol.type) {
                        case ColliderType.BOX when bCol.type == ColliderType.BOX: {
                            ref BoxCollider aBox = ref aCol.AsBoxCollider();
                            ref BoxCollider bBox = ref bCol.AsBoxCollider();
                            BoxColliderSnapshot a = BoxColliderSnapshot.Create(aPos, aScale, aBox);
                            BoxColliderSnapshot b = BoxColliderSnapshot.Create(bPos, bScale, bBox);
                            CollidesBoxBox(in a, entityIds[aIndex], in b, entityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.BOX when bCol.type == ColliderType.CIRCLE: {
                            ref BoxCollider aBox = ref aCol.AsBoxCollider();
                            ref CircleCollider bCirc = ref bCol.AsCircleCollider();
                            BoxColliderSnapshot a = BoxColliderSnapshot.Create(aPos, aScale, aBox);
                            CircleColliderSnapshot b = CircleColliderSnapshot.Create(bPos, bScale, bCirc);
                            CollidesBoxCircle(in a, entityIds[aIndex], in b, entityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.CIRCLE when bCol.type == ColliderType.BOX: {
                            ref CircleCollider aCirc = ref aCol.AsCircleCollider();
                            ref BoxCollider bBox = ref bCol.AsBoxCollider();
                            CircleColliderSnapshot a = CircleColliderSnapshot.Create(aPos, aScale, aCirc);
                            BoxColliderSnapshot b = BoxColliderSnapshot.Create(bPos, bScale, bBox);
                            CollidesBoxCircle(in b, entityIds[aIndex], in a, entityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.CIRCLE when bCol.type == ColliderType.CIRCLE: {
                            ref CircleCollider aCirc = ref aCol.AsCircleCollider();
                            ref CircleCollider bCirc = ref bCol.AsCircleCollider();
                            CircleColliderSnapshot a = CircleColliderSnapshot.Create(aPos, aScale, aCirc);
                            CircleColliderSnapshot b = CircleColliderSnapshot.Create(bPos, bScale, bCirc);
                            CollidesCircleCircle(in a, entityIds[aIndex], in b, entityIds[bIndex]);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnShutdown() { }

    /// <summary>
    /// Computes a collision between a <see cref="BoxCollider"/> and a <see cref="BoxCollider"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Whether a collision was detected.</returns>
    private void CollidesBoxBox(in BoxColliderSnapshot a, int aId, in BoxColliderSnapshot b, int bId) {
        float maxXa = a.Position.X + a.Width * 0.5f;
        float minXa = maxXa - a.Width;
        float maxYa = a.Position.Y + a.Height * 0.5f;
        float minYa = maxYa - a.Height;

        float maxXb = b.Position.X + b.Width * 0.5f;
        float minXb = maxXb - b.Width;
        float maxYb = b.Position.Y + b.Height * 0.5f;
        float minYb = maxYb - b.Height;

        if (maxXa < minXb || minXa > maxXb || maxYa < minYb || minYa > maxYb) {
            return;
        }

        float overlapX = MathF.Min(maxXa, maxXb) - MathF.Max(minXa, minXb);
        float overlapY = MathF.Min(maxYa, maxYb) - MathF.Max(minYa, minYb);

        float penetrationDepth;
        Vector2 normal;

        // Axis of the least penetration
        if (overlapX < overlapY) {
            penetrationDepth = overlapX;
            normal = a.Position.X < b.Position.X ? new Vector2(-1, 0) : new Vector2(1, 0);
        }
        else {
            penetrationDepth = overlapY;
            normal = a.Position.Y < b.Position.Y ? new Vector2(0, -1) : new Vector2(0, 1);
        }

        // Collision Bounds
        float interMinX = MathF.Max(minXa, minXb);
        float interMaxX = MathF.Min(maxXa, maxXb);
        float interMinY = MathF.Max(minYa, minYb);
        float interMaxY = MathF.Min(maxYa, maxYb);

        RectangleF collisionBounds = new(
            interMinX,
            interMinY,
            interMaxX - interMinX,
            interMaxY - interMinY
        );

        Physics.collisions[Physics.collisionCount++] = new Collision(aId, bId, penetrationDepth, normal, collisionBounds);
    }

    /// <summary>
    /// Computes a collision between a <see cref="BoxCollider"/> and a <see cref="CircleCollider"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Whether a collision was detected.</returns>
    private void CollidesBoxCircle(in BoxColliderSnapshot a, int aId, in CircleColliderSnapshot b, int bId) {
        float maxXa = a.Position.X + a.Width * 0.5f;
        float minXa = maxXa - a.Width;
        float maxYa = a.Position.Y + a.Height * 0.5f;
        float minYa = maxYa - a.Height;

        // Clamp circle center to box edges (closest point)
        float closestX = MathF.Max(minXa, MathF.Min(b.Position.X, maxXa));
        float closestY = MathF.Max(minYa, MathF.Min(b.Position.Y, maxYa));

        float distX = b.Position.X - closestX;
        float distY = b.Position.Y - closestY;

        float distanceSquared = distX * distX + distY * distY;

        if (distanceSquared > b.Radius * b.Radius) {
            return;
        }

        float distance = MathF.Sqrt(distanceSquared);

        float penetrationDepth;
        Vector2 normal;
        RectangleF collisionBounds;

        if (distance > 0.0001f) {
            penetrationDepth = b.Radius - distance;
            normal = new Vector2(distX / distance, distY / distance);
        }
        else {
            // Circle center is inside box -> axis of least penetration
            float left = b.Position.X - minXa;
            float right = maxXa - b.Position.X;
            float top = maxYa - b.Position.Y;
            float bottom = b.Position.Y - minYa;

            float minPen = MathF.Min(MathF.Min(left, right), MathF.Min(top, bottom));

            if (minPen == left) {
                penetrationDepth = left;
                normal = new Vector2(-1, 0);
            }
            else if (minPen == right) {
                penetrationDepth = right;
                normal = new Vector2(1, 0);
            }
            else if (minPen == top) {
                penetrationDepth = top;
                normal = new Vector2(0, 1);
            }
            else {
                penetrationDepth = bottom;
                normal = new Vector2(0, -1);
            }
        }

        // Approximate intersection rectangle: bounding box of the circle clipped by the box
        float interMinX = MathF.Max(minXa, b.Position.X - b.Radius);
        float interMaxX = MathF.Min(maxXa, b.Position.X + b.Radius);
        float interMinY = MathF.Max(minYa, b.Position.Y - b.Radius);
        float interMaxY = MathF.Min(maxYa, b.Position.Y + b.Radius);

        collisionBounds = new RectangleF(
            interMinX,
            interMinY,
            interMaxX - interMinX,
            interMaxY - interMinY
        );

        Physics.collisions[Physics.collisionCount++] = new Collision(aId, bId, penetrationDepth, normal, collisionBounds);
    }

    /// <summary>
    /// Computes a collision between a <see cref="CircleCollider"/> and a <see cref="CircleCollider"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Whether a collision was detected.</returns>
    private void CollidesCircleCircle(in CircleColliderSnapshot a, int aId, in CircleColliderSnapshot b, int bId) {
        float distX = b.Position.X - a.Position.X;
        float distY = b.Position.Y - a.Position.Y;

        float distanceSquared = distX * distX + distY * distY;

        float bothRadii = a.Radius + b.Radius;

        if (distanceSquared > bothRadii * bothRadii) {
            return;
        }

        float distance = MathF.Sqrt(distanceSquared);

        float penetrationDepth;
        Vector2 normal;
        RectangleF collisionBounds;

        penetrationDepth = bothRadii - distance;

        if (distance > 0.0001f) {
            normal = new Vector2(distX / distance, distY / distance);
        }
        else {
            // Centers are exactly on top of each other -> arbitrary normal
            normal = new Vector2(1, 0);
        }

        // Approximate intersection rectangle: overlap bounding box
        float interMinX = MathF.Max(a.Position.X - a.Radius, b.Position.X - b.Radius);
        float interMaxX = MathF.Min(a.Position.X + a.Radius, b.Position.X + b.Radius);
        float interMinY = MathF.Max(a.Position.Y - a.Radius, b.Position.Y - b.Radius);
        float interMaxY = MathF.Min(a.Position.Y + a.Radius, b.Position.Y + b.Radius);

        collisionBounds = new RectangleF(
            interMinX,
            interMinY,
            interMaxX - interMinX,
            interMaxY - interMinY
        );

        Physics.collisions[Physics.collisionCount++] = new Collision(aId, bId, penetrationDepth, normal, collisionBounds);
    }
}