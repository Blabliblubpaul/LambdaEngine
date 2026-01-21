using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LambdaEngine.Core.Allocators;
using LambdaEngine.Core.ArchetypeComposition;
using LambdaEngine.Core.Archetypes;
using LambdaEngine.Core.Attributes;
using LambdaEngine.Core.Components;
using LambdaEngine.Core.Queries;
using LambdaEngine.Debug;

namespace LambdaEngine.Core;

// TODO: Use GC.AddMemoryPressure
// TODO: Allow default initialization of components
public sealed partial class EcsWorld : IDisposable {
    private const int INITIAL_ENTITY_CAPACITY = 1000000;

    private readonly Dictionary<int, ArchetypeHandle> _entityToArchetype = new();

    // TODO: Archetype handle in composition, only a sorted list of compositions
    private readonly Dictionary<ArchetypeComposition64, ArchetypeHandle> _globalArchetypes = new();
    private int _archetypeCount = 0;

    private readonly Archetype[] _archetypes;

    private readonly bool _initialized;
    
    private EcsQuery _destructionQueueQuery;
    private HashSet<int> _destructionQueue = new(128);

    internal readonly ChunkAllocator ChunkAllocator;
    internal readonly ArchetypeMetadataSlabAllocator ArchetypeMetadataSlabAllocator;
    internal readonly EntityManager EntityManager;

    internal ulong _version;

    internal ArchetypeCompositionSize ArchetypeCompositionSize { get; private set; }

    #region Init

    private EcsWorld(nuint initBufferSize, Assembly[] assemblies, ArchetypeCompositionSize archetypeCompositionSize) {
        if (_initialized) {
            throw new InvalidOperationException("EcsWorld has already been initialized.");
        }

        ChunkAllocator = new ChunkAllocator(initBufferSize);
        ArchetypeMetadataSlabAllocator = new ArchetypeMetadataSlabAllocator();
        EntityManager = new EntityManager(INITIAL_ENTITY_CAPACITY);

        _archetypes = new Archetype[1024];
        // _archetypes = (Archetype*)NativeMemory.AlignedAlloc(1024 * Archetype.SIZE, 128);

        ArchetypeCompositionSize = archetypeCompositionSize;

        RegisterComponents(assemblies);
        
        _destructionQueueQuery = EcsQuery.Create(this).Include<EntityDestructionTagComponent>().Build();

        _initialized = true;
    }

    private void RegisterComponents(Assembly[] assemblies) {
        LDebug.Log("Registering Ecs Components...");
        foreach (Assembly assembly in assemblies) {
            LDebug.Log($"Registering components from assembly '{assembly.GetName().Name}'...", LogLevel.DEBUG);
            
            foreach (Type type in assembly.GetTypes()) {
                if (type.IsValueType && typeof(IEcsComponent).IsAssignableFrom(type) &&
                    type.GetCustomAttributes(typeof(EcsComponentAttribute), false).Length > 0) {
                    LDebug.Log($"Registering ecs component {type.Name}", LogLevel.DEBUG);

                    int size = Marshal.SizeOf(type);

                    if (!LMath.IsPowerOfTwo(size)) {
                        LDebug.Log($"Warning: Component {type.Name} has a size that is not a power of two.", LogLevel.WARNING);
                    }

                    if (!Attribute.IsDefined(type, typeof(LargeComponentAttribute)) && size > 64) {
                        LDebug.Log($"Warning: Component {type.Name} has a size that is larger than 64 bytes.", LogLevel.WARNING);
                    }
                    else if (!(Attribute.IsDefined(type, typeof(LargeComponentAttribute)) ||
                               Attribute.IsDefined(type, typeof(MediumComponentAttribute))) && size > 32) {
                        LDebug.Log($"Warning: Component {type.Name} has a size that is larger than 32 bytes.", LogLevel.WARNING);
                    }

                    Type generic = typeof(ComponentTypeId<>).MakeGenericType(type);
                    RuntimeHelpers.RunClassConstructor(generic.TypeHandle);
                }
            }
        }
    }

    #endregion

    private static ArchetypeComposition64 GetComposition(Type[] types) {
        ushort[] typeIds = types.Select(ComponentTypeRegistry.GetId).ToArray();

        return new ArchetypeComposition64(typeIds);
    }

    #region Interaction

    public ArchetypeHandle GetOrCreateArchetype(params Type[] componentTypes) {
        ArchetypeComposition64 composition = GetComposition(componentTypes);

        if (!_globalArchetypes.TryGetValue(composition, out ArchetypeHandle? handle)) {
            handle = CreateArchetype(ref composition);

            _globalArchetypes[composition] = handle;
        }

        return handle;
    }

    public int CreateEntity(params Type[] componentTypes) {
        _version++;

        int id = EntityManager.NextId();

        if (componentTypes.Length > 0) {
            ArchetypeComposition64 composition = GetComposition(componentTypes);

            if (!_globalArchetypes.TryGetValue(composition, out ArchetypeHandle? handle)) {
                handle = CreateArchetype(componentTypes);
                _globalArchetypes[composition] = handle;
            }

            handle.InsertDefaultEntity(id);
            _entityToArchetype[id] = handle;
        }

        return id;
    }

