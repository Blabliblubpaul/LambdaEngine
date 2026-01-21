using LambdaEngine.Core.ArchetypeComposition;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.QueryCollection;

namespace LambdaEngine.Core;

public sealed partial class EcsWorld {
    #region Queries

    internal QueryCollection<T0> ExecuteQuery<T0>(EcsQuery query) where T0 : unmanaged, IEcsComponent {
        QueryCollection<T0>.QueryCollectionBuilder builder = QueryCollection<T0>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1> ExecuteQuery<T0, T1>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1>.QueryCollectionBuilder builder = QueryCollection<T0, T1>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2> ExecuteQuery<T0, T1, T2>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2>.QueryCollectionBuilder builder = QueryCollection<T0, T1, T2>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2, T3> ExecuteQuery<T0, T1, T2, T3>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2, T3>.QueryCollectionBuilder builder = QueryCollection<T0, T1, T2, T3>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2, T3, T4> ExecuteQuery<T0, T1, T2, T3, T4>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2, T3, T4>.QueryCollectionBuilder builder =
            QueryCollection<T0, T1, T2, T3, T4>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2, T3, T4, T5> ExecuteQuery<T0, T1, T2, T3, T4, T5>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2, T3, T4, T5>.QueryCollectionBuilder builder =
            QueryCollection<T0, T1, T2, T3, T4, T5>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2, T3, T4, T5, T6> ExecuteQuery<T0, T1, T2, T3, T4, T5, T6>(EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2, T3, T4, T5, T6>.QueryCollectionBuilder builder =
            QueryCollection<T0, T1, T2, T3, T4, T5, T6>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    internal QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7> ExecuteQuery<T0, T1, T2, T3, T4, T5, T6, T7>(
        EcsQuery query)
        where T0 : unmanaged, IEcsComponent
        where T1 : unmanaged, IEcsComponent
        where T2 : unmanaged, IEcsComponent
        where T3 : unmanaged, IEcsComponent
        where T4 : unmanaged, IEcsComponent
        where T5 : unmanaged, IEcsComponent
        where T6 : unmanaged, IEcsComponent
        where T7 : unmanaged, IEcsComponent {
        QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7>.QueryCollectionBuilder builder =
            QueryCollection<T0, T1, T2, T3, T4, T5, T6, T7>.Create(this);

        foreach (ArchetypeComposition64 composition in _globalArchetypes.Keys) {
            if (query.MatchesArchetype(composition)) {
                builder.FromArchetype(_globalArchetypes[composition]);
            }
        }

        return builder.Build();
    }

    #endregion
}