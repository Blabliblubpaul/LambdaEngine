namespace LambdaEngine.Rendering;

/// <summary>
/// A 24bit integer used to uniquely identify a registered gpu pipeline.
/// </summary>
public readonly struct RenderPipelineId {
    public const uint MaxValue = 0x00FFFFFF;

    public static readonly RenderPipelineId INVALID = default;
    
    public uint Id { get; }

    public RenderPipelineId(uint id) {
        if (id == 0 || id > MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        
        Id = id;
    }
}