using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LambdaEngine.Core.InteropTypes;

/// <summary>
/// Represents a blittable 16-bit character that can be implicitly converted to and from the C# <see cref="char"/> type.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public readonly struct Char16(char value) : IEquatable<Char16> {
    private readonly ushort _value = value;

    public static implicit operator char (Char16 value) {
        return (char)value._value;
    }

    public static implicit operator Char16(char value) {
        return new Char16(value);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is Char16 other && Equals(other);
    }

    public bool Equals(Char16 other) {
        return _value == other._value;
    }
    
    public override int GetHashCode() {
        return _value.GetHashCode();
    }

    public override string ToString() {
        return ((char)_value).ToString();
    }

    public static bool operator ==(Char16 left, Char16 right) {
        return left.Equals(right);
    }

    public static bool operator !=(Char16 left, Char16 right) {
        return !(left == right);
    }
}