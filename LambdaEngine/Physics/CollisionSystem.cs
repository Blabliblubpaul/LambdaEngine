using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.Interfaces;
using LambdaEngine.Debug;

namespace LambdaEngine.Physics;

public sealed class CollisionSystem : ISystem {
    internal const int INTIAL_COLLIDER_CAPACITY = 256;
    internal const float GRID_CELL_SIZE = 100.0f;

    public static readonly CollisionSystem Instance = new();
    
    private EcsWorld _world;
    private EcsQuery _colliderQuery;
    
    internal readonly Dictionary<(int cx, int cy), List<int>> Grid = new();

    internal int ColliderCount = 0;
    internal int ColliderCapacity = INTIAL_COLLIDER_CAPACITY;
    internal PositionComponent[] Positions;
    internal ScaleComponent[] Scales;
    internal ColliderComponent[] CollidersArr;
    internal int[] EntityIds;
    
    internal CollisionSystem() { }

    public void OnSetup(LambdaEngine engine, EcsWorld world) {
        _world = world;

        _colliderQuery = EcsQuery.Create(_world).Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Include<ColliderComponent>()
            .Build();
    }

    public void OnStartup() {
        Positions = new PositionComponent[ColliderCapacity];
        Scales = new ScaleComponent[ColliderCapacity];
        CollidersArr = new ColliderComponent[ColliderCapacity];
        EntityIds = new int[ColliderCapacity];
    }

