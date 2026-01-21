using System.Numerics;
using LambdaEngine.Components.Transform;

namespace LambdaEngine.Physics;

public readonly struct CircleColliderSnapshot {
    public readonly Vector2 Position;
    public readonly float Radius;
    
    private CircleColliderSnapshot(Vector2 position, float radius) {
        throw new NotImplementedException("Circle Colliders are not yet supported.");
        
        Position = position;
        Radius = radius;
    }

    public static CircleColliderSnapshot Create(in PositionComponent position, in ScaleComponent scale, in CircleCollider collider) {
        return new CircleColliderSnapshot(position.Position, collider.Radius * scale.Scale.X);
    }
}