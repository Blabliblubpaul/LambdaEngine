using System.Numerics;
using System.Runtime.CompilerServices;

namespace LambdaEngine.Core.Common;

public unsafe struct ComponentSet64 {
    private const int CompositionSize = 8;

    private fixed ulong _set[8];
    private ushort _componentCount = 0;

    public ushort ComponentCount {
        get => _componentCount;
    }

    public ComponentSet64(params ushort[] typeIds) {
        fixed (ulong* set = _set) {
            Unsafe.InitBlockUnaligned(set, 0, 64);
        }

        foreach (ushort type in typeIds) {
            AddComponent(type);
        }
    }

    private ComponentSet64(ulong* componentSet, ushort componentCount) {
        fixed (ulong* set = _set) {
            Unsafe.CopyBlockUnaligned(set, componentSet, 64);
        }

        _componentCount = componentCount;
    }

    public readonly ComponentSet64 With<T>() where T : unmanaged, IEcsComponent {
        ushort typeId = ComponentTypeRegistry.GetId<T>();

        if (!HasComponent(typeId)) {
            ulong* newSet = stackalloc ulong[8];

            fixed (ulong* set = _set) {
                Unsafe.CopyBlockUnaligned(newSet, set, 64);
            }

            newSet[typeId / 64] |= 1UL << (typeId % 64);

            return new ComponentSet64(newSet, (ushort)(_componentCount + 1));
        }

        fixed (ulong* set = _set) {
            return new ComponentSet64(set, _componentCount);
        }
    }

    public readonly ComponentSet64 Without<T>() where T : unmanaged, IEcsComponent {
        ushort typeId = ComponentTypeRegistry.GetId<T>();

        if (HasComponent(typeId)) {
            ulong* newSet = stackalloc ulong[8];

            fixed (ulong* set = _set) {
                Unsafe.CopyBlockUnaligned(newSet, set, 64);
            }

            newSet[typeId / 64] &= ~(1UL << (typeId % 64));

            return new ComponentSet64(newSet, (ushort)(_componentCount - 1));
        }

        fixed (ulong* set = _set) {
            return new ComponentSet64(set, _componentCount);
        }
    }

    public void AddComponent(ushort typeId) {
        _set[typeId / 64] |= 1UL << (typeId % 64);
        _componentCount++;
    }

    public void RemoveComponent(ushort typeId) {
        _set[typeId / 64] &= ~(1UL << (typeId % 64));
        _componentCount--;
    }

    public readonly bool HasComponent(ushort typeId) {
        return (_set[typeId / 64] & (1UL << (typeId % 64))) != 0;
    }

    public readonly ushort[] GetTypeIds() {
        ushort[] ids = new ushort[_componentCount];

        int counter = 0;
        for (int block = 0; block < CompositionSize; block++) {
            ulong value = _set[block];

            while (value != 0) {
                int bit = BitOperations.TrailingZeroCount(value);

                ids[counter++] = (ushort)(block * 64 + bit);

                value &= ~(1UL << bit);
            }
        }

        return ids;
    }

    /// <summary>
    /// Returns true if all components in other are included in this set.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public readonly bool Includes(ComponentSet64 other) {
        if (other._componentCount > _componentCount) {
            return false;
        }

        for (int i = 0; i < CompositionSize; i++) {
            if ((_set[i] & other._set[i]) != other._set[i]) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if no component in other is included in this set.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public readonly bool Excludes(ComponentSet64 other) {
        for (int i = 0; i < CompositionSize; i++) {
            if ((_set[i] & other._set[i]) != 0) {
                return false;
            }
        }

        return true;
    }

    public readonly bool Compare(in ComponentSet64 other) {
        if (other._componentCount != _componentCount) {
            return false;
        }

        fixed (ulong* set = _set) {
            fixed (ulong* setOther = other._set) {
                return LNative.NativeMemcmp(set, setOther, 64) == 0;
            }
        }
    }

    public readonly override int GetHashCode() {
        unchecked {
            int hash = 17;
        
            ulong p0 = _set[0];
            ulong p1 = _set[1];
            ulong p2 = _set[2];
            ulong p3 = _set[3];
            ulong p4 = _set[4];
            ulong p5 = _set[5];
            ulong p6 = _set[6];
            ulong p7 = _set[7];
        
            hash = hash * 31 + (int)(p0 ^ (p0 >> 32));
            hash = hash * 31 + (int)(p1 ^ (p1 >> 32));
            hash = hash * 31 + (int)(p2 ^ (p2 >> 32));
            hash = hash * 31 + (int)(p3 ^ (p3 >> 32));
            hash = hash * 31 + (int)(p4 ^ (p4 >> 32));
            hash = hash * 31 + (int)(p5 ^ (p5 >> 32));
            hash = hash * 31 + (int)(p6 ^ (p6 >> 32));
            hash = hash * 31 + (int)(p7 ^ (p7 >> 32));
        
            return hash;
        }
    }

    public static bool operator ==(in ComponentSet64 left, in ComponentSet64 right) {
        return left.Compare(in right);
    }

    public static bool operator !=(in ComponentSet64 left, in ComponentSet64 right) {
        return !(left == right);
    }
}