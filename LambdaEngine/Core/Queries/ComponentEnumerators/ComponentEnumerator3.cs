using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Queries.ComponentRef;

namespace LambdaEngine.Core.Queries.ComponentEnumerators;

// TODO: Allow direct indexing into QueryCollections
// TODO: Directly store QueryCollections in the enumerator
public ref struct ComponentEnumerator<T0, T1, T2>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent {
    private readonly Memory<int>[] _ids;
    
    private readonly Memory<T0>[] _components0;
    private readonly Memory<T1>[] _components1;
    private readonly Memory<T2>[] _components2;
    private readonly ulong _version;

    private bool _isAtEnd;

    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    /// <summary>
    /// Returns a <see cref="ComponentRef{T0,T1,T2}"/> for the index to which the enumerator is currently pointing.
    /// <remarks>Before accessing <see cref="Current"/> for the first time, <see cref="MoveNext"/> has to be called at least once.</remarks>
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an exception if the enumerator is in an invalid state or has become invalid.</exception>
    public ComponentRef<T0, T1, T2> Current {
        get {
            if (_componentIndex == -1) {
                throw new InvalidOperationException(
                    "Enumerator is in an invalid state. Call MoveNext() before accessing Current for the first time.");
            }
            if (IsValid && !_isAtEnd) {
                return new ComponentRef<T0, T1, T2>(
                    _ids[_arrayIndex].Span[_componentIndex],
                    ref _components0[_arrayIndex].Span[_componentIndex],
                    ref _components1[_arrayIndex].Span[_componentIndex],
                    ref _components2[_arrayIndex].Span[_componentIndex]);
            }

            throw new InvalidOperationException("Enumerator is invalid.");
        }
    }

    private int _arrayIndex;
    private int _componentIndex;

    internal ComponentEnumerator(EcsWorld world,
        NativeMemoryManager<int>[] ids,
        NativeMemoryManager<T0>[] components0,
        NativeMemoryManager<T1>[] components1,
        NativeMemoryManager<T2>[] components2) {
        _world = world;
        _version = _world._version;
        
        _ids = new Memory<int>[ids.Length];

        _components0 = new Memory<T0>[components0.Length];
        _components1 = new Memory<T1>[components1.Length];
        _components2 = new Memory<T2>[components2.Length];

        for (int i = 0; i < ids.Length; i++) {
            _ids[i] = ids[i].Memory;
            
            _components0[i] = components0[i].Memory;
            _components1[i] = components1[i].Memory;
            _components2[i] = components2[i].Memory;
        }

        _arrayIndex = 0;
        _componentIndex = -1;
    }

    public bool MoveNext() {
        if (_components0.Length == 0) {
            return false;
        }

        _componentIndex++;

        // Both component arrays must have the same per-chunk count for matching archetypes
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