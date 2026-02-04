namespace LambdaEngine.Rendering;

/// <summary>
/// A 24bit integer used to uniquely identify a loaded texture.
/// </summary>
public readonly struct TextureId : IEquatable<TextureId> {
    private const uint MAX_ID = 0x00FFFFFF;

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
    
    public bool Equals(TextureId other) {
        return Id == other.Id;
    }

    public override bool Equals(object? obj) {
        return obj is TextureId other && Equals(other);
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }

    public static bool operator ==(TextureId left, TextureId right) {
        return left.Equals(right);
    }

    public static bool operator !=(TextureId left, TextureId right) {
        return !left.Equals(right);
    }
}