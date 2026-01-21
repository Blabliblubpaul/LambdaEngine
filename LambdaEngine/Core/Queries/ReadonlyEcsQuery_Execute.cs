using LambdaEngine.Core.Queries.ReadonlyQueryCollection;
using LambdaEngine.Core.Queries.QueryCollection;

namespace LambdaEngine.Core.Queries;

public partial class ReadonlyEcsQuery {
    public ReadonlyQueryCollection<T0> Execute<T0>() where T0 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();

        if (!_include.HasComponent(t0Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0>(this);
    }

    public ReadonlyQueryCollection<T0, T1> Execute<T0, T1>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2> Execute<T0, T1, T2>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2, T3> Execute<T0, T1, T2, T3>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();
        ushort t3Id = ComponentTypeRegistry.GetId<T3>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id) ||
            !_include.HasComponent(t3Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2, T3>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2, T3, T4> Execute<T0, T1, T2, T3, T4>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();
        ushort t3Id = ComponentTypeRegistry.GetId<T3>();
        ushort t4Id = ComponentTypeRegistry.GetId<T4>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id) ||
            !_include.HasComponent(t3Id) || !_include.HasComponent(t4Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2, T3, T4>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5> Execute<T0, T1, T2, T3, T4, T5>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();
        ushort t3Id = ComponentTypeRegistry.GetId<T3>();
        ushort t4Id = ComponentTypeRegistry.GetId<T4>();
        ushort t5Id = ComponentTypeRegistry.GetId<T5>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id) ||
            !_include.HasComponent(t3Id) || !_include.HasComponent(t4Id) || !_include.HasComponent(t5Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6> Execute<T0, T1, T2, T3, T4, T5, T6>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();
        ushort t3Id = ComponentTypeRegistry.GetId<T3>();
        ushort t4Id = ComponentTypeRegistry.GetId<T4>();
        ushort t5Id = ComponentTypeRegistry.GetId<T5>();
        ushort t6Id = ComponentTypeRegistry.GetId<T6>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id) ||
            !_include.HasComponent(t3Id) || !_include.HasComponent(t4Id) || !_include.HasComponent(t5Id) ||
            !_include.HasComponent(t6Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5, T6>(this);
    }

    public ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6, T7> Execute<T0, T1, T2, T3, T4, T5, T6, T7>()
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent
        where T7 : unmanaged, IEcsComponent {
        ushort t0Id = ComponentTypeRegistry.GetId<T0>();
        ushort t1Id = ComponentTypeRegistry.GetId<T1>();
        ushort t2Id = ComponentTypeRegistry.GetId<T2>();
        ushort t3Id = ComponentTypeRegistry.GetId<T3>();
        ushort t4Id = ComponentTypeRegistry.GetId<T4>();
        ushort t5Id = ComponentTypeRegistry.GetId<T5>();
        ushort t6Id = ComponentTypeRegistry.GetId<T6>();
        ushort t7Id = ComponentTypeRegistry.GetId<T7>();

        if (!_include.HasComponent(t0Id) || !_include.HasComponent(t1Id) || !_include.HasComponent(t2Id) ||
            !_include.HasComponent(t3Id) || !_include.HasComponent(t4Id) || !_include.HasComponent(t5Id) ||
            !_include.HasComponent(t6Id) || !_include.HasComponent(t7Id)) {
            throw new InvalidOperationException("Invalid query: all component types must be included.");
        }

        return _world. ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5, T6, T7>(this);
    }
}