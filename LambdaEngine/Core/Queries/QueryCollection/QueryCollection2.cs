using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core.Queries.QueryCollection;

public class QueryCollection<T0, T1> : IQueryCollection
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    private readonly NativeMemoryManager<T0>[] _components0;
    private readonly NativeMemoryManager<T1>[] _components1;
    private readonly ulong _version;
    private readonly EcsWorld _world;

    public bool IsValid => _world._version == _version;

    public long EntityCount { get; }

    private QueryCollection(EcsWorld world, NativeMemoryManager<int>[] ids, NativeMemoryManager<T0>[] c0, NativeMemoryManager<T1>[] c1) {
        _world = world;
        _version = _world._version;
        
        _ids = ids;
        
        _components0 = c0;
        _components1 = c1;

        long count = 0;
        foreach (NativeMemoryManager<int> idChunk in ids) {
            count += idChunk.Memory.Length;
        }
        
        EntityCount = count;
    }

    public ComponentEnumerable<T0, T1> GetComponents() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }
        
        return new ComponentEnumerable<T0, T1>(_world, _ids, _components0, _components1);
    }

    // public ComponentEnumerable<T> GetComponents<T>() where T : unmanaged, IEcsComponent {
    //     if (!IsValid) {
    //         throw new InvalidOperationException("This collection is invalid.");
    //     }
    //     
    //     if (typeof(T) == typeof(T0)) {
    //         ComponentEnumerable<T0> e = new(_world, null, _components0);
    //         return Unsafe.As<ComponentEnumerable<T0>, ComponentEnumerable<T>>(ref e);
    //     }
    //     
    //     if (typeof(T) == typeof(T1)) {
    //         ComponentEnumerable<T1> e = new(_world, null, _components1);
    //         return Unsafe.As<ComponentEnumerable<T1>, ComponentEnumerable<T>>(ref e);
    //     }
    //     
    //     throw new ArgumentOutOfRangeException(nameof(T));
    // }

    internal static QueryCollectionBuilder Create(EcsWorld world) {
        return new QueryCollectionBuilder(world);
    }

    internal class QueryCollectionBuilder {
        private readonly List<ArchetypeHandle> _archetypes = new(16);
        private readonly EcsWorld _world;

        public QueryCollectionBuilder(EcsWorld world) { _world = world; }

        public QueryCollectionBuilder FromArchetype(ArchetypeHandle archetype) {
            _archetypes.Add(archetype);
            return this;
        }

        public QueryCollection<T0, T1> Build() {
            List<NativeMemoryManager<int>> ids = new(_archetypes.Count);
            
            List<NativeMemoryManager<T0>> memory0 = new(_archetypes.Count);
            List<NativeMemoryManager<T1>> memory1 = new(_archetypes.Count);

            foreach (ArchetypeHandle handle in _archetypes) {
                ids.AddRange(handle.GetIds());
                
                memory0.AddRange(handle.GetComponents<T0>());
                memory1.AddRange(handle.GetComponents<T1>());
            }

            return new QueryCollection<T0, T1>(_world, ids.ToArray(), memory0.ToArray(), memory1.ToArray());
        }
    }
}