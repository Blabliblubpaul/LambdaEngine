using System.Numerics;

namespace LambdaEngine.Rendering;

public static class Camera {
    public static Vector2 Position =  Vector2.Zero;
    public static float Zoom = 1.0f;

    private static Vector2 ScreenSize {
        get => new(WindowManager.WindowWidth, WindowManager.WindowHeight);
    }

    public static Vector2 WorldToCameraSpace(Vector2 worldPos) {
        return worldPos - Position;
    }

    public static Vector2 CameraToWorldSpace(Vector2 cameraPos) {
        return cameraPos + Position;
    }

    public static Vector2 CameraToScreenSpace(Vector2 cameraPos) {
        Vector2 temp = cameraPos * Zoom;
        temp.Y = -temp.Y;
        
        return temp + ScreenSize * 0.5f;
    }

    public static Vector2 ScreenToCameraSpace(Vector2 screenPos) {
        Vector2 temp = screenPos - ScreenSize * 0.5f;
        temp.Y = -temp.Y;
        
        return temp / Zoom;
    }

    public static Vector2 WorldToScreenSpace(Vector2 worldPos) {
        return CameraToScreenSpace(WorldToCameraSpace(worldPos));
    }

    public static Vector2 ScreenToWorldSpace(Vector2 screenPos) {
        return CameraToWorldSpace(ScreenToCameraSpace(screenPos));
    }
}