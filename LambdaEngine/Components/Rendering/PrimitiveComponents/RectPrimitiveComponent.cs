using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Rendering.PrimitiveComponents;

[EcsComponent]
public struct RectPrimitiveComponent(float x, float y) : IEcsComponent {
    public sbyte ZIndex = 0;
    public Vector2 Size = new(x ,y);

    public RectPrimitiveComponent(float x, float y, sbyte zIndex) : this(x, y) {
        ZIndex = zIndex;
    }
}