using System.Numerics;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Rendering.PrimitiveComponents;

[EcsComponent]
public struct RectPrimitiveComponent(float x, float y) : IEcsComponent {
    public int ZIndex = 0;
    public Vector2 Size = new(x ,y);

    public RectPrimitiveComponent(float x, float y, int zIndex) : this(x, y) {
        ZIndex = zIndex;
    }
}