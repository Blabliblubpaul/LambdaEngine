using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Transform;

[EcsComponent]
public struct PositionComponent(float x, float y) : IEcsComponent {
    public Vector2 Position = new(x, y);
    
    public PositionComponent(Vector2 position) : this(position.X, position.Y) { }

    public override string ToString() {
        return Position.ToString();
    }
}