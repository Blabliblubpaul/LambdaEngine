using System.Runtime.InteropServices;
using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;

namespace LambdaEngine.Physics;

[StructLayout(LayoutKind.Explicit)]
[EcsComponent]
public unsafe struct ColliderComponent : IEcsComponent {
    [FieldOffset(0)] internal BoxCollider boxCollider;
    [FieldOffset(0)] internal CircleCollider circleCollider;
    
    [FieldOffset(16)] internal readonly ColliderType type;

    // 7 bytes of padding to ensure alignment to 4 (size 24)
    [FieldOffset(23)] private readonly byte padding = 0;
    
    public bool IsBoxCollider {
        get => type == ColliderType.BOX;
    }

    public bool IsCircleCollider {
        get => type == ColliderType.CIRCLE;
    }

    public ColliderComponent(BoxCollider boxCollider) {
        this.boxCollider = boxCollider;
        type = ColliderType.BOX;
    }
    
    public ColliderComponent(CircleCollider circleCollider) {
        this.circleCollider = circleCollider;
        type = ColliderType.CIRCLE;   
    }
    
    public ref BoxCollider AsBoxCollider() {
        if (type != ColliderType.BOX) {
            throw new InvalidOperationException("ColliderComponent is not a BoxCollider.");
        }
        
        return ref boxCollider;
    }
    
    public ref CircleCollider AsCircleCollider() {
        if (type != ColliderType.CIRCLE) {
            throw new InvalidOperationException("ColliderComponent is not a CircleCollider.");
        }
        
        return ref circleCollider;
    }
}