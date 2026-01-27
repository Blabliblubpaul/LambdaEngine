using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;
using LambdaEngine.Rendering;

namespace LambdaEngine.Components.Rendering;

[EcsComponent]
public struct SpriteComponent : IEcsComponent {
    internal IntPtr TextureHandle;
    public sbyte ZIndex;

    public Texture Texture {
        get => new(TextureHandle);
        set => TextureHandle = value.Handle;
    }
    
    public SpriteComponent(Texture texture, sbyte zIndex = 0) {
        TextureHandle = texture.Handle;
        ZIndex = zIndex;
    }
}