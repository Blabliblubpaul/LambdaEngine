using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LambdaEngine.Core.InteropTypes;

/// <summary>
/// Represents a blittable boolean.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Bool8 : IEquatable<Bool8> {
    public static readonly Bool8 True = new(1);
    public static readonly Bool8 False = default;
    
    private readonly byte _value;

    private Bool8(byte value) {
        _value = value;
    }
    
    public bool IsTrue {
        get => _value != 0;
    }

    public bool IsFalse {
        get => _value == 0;
    }

    public static implicit operator bool (Bool8 value) {
        return value._value != 0;
    }

    public static implicit operator Bool8(bool value) {
        return value ? True : False;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is Bool8 other && Equals(other);
    }

    public bool Equals(Bool8 other) {
        return _value == 0 && other._value == 0 ||
               _value != 0 && other._value != 0;
    }
    
    public override int GetHashCode() {
        return ((bool)this).GetHashCode();
    }

    public override string ToString() {
        return _value != 0 ? bool.TrueString : bool.FalseString;
    }

    public static bool operator ==(Bool8 left, Bool8 right) {
        return left.Equals(right);
    }

    public static bool operator !=(Bool8 left, Bool8 right) {
        return !(left == right);
    }
}