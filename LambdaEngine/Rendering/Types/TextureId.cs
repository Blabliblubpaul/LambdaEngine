namespace LambdaEngine.Rendering;

/// <summary>
/// A 24bit integer used to uniquely identify a loaded texture.
/// </summary>
public readonly struct TextureId {
    private const uint MAX_ID = 0x00FFFFFF;

    public static readonly TextureId NO_TEXTURE = new(0);
    
    public uint Id { get; }

    public TextureId(uint id) {
        if (id > MAX_ID) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        
        Id = id;
    }
}