using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;
using LambdaEngine.Types;

namespace LambdaEngine.Components.Rendering;

[EcsComponent]
public struct ColorComponent(ColorRgb color) : IEcsComponent {
    public ColorRgb Color = color;
    private byte padding;
}