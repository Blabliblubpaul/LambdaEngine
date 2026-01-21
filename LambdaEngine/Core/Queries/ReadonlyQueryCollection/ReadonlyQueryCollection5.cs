using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core.Queries.ReadonlyQueryCollection;

public class ReadonlyQueryCollection<T0, T1, T2, T3, T4> : IQueryCollection
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent
    where T3 : unmanaged, IEcsComponent
    where T4 : unmanaged, IEcsComponent {
    private readonly NativeMemoryManager<int>[] _ids;
    private readonly NativeMemoryManager<T0>[] _components0;
    private readonly NativeMemoryManager<T1>[] _components1;
    private readonly NativeMemoryManager<T2>[] _components2;
    private readonly NativeMemoryManager<T3>[] _components3;
    private readonly NativeMemoryManager<T4>[] _components4;
    private readonly ulong _version;
    private readonly EcsWorld _world;

    public bool IsValid {
        get => _world._version == _version;
    }

    public long EntityCount { get; }

    private ReadonlyQueryCollection(EcsWorld world,
        NativeMemoryManager<int>[] ids,
        NativeMemoryManager<T0>[] c0,
        NativeMemoryManager<T1>[] c1,
        NativeMemoryManager<T2>[] c2,
        NativeMemoryManager<T3>[] c3,
        NativeMemoryManager<T4>[] c4) {
        _world = world;
        _version = _world._version;
        
        _ids = ids;
        
        _components0 = c0;
        _components1 = c1;
        _components2 = c2;
        _components3 = c3;
        _components4 = c4;

        long count = 0;
        foreach (NativeMemoryManager<int> idChunk in ids) {
            count += idChunk.Memory.Length;
        }
        
        EntityCount = count;
    }

    public ComponentEnumerable<T0, T1, T2, T3, T4> GetComponents() {
        if (!IsValid) {
            throw new InvalidOperationException("This collection is invalid.");
        }

        return new ComponentEnumerable<T0, T1, T2, T3, T4>(_world, _ids, _components0, _components1, _components2,
            _components3, _components4);
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
    //     throw new ArgumentOutOfRangeException(nameof(T));
    // }

    internal static ReadonlyQueryCollectionBuilder Create(EcsWorld world) {
        return new ReadonlyQueryCollectionBuilder(world);
    }

    internal class ReadonlyQueryCollectionBuilder {
        private readonly List<ArchetypeHandle> _archetypes = new(16);
        private readonly EcsWorld _world;

        public ReadonlyQueryCollectionBuilder(EcsWorld world) {
            _world = world;
        }

        public ReadonlyQueryCollectionBuilder FromArchetype(ArchetypeHandle archetype) {
            _archetypes.Add(archetype);
            return this;
        }

        public ReadonlyQueryCollection<T0, T1, T2, T3, T4> Build() {
            List<NativeMemoryManager<int>> ids = new(_archetypes.Count);
            
            List<NativeMemoryManager<T0>> memory0 = new(_archetypes.Count);
            List<NativeMemoryManager<T1>> memory1 = new(_archetypes.Count);
            List<NativeMemoryManager<T2>> memory2 = new(_archetypes.Count);
            List<NativeMemoryManager<T3>> memory3 = new(_archetypes.Count);
            List<NativeMemoryManager<T4>> memory4 = new(_archetypes.Count);

            foreach (ArchetypeHandle handle in _archetypes) {
                ids.AddRange(handle.GetIds());
                
                memory0.AddRange(handle.GetComponents<T0>());
                memory1.AddRange(handle.GetComponents<T1>());
                memory2.AddRange(handle.GetComponents<T2>());
                memory3.AddRange(handle.GetComponents<T3>());
                memory4.AddRange(handle.GetComponents<T4>());
            }

            return new ReadonlyQueryCollection<T0, T1, T2, T3, T4>(_world, ids.ToArray(), memory0.ToArray(), 
                memory1.ToArray(), memory2.ToArray(), memory3.ToArray(), memory4.ToArray());
        }
    }
}