using LambdaEngine.Core.ArchetypeComposition;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ReadonlyQueryCollection;

namespace LambdaEngine.Core;

public sealed partial class EcsWorld {
    #region Queries

    internal ReadonlyQueryCollection<T0> ReadonlyExecuteQuery<T0>(ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0>.ReadonlyQueryCollectionBuilder builder = ReadonlyQueryCollection<T0>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1> ReadonlyExecuteQuery<T0, T1>(ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2> ReadonlyExecuteQuery<T0, T1, T2>(ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2, T3> ReadonlyExecuteQuery<T0, T1, T2, T3>(ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2, T3>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2, T3>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2, T3, T4> ReadonlyExecuteQuery<T0, T1, T2, T3, T4>(
        ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2, T3, T4>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2, T3, T4>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5> ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5>(
        ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6> ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5, T6>(
        ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6, T7> ReadonlyExecuteQuery<T0, T1, T2, T3, T4, T5, T6,
        T7>(
        ReadonlyEcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent
        where T7 : unmanaged, IEcsComponent {
        ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6, T7>.ReadonlyQueryCollectionBuilder builder =
            ReadonlyQueryCollection<T0, T1, T2, T3, T4, T5, T6, T7>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    #endregion
}