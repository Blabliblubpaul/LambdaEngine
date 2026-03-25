namespace LambdaEngine.Rendering;

/// <summary>
/// A 24bit integer used to uniquely identify a registered gpu pipeline.
/// </summary>
public readonly struct RenderPipelineId : IEquatable<RenderPipelineId> {
    internal const uint MaxValue = 0x00FFFFFF - 1;
    
    private const uint INVALID_ID = 0x00FFFFFF;

    public static readonly RenderPipelineId INVALID = new(INVALID_ID);
    
    public uint Id { get; }

    private RenderPipelineId(uint id) {
        Id = id;
    }

    internal static RenderPipelineId NewUnchecked(uint id) {
        return new RenderPipelineId(id);
    }

    public static RenderPipelineId New(uint id) {
        if (id > MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        
        return new RenderPipelineId(id);
    }
    
    public bool Equals(RenderPipelineId other) {
        return Id == other.Id;
    }

    public override bool Equals(object? obj) {
        return obj is RenderPipelineId other && Equals(other);
    }

    public override int GetHashCode() {
        return (int)Id;
    }

    public static bool operator ==(RenderPipelineId left, RenderPipelineId right) {
        return left.Equals(right);
    }

    public static bool operator !=(RenderPipelineId left, RenderPipelineId right) {
        return !left.Equals(right);
    }
}