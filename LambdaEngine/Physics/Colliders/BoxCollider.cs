using System.Numerics;

namespace LambdaEngine.Physics;

public struct BoxCollider {
    public float Width;
    public float Height;

    public Vector2 Size {
        get => new (Width, Height);
    }

    public BoxCollider() {
        Width = 1;
        Height = 1;
    }

    public BoxCollider(float width, float height) {
        Width = width;
        Height = height;
    }
    
    public BoxCollider(Vector2 size) {
        Width = size.X;
        Height = size.Y;
    }
}