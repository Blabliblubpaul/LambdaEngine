using LambdaEngine.Core.ArchetypeComposition;
using LambdaEngine.Core.Common;

namespace LambdaEngine.Core.Queries;

// TODO: Add support for optional components in a query.
// -> Nullable (?) or custom wrapper with .Exists property?
// TODO: Add component groups as shallow wrappers over multiple components, including optional components in a group.
// TODO: Pass components as types params with params Type[], allow component groups.
// -> Allows arbitrary amounts of components per query.
public partial class EcsQuery : IEcsQuery {
    private readonly ComponentSet64 _include;
    private readonly ComponentSet64 _exclude;

    private readonly EcsWorld _world;

    private EcsQuery(EcsWorld world, ComponentSet64 include, ComponentSet64 exclude) {
        _world = world;

        _include = include;
        _exclude = exclude;
    }

    internal bool MatchesArchetype(ArchetypeComposition64 composition) {
        return composition.Includes(_include) && composition.Excludes(_exclude);
    }

    public static QueryBuilder Create(EcsWorld world) {
        return new QueryBuilder(world);
    }

    public class QueryBuilder {
        private readonly List<ushort> _include = [];
        private readonly List<ushort> _exclude = [];

        private readonly EcsWorld _world;

        internal QueryBuilder(EcsWorld world) {
            _world = world;
        }

        public QueryBuilder Include<T>() where T : unmanaged, IEcsComponent {
            _include.Add(ComponentTypeRegistry.GetId<T>());

            return this;
        }

        public QueryBuilder Exclude<T>() where T : unmanaged, IEcsComponent {
            _exclude.Add(ComponentTypeRegistry.GetId<T>());

            return this;
        }

        public EcsQuery Build() {
            ComponentSet64 include = new();
            ComponentSet64 exclude = new();

            foreach (ushort type in _include) {
                include.AddComponent(type);
            }

            foreach (ushort type in _exclude) {
                exclude.AddComponent(type);
            }

            EcsQuery query = new(_world, include, exclude);

            return query;
        }
    }
}