    public void OnExecute() {
        Physics.collisionCount = 0;

        foreach (List<int> bucket in Grid.Values) {
            bucket.Clear();
        }

        QueryCollection<PositionComponent, ScaleComponent, ColliderComponent> colliders =
            _colliderQuery.Execute<PositionComponent, ScaleComponent, ColliderComponent>();
        
        ColliderCount = colliders.EntityCount;
        if (ColliderCount > ColliderCapacity) {
            ColliderCapacity = Math.Max(ColliderCount, ColliderCapacity * 2);
            
            Positions = new PositionComponent[ColliderCapacity];
            Scales = new ScaleComponent[ColliderCapacity];
            CollidersArr = new ColliderComponent[ColliderCapacity];
            EntityIds = new int[ColliderCapacity];
        }

        int maxCollisions = (int)(colliders.EntityCount * (colliders.EntityCount - 1) * 0.5);

        if (Physics.collisions.Length < maxCollisions) {
            Physics.collisions = new Collision[maxCollisions];
        }
        
        Physics.collisionCount = 0;

        int index = 0;
        foreach (ComponentRef<PositionComponent, ScaleComponent, ColliderComponent> entity in colliders.GetComponents()) {
            ref PositionComponent pos = ref entity.Item0;
            ref ScaleComponent scale = ref entity.Item1;
            ref ColliderComponent collider = ref entity.Item2;
        
            Vector2 size = collider.boxCollider.Size * scale.Scale;
        
            Vector2 halfSize = size * 0.5f;
        
            int minCx = (int)MathF.Floor((pos.Position.X - halfSize.X) / GRID_CELL_SIZE);
            int maxCx = (int)MathF.Floor((pos.Position.X + halfSize.X) / GRID_CELL_SIZE);
            int minCy = (int)MathF.Floor((pos.Position.Y - halfSize.Y) / GRID_CELL_SIZE);
            int maxCy = (int)MathF.Floor((pos.Position.Y + halfSize.Y) / GRID_CELL_SIZE);
        
            for (int cx = minCx; cx <= maxCx; cx++) {
                for (int cy = minCy; cy <= maxCy; cy++) {
                    if (!Grid.TryGetValue((cx, cy), out List<int> list)) {
                        list = new List<int>(4);
                        Grid[(cx, cy)] = list;
                    }
        
                    list.Add(index);
                }
            }
            
            Positions[index] = entity.Item0;
            Scales[index] = entity.Item1;
            CollidersArr[index] = entity.Item2;
            EntityIds[index] = entity.Id;
            index++;
        }
        
        foreach (KeyValuePair<(int cx, int cy), List<int>> kv in Grid) {
            List<int> bucket = kv.Value;
            int count = bucket.Count;
        
            for (int i = 0; i < count; i++) {
                int aIndex = bucket[i];

                ref PositionComponent aPos = ref Positions[aIndex];
                ref ScaleComponent aScale = ref Scales[aIndex];
                ref ColliderComponent aCol = ref CollidersArr[aIndex];
        
                for (int j = i + 1; j < count; j++) {
                    int bIndex = bucket[j];

                    ref PositionComponent bPos = ref Positions[bIndex];
                    ref ScaleComponent bScale = ref Scales[bIndex];
                    ref ColliderComponent bCol = ref CollidersArr[bIndex];
        
                    switch (aCol.type) {
                        case ColliderType.BOX when bCol.type == ColliderType.BOX: {
                            ref BoxCollider aBox = ref aCol.AsBoxCollider();
                            ref BoxCollider bBox = ref bCol.AsBoxCollider();
                            BoxColliderSnapshot a = BoxColliderSnapshot.Create(aPos, aScale, aBox);
                            BoxColliderSnapshot b = BoxColliderSnapshot.Create(bPos, bScale, bBox);
                            CollidesBoxBox(in a, EntityIds[aIndex], in b, EntityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.BOX when bCol.type == ColliderType.CIRCLE: {
                            ref BoxCollider aBox = ref aCol.AsBoxCollider();
                            ref CircleCollider bCirc = ref bCol.AsCircleCollider();
                            BoxColliderSnapshot a = BoxColliderSnapshot.Create(aPos, aScale, aBox);
                            CircleColliderSnapshot b = CircleColliderSnapshot.Create(bPos, bScale, bCirc);
                            CollidesBoxCircle(in a, EntityIds[aIndex], in b, EntityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.CIRCLE when bCol.type == ColliderType.BOX: {
                            ref CircleCollider aCirc = ref aCol.AsCircleCollider();
                            ref BoxCollider bBox = ref bCol.AsBoxCollider();
                            CircleColliderSnapshot a = CircleColliderSnapshot.Create(aPos, aScale, aCirc);
                            BoxColliderSnapshot b = BoxColliderSnapshot.Create(bPos, bScale, bBox);
                            CollidesBoxCircle(in b, EntityIds[aIndex], in a, EntityIds[bIndex]);
                            break;
                        }
        
                        case ColliderType.CIRCLE when bCol.type == ColliderType.CIRCLE: {
                            ref CircleCollider aCirc = ref aCol.AsCircleCollider();
                            ref CircleCollider bCirc = ref bCol.AsCircleCollider();
                            CircleColliderSnapshot a = CircleColliderSnapshot.Create(aPos, aScale, aCirc);
                            CircleColliderSnapshot b = CircleColliderSnapshot.Create(bPos, bScale, bCirc);
                            CollidesCircleCircle(in a, EntityIds[aIndex], in b, EntityIds[bIndex]);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnShutdown() { }

    internal bool IntersectsRayBox(Vector2 rayOrigin, Vector2 rayDir, float rayMaxDistance, int boxIndex,
        out RaycastHit hit) {
        hit = default;

        float tMin = 0;
        float tMax = rayMaxDistance;

        Vector2 colPos = Positions[boxIndex].Position;
        Vector2 colScale = Scales[boxIndex].Scale;
        ColliderComponent col = CollidersArr[boxIndex];
        
        float w = col.boxCollider.Width * colScale.X;
        float h = col.boxCollider.Height * colScale.Y;

        float colMinX = MathF.Min(colPos.X, colPos.X + w);
        float colMaxX = MathF.Max(colPos.X, colPos.X + w);
        float colMinY = MathF.Min(colPos.Y, colPos.Y + h);
        float colMaxY = MathF.Max(colPos.Y, colPos.Y + h);

        int hitAxis = -1;
        
        // X axis
        if (rayDir.X != 0) {
            float inv = 1.0f / rayDir.X;
            float t1 = (colMinX - rayOrigin.X) * inv;
            float t2 = (colMaxX - rayOrigin.X) * inv;

            if (t1 > t2) {
                (t1, t2) = (t2, t1);
            }
            
            float prevMin = tMin;
            tMin = MathF.Max(tMin, t1);
            if (tMin != prevMin) {
                hitAxis = 0;
            }
            tMax = MathF.Min(tMax, t2);

            if (tMin > tMax) {
                return false;
            }
        } else if (rayOrigin.X < colMinX || rayOrigin.X > colMaxX) {
            return false;
        }
        
        // Y Axis
        if (rayDir.Y != 0) {
            float inv = 1.0f / rayDir.Y;
            float t1 = (colMinY - rayOrigin.Y) * inv;
            float t2 = (colMaxY - rayOrigin.Y) * inv;

            if (t1 > t2) {
                (t1, t2) = (t2, t1);
            }

            float prevMin = tMin;
            tMin = MathF.Max(tMin, t1);
            if (tMin != prevMin) {
                hitAxis = 1;
            }
            tMax = MathF.Min(tMax, t2);


            if (tMin > tMax) {
                return false;
            }
        } else if (rayOrigin.Y < colMinY || rayOrigin.Y > colMaxY) {
            return false;
        }

        Vector2 normal = hitAxis switch {
            0 => rayDir.X > 0 ? -Vector2.UnitX : Vector2.UnitX,
            1 => rayDir.Y > 0 ? -Vector2.UnitY : Vector2.UnitY,
            _ => Vector2.Zero
        };

        hit = new RaycastHit(normal, tMin);
        return true;
    }

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