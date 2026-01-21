namespace LambdaEngine.Physics;

public struct CircleCollider {
    public float Radius;

    public CircleCollider() {
        Radius = 1;
    }
    
    public CircleCollider(float radius) {
        Radius = radius;
    }
}