    private ArchetypeHandle CreateArchetype(Type[] types) {
        _archetypes[_archetypeCount] = new Archetype(this, types);

        // return new ArchetypeHandle(_archetypes + _archetypeCount++);
        return new ArchetypeHandle(_archetypes[_archetypeCount++]);
    }

    private ArchetypeHandle CreateArchetype(ref ArchetypeComposition64 comp64) {
        _archetypes[_archetypeCount] = new Archetype(this, ref comp64);

        // return new ArchetypeHandle(_archetypes + _archetypeCount++);
        return new ArchetypeHandle(_archetypes[_archetypeCount++]);
    }

    public int CreateEntity(ArchetypeHandle handle) {
        _version++;

        int id = EntityManager.NextId();

        handle.InsertDefaultEntity(id);
        _entityToArchetype[id] = handle;

        return id;
    }

    public void SetComponent<[EnforceEcsComponent] T>(int entity, T component) where T : unmanaged, IEcsComponent {
        _entityToArchetype[entity].SetComponent(entity, component);
    }

    public bool HasComponent<[EnforceEcsComponent] T>(int entity) where T : unmanaged, IEcsComponent {
        return _entityToArchetype[entity].HasComponent<T>();
    }

    public void AddComponent<[EnforceEcsComponent] T>(int entity) where T : unmanaged, IEcsComponent {
        _version++;

        ArchetypeHandle source = _entityToArchetype[entity];

        if (source.HasComponent<T>()) {
            throw new InvalidOperationException("Entity already has component.");
        }

        ArchetypeComposition64 comp64 = source.Composition.With<T>();


        if (!_globalArchetypes.TryGetValue(comp64, out ArchetypeHandle? handle)) {
            handle = CreateArchetype(ref comp64);
            _globalArchetypes[comp64] = handle;
        }

        source.MigrateEntityToArchetype(entity, handle.Get());

        _entityToArchetype[entity] = handle;
    }

    public ref T GetComponent<[EnforceEcsComponent] T>(int entity) where T : unmanaged, IEcsComponent {
        return ref _entityToArchetype[entity].GetComponent<T>(entity);
    }

    public void RemoveComponent<[EnforceEcsComponent] T>(int entity) where T : unmanaged, IEcsComponent {
        _version++;

        ArchetypeHandle source = _entityToArchetype[entity];

        if (!source.HasComponent<T>()) {
            throw new InvalidOperationException("Entity does not have component.");
        }

        ArchetypeComposition64 comp64 = source.Composition.Without<T>();

        if (!_globalArchetypes.TryGetValue(comp64, out ArchetypeHandle? handle)) {
            handle = CreateArchetype(ref comp64);
            _globalArchetypes[comp64] = handle;
        }

        source.MigrateEntityToArchetype(entity, handle.Get());

        _entityToArchetype[entity] = handle;
    }

    public void MarkEntityForDestruction(int entity) {
        _destructionQueue.Add(entity);
    }

    internal void AddDestructionTags() {
        foreach (int entity in _destructionQueue) {
            AddComponent<EntityDestructionTagComponent>(entity);
        }
    }

    public void DestroyMarkedEntities() {   
        _version++;
        
        foreach (int entity in _destructionQueue) {
            _entityToArchetype[entity].DestroyEntityComponents(entity);
            _entityToArchetype.Remove(entity);

            EntityManager.FreeId(entity);
        }
        
        _destructionQueue.Clear();
    }

    public EcsQuery GetDestroyQueueQuery() {
        return _destructionQueueQuery;
    }

    /// <summary>
    /// It is not recommended to call this method.
    /// Prefer MarkEntityForDestruction and DestroyMarkedEntities instead.
    /// </summary>
    /// <param name="entity"></param>
    public void DestroyEntityImmediately(int entity) {
        _version++;

        _entityToArchetype[entity].DestroyEntityComponents(entity);
        _entityToArchetype.Remove(entity);

        _destructionQueue.Remove(entity);

        EntityManager.FreeId(entity);
    }

    #endregion

    public static EcsWorldBuilder Create(nuint ecsInitBufferSize) {
        return new EcsWorldBuilder(ecsInitBufferSize);
    }

    public class EcsWorldBuilder {
        private readonly List<Assembly> _assemblies = [];
        private readonly nuint _initBufferSize;
        private ArchetypeCompositionSize _archetypeCompositionSize = ArchetypeCompositionSize.C128;

        internal EcsWorldBuilder(nuint initBufferSize) {
            _initBufferSize = initBufferSize;
        }

        /// <summary>
        /// Registers an assembly for component discovery.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public EcsWorldBuilder AddAssembly(Assembly assembly) {
            _assemblies.Add(assembly);

            return this;
        }

        public EcsWorldBuilder SetArchetypeCompositionSize(ArchetypeCompositionSize size) {
            _archetypeCompositionSize = size;

            return this;
        }

        public EcsWorld Build() {
            return new EcsWorld(_initBufferSize, _assemblies.ToArray(), _archetypeCompositionSize);
        }
    }

    private void ReleaseUnmanagedResources() {
        // NativeMemory.AlignedFree(_archetypes);
    }

    private void Dispose(bool disposing) {
        // ReleaseUnmanagedResources();

        if (disposing) {
            ChunkAllocator.Dispose();
            ArchetypeMetadataSlabAllocator.Dispose();
            EntityManager.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EcsWorld() {
        Dispose(false);
    }
}