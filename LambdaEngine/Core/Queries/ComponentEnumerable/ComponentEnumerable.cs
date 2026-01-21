using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Queries.ComponentEnumerators;

namespace LambdaEngine.Core.Queries;

public class ComponentEnumerable<T> where T : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    
    private readonly NativeMemoryManager<T>[] _components;

    private readonly ulong _version;

    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }
    
    public long Count { get; init; }

    internal ComponentEnumerable(EcsWorld world, NativeMemoryManager<int>[] ids, NativeMemoryManager<T>[] components) {
        _world = world;
        _version = _world._version;

        _ids = ids;
        
        _components = components;

        long count = 0;
        foreach (NativeMemoryManager<int> idBatch in ids) {
            count += idBatch.Memory.Length;
        }
        
        Count = count;
    }
    
    public ComponentEnumerator<T> GetEnumerator() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }
        
        return new ComponentEnumerator<T>(_world, _ids, _components);
    }
}
