using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Queries.ComponentRef;

namespace LambdaEngine.Core.Queries.ComponentEnumerators;

// TODO: Add Id collections
// TODO: Use raw pointers instead of memory managers/memory.
public ref struct ComponentEnumerator<T0> where T0 : unmanaged, IEcsComponent {
    private readonly Memory<int>[] _ids;
    private readonly Memory<T0>[] _components0;
    private readonly ulong _version;

    private bool _isAtEnd;
    
    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    /// <summary>
    /// Returns a <see cref="ComponentRef{T0}"/> for the index to which the enumerator is currently pointing.
    /// <remarks>Before accessing <see cref="Current"/> for the first time, <see cref="MoveNext"/> has to be called at least once.</remarks>
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an exception if the enumerator is in an invalid state or has become invalid.</exception>
    public ComponentRef<T0> Current {
        get {
            if (_componentIndex == -1) {
                throw new InvalidOperationException(
                    "Enumerator is in an invalid state. Call MoveNext() before accessing Current for the first time.");
            }
            if (IsValid && !_isAtEnd) {
                return new ComponentRef<T0>(_ids[_arrayIndex].Span[_componentIndex], ref _components0[_arrayIndex].Span[_componentIndex]);
            }

            throw new InvalidOperationException("Enumerator is invalid.");
        }
    }
    

    private int _arrayIndex = 0;
    private int _componentIndex = -1;
    
    internal ComponentEnumerator(EcsWorld world, NativeMemoryManager<int>[] ids, NativeMemoryManager<T0>[] components0) {
        _world = world;
        _version = _world._version;
        
        _ids = new Memory<int>[ids.Length];
        
        _components0 = new Memory<T0>[components0.Length];
        
        for (int i = 0; i < ids.Length; i++) {
            _ids[i] = ids[i].Memory;
            _components0[i] = components0[i].Memory;
        }
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