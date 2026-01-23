using System.Numerics;

namespace LambdaEngine.Physics;

public readonly ref struct RaycastHit(Vector2 normal, float distance) {
    public readonly Vector2 Normal = normal;
    public readonly float Distance = distance;
}