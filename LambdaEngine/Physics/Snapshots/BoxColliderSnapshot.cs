using System.Numerics;
using LambdaEngine.Components.Transform;

namespace LambdaEngine.Physics;

public readonly struct BoxColliderSnapshot {
    public readonly Vector2 Position;
    public readonly float Width;
    public readonly float Height;

    private BoxColliderSnapshot(Vector2 position, float width, float height) {
        Position = position;
        Width = width;
        Height = height;
    }

    public static BoxColliderSnapshot Create(in PositionComponent position, in ScaleComponent scale, in BoxCollider collider) {
        Vector2 worldSize = new(collider.Width * scale.Scale.X, collider.Height * scale.Scale.Y);
        Vector2 worldPosition = position.Position;
        
        return new BoxColliderSnapshot(worldPosition, worldSize.X, worldSize.Y);
    }
}