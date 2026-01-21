using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace LambdaEngine.Core.InteropTypes;

/// <summary>
/// Represents a blittable 8-bit character.
/// </summary>
/// <param name="value">The byte value that encodes the character.</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Char8(byte value) : IEquatable<Char8> {
    private readonly byte _value = value;

    public static explicit operator char (Char8 value) {
        return (char)value._value;
    }

    public static explicit operator Char8(char c) {
        if (c > 0xFF) {
            throw new ArgumentOutOfRangeException(nameof(c), "Char value does not fit in a byte.");
        }
        
        return new Char8((byte)c);
    }

    public static implicit operator Char8(byte value) {
        return new Char8(value);
    }

    public static implicit operator byte(Char8 value) {
        return value._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is Char8 other && Equals(other);
    }

    public bool Equals(Char8 other) {
        return _value == other._value;
    }
    
    public override int GetHashCode() {
        return _value.GetHashCode();
    }

    public override string ToString() {
        return Encoding.ASCII.GetString([_value]);
    }

    public static bool operator ==(Char8 left, Char8 right) {
        return left.Equals(right);
    }

    public static bool operator !=(Char8 left, Char8 right) {
        return !(left == right);
    }
}