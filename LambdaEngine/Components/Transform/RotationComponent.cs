using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Components.Transform;

[EcsComponent]
public struct RotationComponent : IEcsComponent {
    public float Rotation;
}