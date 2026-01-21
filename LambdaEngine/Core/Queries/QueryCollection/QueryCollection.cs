using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core.Queries.QueryCollection;

public class QueryCollection<T0> : IQueryCollection where T0 : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    private readonly NativeMemoryManager<T0>[] _components;
    private readonly ulong _version;

    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }
    
    public long EntityCount { get; }

    private QueryCollection(EcsWorld world, NativeMemoryManager<int>[] ids, NativeMemoryManager<T0>[] memory) {
        _world = world;
        _version = _world._version;
        
        _ids = ids;
        
        _components = memory;

        long count = 0;
        foreach (NativeMemoryManager<int> idChunk in ids) {
            count += idChunk.Memory.Length;
        }
        
        EntityCount = count;
    }

    public ComponentEnumerable<T0> GetComponents() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }
        
        return  new ComponentEnumerable<T0>(_world, _ids, _components);
    }

    internal static QueryCollectionBuilder Create(EcsWorld world) {
        return new QueryCollectionBuilder(world);
    }
    
    internal class QueryCollectionBuilder {
        private readonly List<ArchetypeHandle> _archetypes = new(16);
        
        private readonly EcsWorld _world;

        public QueryCollectionBuilder(EcsWorld world) {
            _world = world;
        }

        public QueryCollectionBuilder FromArchetype(ArchetypeHandle archetype) {
            _archetypes.Add(archetype);

            return this;
        }

        public QueryCollection<T0> Build() {
            List<NativeMemoryManager<int>> ids = new(_archetypes.Count);
            
            List<NativeMemoryManager<T0>> memory = new(_archetypes.Count);

            foreach (ArchetypeHandle handle in _archetypes) {
                ids.AddRange(handle.GetIds());
                
                memory.AddRange(handle.GetComponents<T0>());
            }

            return new QueryCollection<T0>(_world, ids.ToArray(), memory.ToArray());
        }
    }
}
