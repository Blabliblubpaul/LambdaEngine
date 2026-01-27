namespace LambdaEngine.Rendering;

internal readonly struct RenderKey {
    // 64bit: |8bit Z-Index|24bit PipelineId|24bit TextureId|8bit RenderType|
    private const ulong PIPELINE_MASK = 0x00FFFFFF00000000;
    private const ulong TEXTURE_MASK  = 0x00000000FFFFFF00;
    
    public readonly ulong Key;

    public sbyte ZIndex {
        get => (sbyte)((Key >> 56) - 128);
    }

    public RenderPipelineId PipelineId {
        get => new((uint)((Key & PIPELINE_MASK) >> 32));
    }

    public TextureId TextureId {
        get => new((uint)((Key & TEXTURE_MASK) >> 8));
    }

    public RenderCommandType RenderType {
        get => (RenderCommandType)Key;
    }

    public RenderKey(sbyte zIndex, RenderPipelineId pipelineId, TextureId textureId, RenderCommandType renderCommandType) {
        Key = 0;
        Key |= (ulong)(zIndex + 128) << 56;
        Key |= (ulong)pipelineId.Id << 32;
        Key |= (ulong)textureId.Id << 8;
        Key |= (ulong)renderCommandType;
    }
}