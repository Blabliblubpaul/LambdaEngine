using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Queries.ComponentEnumerators;

namespace LambdaEngine.Core.Queries;

public class ComponentEnumerable<T0, T1>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    
    private readonly NativeMemoryManager<T0>[] _components0;
    private readonly NativeMemoryManager<T1>[] _components1;

    private readonly ulong _version;

    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    public long Count { get; }

    internal ComponentEnumerable(EcsWorld world, NativeMemoryManager<int>[] ids, NativeMemoryManager<T0>[] components0,
        NativeMemoryManager<T1>[] components1) {
        _world = world;
        _version = _world._version;
        
        _ids = ids;
        
        _components0 = components0;
        _components1 = components1;

        long count = 0;
        foreach (NativeMemoryManager<int> idBatch in ids) {
            count += idBatch.Memory.Length;
        }

        Count = count;
    }

    public ComponentEnumerator<T0, T1> GetEnumerator() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }

        return new ComponentEnumerator<T0, T1>(_world, _ids, _components0, _components1);
    }
}