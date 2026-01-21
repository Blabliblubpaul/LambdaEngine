using LambdaEngine.Core.Common;

namespace LambdaEngine.Core.ArchetypeComposition;

/// <summary>
/// 512 component types
/// </summary>
internal readonly struct ArchetypeComposition64 : IEquatable<ArchetypeComposition64> {
    private readonly ComponentSet64 _types;
    public readonly ushort ComponentCount = 0;

    public ArchetypeComposition64(params ushort[] typeIds) {
        foreach (ushort type in typeIds) {
            _types.AddComponent(type);
        
            ComponentCount++;
        }
    }
    
    // TODO: Dont allow this anymore
    public ArchetypeComposition64(params Type[] types) {
        foreach (Type type in types) {
            _types.AddComponent(ComponentTypeRegistry.GetId(type));
        
            ComponentCount++;
        }
    }

    private ArchetypeComposition64(ComponentSet64 types) {
        _types = types;
        ComponentCount = types.ComponentCount;
    }

    public readonly ArchetypeComposition64 With<T>() where T : unmanaged, IEcsComponent {
        return new ArchetypeComposition64(_types.With<T>());
    }
    
    public ArchetypeComposition64 Without<T>() where T : unmanaged, IEcsComponent {
        return new ArchetypeComposition64(_types.Without<T>());
    }

    public bool Has(ushort typeId) {
        return _types.HasComponent(typeId);
    }

    /// <summary>
    /// Returns true if all components in other are included in this composition.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Includes(ComponentSet64 other) {
        return _types.Includes(other);
    }
    
    /// <summary>
    /// Returns true if no component in other is included in this set.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Excludes(ComponentSet64 other) {
        return _types.Excludes(other);
    }
    
    public ushort[] GetTypeIds() {
        return _types.GetTypeIds();
    }
    
    public readonly bool Compare(in ArchetypeComposition64 other) {
        return _types.Compare(other._types);
    }
    
    public readonly override bool Equals(object? obj) {
        return obj is ArchetypeComposition64 composition64 && Equals(composition64);
    }

    public readonly bool Equals(ArchetypeComposition64 other) {
        return Compare(other);
    }

    public readonly override int GetHashCode() {
        return _types.GetHashCode();
    }
}
