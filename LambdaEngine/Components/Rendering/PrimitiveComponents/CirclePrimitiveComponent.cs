using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Rendering.PrimitiveComponents;

[EcsComponent]
public struct CirclePrimitiveComponent : IEcsComponent {
    public float Radius;
}