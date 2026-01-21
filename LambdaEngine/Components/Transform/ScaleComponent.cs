using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Transform;

[EcsComponent]
public struct ScaleComponent : IEcsComponent {
    public Vector2 Scale = Vector2.One;

    public ScaleComponent(Vector2 scale) : this() {
        Scale = scale;
    }
    
    public ScaleComponent(float x, float y) : this() {
        Scale = new Vector2(x, y);
    }

    public override string ToString() {
        return $"Scale: {Scale}";
    }
}