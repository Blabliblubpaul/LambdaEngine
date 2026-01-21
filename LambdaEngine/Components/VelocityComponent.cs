using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components;

[EcsComponent]
public struct VelocityComponent(float x, float y) : IEcsComponent {
    public Vector2 Velocity = new(x, y);
    
    public VelocityComponent(Vector2 velocity) : this(velocity.X, velocity.Y) { }
}