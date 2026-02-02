namespace LambdaEngine.Rendering.Types;

/// <summary>
/// A 24bit integer used to uniquely identify a loaded texture.
/// </summary>
public readonly struct TextureId {
    private const uint MAX_ID = 0xFFFFFF;

    public static readonly TextureId NO_TEXTURE = new(0);
    
    public uint Id { get; }

    // This works, as MAX_ID is in range of Int32.
    internal int AsInt32 {
        get => (int)Id;
    }

    public TextureId(uint id) {
        if (id > MAX_ID) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        
        Id = id;
    }
}