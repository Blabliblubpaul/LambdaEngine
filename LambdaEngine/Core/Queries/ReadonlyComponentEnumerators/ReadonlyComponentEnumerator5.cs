using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Queries.ReadonlyComponentRef;

namespace LambdaEngine.Core.Queries.ReadonlyComponentEnumerators;

public ref struct ReadonlyComponentEnumerator<T0, T1, T2, T3, T4>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent
    where T3 : unmanaged, IEcsComponent
    where T4 : unmanaged, IEcsComponent {
    private readonly Memory<int>[] _ids;
    
    private readonly ReadOnlyMemory<T0>[] _components0;
    private readonly ReadOnlyMemory<T1>[] _components1;
    private readonly ReadOnlyMemory<T2>[] _components2;
    private readonly ReadOnlyMemory<T3>[] _components3;
    private readonly ReadOnlyMemory<T4>[] _components4;
    private readonly ulong _version;

    private bool _isAtEnd;
    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    /// <summary>
    /// Returns a <see cref="ReadonlyComponentRef{T0,T1,T2,T3,T4}"/> for the index to which the enumerator is currently pointing.
    /// <remarks>Before accessing <see cref="Current"/> for the first time, <see cref="MoveNext"/> has to be called at least once.</remarks>
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an exception if the enumerator is in an invalid state or has become invalid.</exception>
    public ReadonlyComponentRef<T0, T1, T2, T3, T4> Current {
        get {
            if (_componentIndex == -1) {
                throw new InvalidOperationException(
                    "Enumerator is in an invalid state. Call MoveNext() before accessing Current for the first time.");
            }
            if (IsValid && !_isAtEnd) {
                return new ReadonlyComponentRef<T0, T1, T2, T3, T4>(
                    _ids[_arrayIndex].Span[_componentIndex],
                    in _components0[_arrayIndex].Span[_componentIndex],
                    in _components1[_arrayIndex].Span[_componentIndex],
                    in _components2[_arrayIndex].Span[_componentIndex],
                    in _components3[_arrayIndex].Span[_componentIndex],
                    in _components4[_arrayIndex].Span[_componentIndex]);
            }

            throw new InvalidOperationException("Enumerator is invalid.");
        }
    }

    private int _arrayIndex;
    private int _componentIndex;

    internal ReadonlyComponentEnumerator(EcsWorld world,
        NativeMemoryManager<int>[] ids,
        NativeMemoryManager<T0>[] components0,
        NativeMemoryManager<T1>[] components1,
        NativeMemoryManager<T2>[] components2,
        NativeMemoryManager<T3>[] components3,
        NativeMemoryManager<T4>[] components4) {
        _world = world;
        _version = _world._version;
        
        _ids = new Memory<int>[ids.Length];

        _components0 = new ReadOnlyMemory<T0>[components0.Length];
        _components1 = new ReadOnlyMemory<T1>[components1.Length];
        _components2 = new ReadOnlyMemory<T2>[components2.Length];
        _components3 = new ReadOnlyMemory<T3>[components3.Length];
        _components4 = new ReadOnlyMemory<T4>[components4.Length];

        for (int i = 0; i < ids.Length; i++) {
            _ids[i] = ids[i].Memory;
            
            _components0[i] = components0[i].Memory;
            _components1[i] = components1[i].Memory;
            _components2[i] = components2[i].Memory;
            _components3[i] = components3[i].Memory;
            _components4[i] = components4[i].Memory;
        }

        _arrayIndex = 0;
        _componentIndex = -1;
    }

    public bool MoveNext() {
        if (_components0.Length == 0) {
            return false;
        }

        _componentIndex++;

        while (_components0[_arrayIndex].Length == _componentIndex) {
            _arrayIndex++;
            if (_components0.Length == _arrayIndex) {
                _isAtEnd = true;
                return false;
            }

            _componentIndex = 0;
        }

        return true;
    }
    
    /// <summary>
    ///  Resets the enumerator to its default state.
    /// </summary>
    public void Reset() {
        _arrayIndex = 0;
        _componentIndex = -1;
        
        _isAtEnd = false;
    }
}