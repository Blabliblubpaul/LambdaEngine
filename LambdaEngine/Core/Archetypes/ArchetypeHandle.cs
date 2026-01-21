using LambdaEngine.Core.ArchetypeComposition;

namespace LambdaEngine.Core.Archetypes;

public class ArchetypeHandle {
    private readonly Archetype _archetype;

    internal ArchetypeHandle(Archetype archetype) {
        _archetype = archetype;
    }
    
    internal ArchetypeComposition64 Composition {
        get => _archetype.Composition;
    }

    public Archetype Get() {
        return _archetype;
    }

    internal bool HasComponent<T>() where T : unmanaged, IEcsComponent {
        return _archetype.HasComponent<T>();
    }

    internal void InsertDefaultEntity(int entity) {
        _archetype.InsertDefaultEntity(entity);
    }

    internal void SetComponent<T>(int entity, T component) where T : unmanaged, IEcsComponent {
        _archetype.SetComponent(entity, component);
    }

    internal ref T GetComponent<T>(int entity) where T : unmanaged, IEcsComponent {
        return ref _archetype.GetComponent<T>(entity);
    }

    internal void MigrateEntityToArchetype(int entity, Archetype archetype) {
        _archetype.MigrateEntityToArchetype(entity, archetype);
    }

    internal void DestroyEntityComponents(int entity) {
        _archetype.DestroyEntityComponents(entity);
    }

    internal NativeMemoryManager<T>[] GetComponents<T>() where T : unmanaged, IEcsComponent {
        return _archetype.GetComponents<T>();
    }

    internal IEnumerable<NativeMemoryManager<int>> GetIds() {
        return _archetype.GetIds();
    }
}