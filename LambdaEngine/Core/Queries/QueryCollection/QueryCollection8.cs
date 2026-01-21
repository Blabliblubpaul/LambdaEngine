using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core.Queries.QueryCollection;

public class QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7> : IQueryCollection
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent
    where T3 : unmanaged, IEcsComponent
    where T4 : unmanaged, IEcsComponent
    where T5 : unmanaged, IEcsComponent
    where T6 : unmanaged, IEcsComponent
    where T7 : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    private readonly NativeMemoryManager<T0>[] _components0;
    private readonly NativeMemoryManager<T1>[] _components1;
    private readonly NativeMemoryManager<T2>[] _components2;
    private readonly NativeMemoryManager<T3>[] _components3;
    private readonly NativeMemoryManager<T4>[] _components4;
    private readonly NativeMemoryManager<T5>[] _components5;
    private readonly NativeMemoryManager<T6>[] _components6;
    private readonly NativeMemoryManager<T7>[] _components7;
    private readonly ulong _version;
    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    public long EntityCount { get; }

    private QueryCollection(EcsWorld world,
        NativeMemoryManager<int>[] ids,
        NativeMemoryManager<T0>[] c0,
        NativeMemoryManager<T1>[] c1,
        NativeMemoryManager<T2>[] c2,
        NativeMemoryManager<T3>[] c3,
        NativeMemoryManager<T4>[] c4,
        NativeMemoryManager<T5>[] c5,
        NativeMemoryManager<T6>[] c6,
        NativeMemoryManager<T7>[] c7) {
        _world = world;
        _version = _world._version;
        
        _ids = ids;
        
        _components0 = c0;
        _components1 = c1;
        _components2 = c2;
        _components3 = c3;
        _components4 = c4;
        _components5 = c5;
        _components6 = c6;
        _components7 = c7;

        long count = 0;
        foreach (NativeMemoryManager<int> idChunk in ids) {
            count += idChunk.Memory.Length;
        }
        
        EntityCount = count;
    }

    public ComponentEnumerable<T0, T1, T2, T3, T4, T5, T6, T7> GetComponents() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }

        return new ComponentEnumerable<T0, T1, T2, T3, T4, T5, T6, T7>(_world, _ids,
            _components0, _components1, _components2, _components3,
            _components4, _components5, _components6, _components7);
    }

    // public ComponentEnumerable<T> GetComponents<T>() where T : unmanaged, IEcsComponent {
    //     if (!IsValid) {
    //         throw new InvalidOperationException("This collection is invalid.");
    //     }
    //
    //     if (typeof(T) == typeof(T0)) {
    //         ComponentEnumerable<T0> e = new(_world, _components0);
    //         return Unsafe.As<ComponentEnumerable<T0>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T1)) {
    //         ComponentEnumerable<T1> e = new(_world, _components1);
    //         return Unsafe.As<ComponentEnumerable<T1>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T2)) {
    //         ComponentEnumerable<T2> e = new(_world, _components2);
    //         return Unsafe.As<ComponentEnumerable<T2>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T3)) {
    //         ComponentEnumerable<T3> e = new(_world, _components3);
    //         return Unsafe.As<ComponentEnumerable<T3>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T4)) {
    //         ComponentEnumerable<T4> e = new(_world, _components4);
    //         return Unsafe.As<ComponentEnumerable<T4>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T5)) {
    //         ComponentEnumerable<T5> e = new(_world, _components5);
    //         return Unsafe.As<ComponentEnumerable<T5>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T6)) {
    //         ComponentEnumerable<T6> e = new(_world, _components6);
    //         return Unsafe.As<ComponentEnumerable<T6>, ComponentEnumerable<T>>(ref e);
    //     }
    //
    //     if (typeof(T) == typeof(T7)) {
    //         ComponentEnumerable<T7> e = new(_world, _components7);
    //         return Unsafe.As<ComponentEnumerable<T7>, ComponentEnumerable<T>>(ref e);
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

        public QueryCollectionBuilder(EcsWorld world) {
            _world = world;
        }

        public QueryCollectionBuilder FromArchetype(ArchetypeHandle archetype) {
            _archetypes.Add(archetype);
            return this;
        }

        public QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7> Build() {
            List<NativeMemoryManager<int>> ids = new(_archetypes.Count);
            
            List<NativeMemoryManager<T0>> memory0 = new(_archetypes.Count);
            List<NativeMemoryManager<T1>> memory1 = new(_archetypes.Count);
            List<NativeMemoryManager<T2>> memory2 = new(_archetypes.Count);
            List<NativeMemoryManager<T3>> memory3 = new(_archetypes.Count);
            List<NativeMemoryManager<T4>> memory4 = new(_archetypes.Count);
            List<NativeMemoryManager<T5>> memory5 = new(_archetypes.Count);
            List<NativeMemoryManager<T6>> memory6 = new(_archetypes.Count);
            List<NativeMemoryManager<T7>> memory7 = new(_archetypes.Count);

            foreach (ArchetypeHandle handle in _archetypes) {
                ids.AddRange(handle.GetIds());
                
                memory0.AddRange(handle.GetComponents<T0>());
                memory1.AddRange(handle.GetComponents<T1>());
                memory2.AddRange(handle.GetComponents<T2>());
                memory3.AddRange(handle.GetComponents<T3>());
                memory4.AddRange(handle.GetComponents<T4>());
                memory5.AddRange(handle.GetComponents<T5>());
                memory6.AddRange(handle.GetComponents<T6>());
                memory7.AddRange(handle.GetComponents<T7>());
            }

            return new QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7>(_world, ids.ToArray(),
                memory0.ToArray(), memory1.ToArray(), memory2.ToArray(), memory3.ToArray(),
                memory4.ToArray(), memory5.ToArray(), memory6.ToArray(), memory7.ToArray());
        }
    }
}