namespace LambdaEngine.Assets;

internal readonly struct DecoderKey : IEquatable<DecoderKey> {
    public readonly string Extension;
    public readonly Type Output;

    public DecoderKey(string extension, Type output) {
        Extension = extension.Trim().ToLower();
        Output = output;
    }

    public bool Equals(DecoderKey other) {
        return Extension == other.Extension && Output == other.Output;
    }

    public override bool Equals(object? obj) {
        return obj is DecoderKey other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Extension, Output);
    }

    public static bool operator ==(DecoderKey left, DecoderKey right) {
        return left.Equals(right);
    }

    public static bool operator !=(DecoderKey left, DecoderKey right) {
        return !left.Equals(right);
    }
}