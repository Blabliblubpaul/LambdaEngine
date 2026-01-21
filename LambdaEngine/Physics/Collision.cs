using System.Drawing;
using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Physics;

public readonly struct Collision {
    public readonly int IdEntityA;
    public readonly int IdEntityB;

    public readonly float PenetrationDepth;
    public readonly Vector2 CollisionNormal;
    public readonly RectangleF CollisionBounds;

    internal Collision(int aId, int bId, float penetrationDepth,  Vector2 collisionNormal, RectangleF collisionBounds) {
        IdEntityA = aId;
        IdEntityB = bId;

        PenetrationDepth = penetrationDepth;
        CollisionNormal = collisionNormal;
        CollisionBounds = collisionBounds;
    }

    public readonly bool HasParticipant(int entity) {
        return entity == IdEntityA || entity == IdEntityB;
    }
}