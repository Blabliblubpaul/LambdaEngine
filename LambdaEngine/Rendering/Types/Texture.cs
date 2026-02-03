using SDL3;

namespace LambdaEngine.Rendering;

public unsafe class Texture {
    internal readonly SDL.Surface* Handle;
    
    public TextureId ID { get; }

    public int Width {
        get => Handle->Width;
    }

    public int Height {
        get => Handle->Height;
    }
    
    internal Texture(SDL.Surface* handle, TextureId id) {
        Handle = handle;
        ID = id;
    }
}