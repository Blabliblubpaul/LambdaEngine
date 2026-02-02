using LambdaEngine.Core;
using LambdaEngine.Core.Attributes;
using LambdaEngine.Rendering;
using LambdaEngine.Rendering.Types;

namespace LambdaEngine.Components.Rendering;

[EcsComponent]
public struct SpriteComponent : IEcsComponent {
    internal TextureId TextureId;
    public sbyte ZIndex;

    // TODO: allow texture/object getter and setter
    public Texture Texture {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    
    public SpriteComponent(Texture texture, sbyte zIndex = 0) {
        TextureId = texture.ID;
        ZIndex = zIndex;
    }
}