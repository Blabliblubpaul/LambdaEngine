using System.Numerics;
using SDL3;

namespace LambdaEngine.Rendering;

public class Texture {
    internal readonly IntPtr Handle;

    public Vector2 TextureSize {
        get => GetTextureSize(Handle);
    }
    
    internal Texture(IntPtr handle) {
        Handle = handle;
    }

    internal static Vector2 GetTextureSize(IntPtr textureHandle) {
        SDL.GetTextureSize(textureHandle, out float w, out float h);
        return new Vector2(w, h);
    }
}