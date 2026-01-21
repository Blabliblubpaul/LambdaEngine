// #define DEBUG_DUMP_CHUNK_LAYOUT

using System.Runtime.CompilerServices;
using LambdaEngine.Core.Allocators;
using LambdaEngine.Core.ArchetypeComposition;

namespace LambdaEngine.Core.Archetypes;

public sealed unsafe class Archetype {
    // TODO: Calc size
    internal const nuint SIZE = 128; // 109 aligned to 128
    private const int CHUNK_SIZE = 16_384;
    private const int COMPONENT_ALIGNMENT = 32;

    private const int METADATA_SIZE_COEFFICIENT = sizeof(ushort) * 3;

    private readonly ArchetypeComposition64 _composition;

    private int _capacity;

    private readonly ushort* _offsets;
    private readonly ushort* _sizes;

    private readonly ushort* _typeIndices;

    #region Caches

    private ushort _typeIndexCache0;
    private int _typeIndexCacheValue0;
    private ushort _typeIndexCache1;
    private int _typeIndexCacheValue1;
    private bool _cache;

    #endregion

    private readonly ArchetypeChunkStack _chunks = new(4);

    private readonly EntityManager _entityManager;
    private readonly ChunkAllocator _chunkAllocator;

    internal ArchetypeComposition64 Composition {
        get => _composition;
    }

    internal Archetype(EcsWorld world, params Type[] types) {
        _entityManager = world.EntityManager;
        ArchetypeMetadataSlabAllocator metadataAllocator = world.ArchetypeMetadataSlabAllocator;
        _chunkAllocator = world.ChunkAllocator;

        _composition = new ArchetypeComposition64(types);

        if (types.Length == 0) {
            return;
        }

        AllocateChunk();

        ushort[] typeIds = _composition.GetTypeIds();
        int typeCount = typeIds.Length;

        byte* slab = metadataAllocator.AllocateSlab((nuint)(typeCount * METADATA_SIZE_COEFFICIENT));

        _typeIndices = (ushort*)slab;
        _offsets = (ushort*)(slab + typeCount * sizeof(ushort));
        _sizes = (ushort*)(slab + typeCount * sizeof(ushort) * 2);

        for (int i = 0; i < typeCount; i++) {
            _typeIndices[i] = typeIds[i];
        }

        CalculateChunkLayout(COMPONENT_ALIGNMENT, typeIds);
    }

    internal Archetype(EcsWorld world, ref ArchetypeComposition64 composition) {
        _entityManager = world.EntityManager;
        ArchetypeMetadataSlabAllocator metadataAllocator = world.ArchetypeMetadataSlabAllocator;
        _chunkAllocator = world.ChunkAllocator;

        _composition = composition;

        if (composition.ComponentCount == 0) {
            return;
        }

        AllocateChunk();

        ushort[] typeIds = _composition.GetTypeIds();
        int typeCount = typeIds.Length;

        byte* slab = metadataAllocator.AllocateSlab((nuint)(typeCount * METADATA_SIZE_COEFFICIENT));

        _typeIndices = (ushort*)slab;
        _offsets = (ushort*)(slab + typeCount * sizeof(ushort));
        _sizes = (ushort*)(slab + typeCount * sizeof(ushort) * 2);

        for (int i = 0; i < typeCount; i++) {
            _typeIndices[i] = typeIds[i];
        }

        CalculateChunkLayout(COMPONENT_ALIGNMENT, typeIds);
    }

    internal bool HasComponent<T>() where T : unmanaged, IEcsComponent {
        return _composition.Has(ComponentTypeRegistry.GetId<T>());
    }

    /// <summary>
    /// Modifies the entity's chunk and entity index to point to the new entity position.
    /// </summary>
    /// <param name="entity"></param>
    internal void InsertDefaultEntity(int entity) {
        if (_composition.ComponentCount == 0) {
            return;
        }

        D_Debug.Assert(_chunks != null, "Archetype chunk storage is uninitialized.");
        D_Debug.Assert(_chunks.Count > 0, "Archetype has no chunks.");

        ref Chunk chunk = ref _chunks.Top;

        if (chunk.Count == _capacity) {
            AllocateChunk();
            chunk = ref _chunks.Top;
        }

        _entityManager.SetEntity(entity, chunk.ID, chunk.Count);

        ((int*)chunk.Ptr)[chunk.Count] = entity;
        chunk.Count++;
    }

    internal void SetComponent<T>(int entity, T component) where T : unmanaged, IEcsComponent {
        if (_composition.ComponentCount == 0) {
            return;
        }

        int index = FindTypeIndex(ComponentTypeId<T>.ID);

        int chunkId = _entityManager.GetEntityChunk(entity);
        int entityIndex = _entityManager.GetEntityIndex(entity);

        *(T*)(_chunks.GetById(chunkId).Ptr + _offsets[index] + entityIndex * _sizes[index]) = component;
    }

    internal ref T GetComponent<T>(int entity) where T : unmanaged, IEcsComponent {
        int index = FindTypeIndex(ComponentTypeId<T>.ID);

        int chunkId = _entityManager.GetEntityChunk(entity);
        int entityIndex = _entityManager.GetEntityIndex(entity);

        return ref *(T*)(_chunks.GetById(chunkId).Ptr + _offsets[index] + entityIndex * _sizes[index]);
    }

    internal void MigrateEntityToArchetype(int entity, Archetype archetype) {
        if (ReferenceEquals(this, archetype)) {
            return;
        }

        int entityIndex = _entityManager.GetEntityIndex(entity);
        int chunkId = _entityManager.GetEntityChunk(entity);
        ref Chunk chunk = ref _chunks.GetById(chunkId);

        // Chunk and index of entity are now modified.
        archetype.InsertDefaultEntity(entity);

        ushort[] typeIds = _composition.GetTypeIds();

        foreach (ushort componentType in typeIds) {
            if (!archetype._composition.Has(componentType)) {
                continue;
            }

            int typeIndex = FindTypeIndex(componentType);

            byte* ptr = chunk.Ptr + _offsets[typeIndex] + entityIndex * _sizes[typeIndex];
            archetype.CopyRawComponent(entity, componentType, ptr);
        }

        DestroyEntityComponents(entityIndex, chunkId);
    }

    private void CopyRawComponent(int entity, ushort componentType, byte* component) {
        int typeIndex = FindTypeIndex(componentType);

        int chunkId = _entityManager.GetEntityChunk(entity);

        ref Chunk chunk = ref _chunks.GetById(chunkId);

        byte* ptr = chunk.Ptr + _offsets[typeIndex] + _entityManager.GetEntityIndex(entity) * _sizes[typeIndex];
        Unsafe.CopyBlockUnaligned(ptr, component, _sizes[typeIndex]);
    }

    private void DestroyEntityComponents(int entityIndex, int chunkId) {
        if (_composition.ComponentCount == 0) {
            return;
        }

        ref Chunk chunk = ref _chunks.GetById(chunkId);

        if (entityIndex == chunk.Count - 1 && chunk.ID == _chunks.Top.ID) {
            for (int i = 0; i < _composition.ComponentCount; i++) {
                byte* ptr = chunk.Ptr + _offsets[i] + entityIndex * _sizes[i];

                Unsafe.InitBlockUnaligned(ptr, 0, _sizes[i]);
            }

            chunk.Count--;

            if (chunk.Count == 0 && _chunks.Count > 1) {
                _chunkAllocator.FreeChunk(ref chunk);
                _chunks.Pop();
            }

            return;
        }

        ref Chunk lastChunk = ref _chunks.Top;
        int last = lastChunk.Count - 1;

        // Replace id if the destroyed entity with the id of the last entity
        (*((int*)chunk.Ptr + entityIndex)) = *((int*)lastChunk.Ptr + last);
        // Copy components from the last entity to the destroyed entity
        for (int i = 0; i < _composition.ComponentCount; i++) {
            byte* dest = chunk.Ptr + _offsets[i] + entityIndex * _sizes[i];
            byte* source = lastChunk.Ptr + _offsets[i] + last * _sizes[i];

            Unsafe.CopyBlockUnaligned(dest, source, _sizes[i]);
        }

        // Zero out the last entity's components' in its old position
        for (int i = 0; i < _composition.ComponentCount; i++) {
            byte* ptr = lastChunk.Ptr + _offsets[i] + last * _sizes[i];

            Unsafe.InitBlockUnaligned(ptr, 0, _sizes[i]);
        }

        // Get the id of the last entity from its old position
        int lastId = ((int*)lastChunk.Ptr)[last];

        // Update the chunkId and entityIndex of the (formerly) last entity
        _entityManager.SetEntity(lastId, chunk.ID, entityIndex);

        // Decrement the Count of the last chunk
        lastChunk.Count--;

        // Free the last chunk if it is empty
        if (lastChunk.Count == 0 && _chunks.Count > 1) {
            _chunkAllocator.FreeChunk(ref lastChunk);
            _chunks.Pop();
        }
    }
    
    internal void DestroyEntityComponents(int entity) {
        if (_composition.ComponentCount == 0) {
            return;
        }
 
        int chunkId = _entityManager.GetEntityChunk(entity);
        int entityIndex = _entityManager.GetEntityIndex(entity);
        
        DestroyEntityComponents(entityIndex, chunkId);
        
        // if (setEntity) {
        //     _entityManager.SetEntity(entity, -1, -1);
        // }
    }

    // internal void DestroyEntityComponents(int entity, bool setEntity) {
    //     if (_composition.ComponentCount == 0) {
    //         return;
    //     }
    //
    //     int chunkId = _entityManager.GetEntityChunk(entity);
    //     int entityIndex = _entityManager.GetEntityIndex(entity);
    //
    //     ref Chunk chunk = ref _chunks.GetById(chunkId);
    //
    //     if (entityIndex == chunk.Count - 1 && chunk.ID == _chunks.Top.ID) {
    //         for (int i = 0; i < _composition.ComponentCount; i++) {
    //             byte* ptr = chunk.Ptr + _offsets[i] + entityIndex * _sizes[i];
    //
    //             Unsafe.InitBlockUnaligned(ptr, 0, _sizes[i]);
    //         }
    //
    //         chunk.Count--;
    //
    //         if (chunk.Count == 0 && _chunks.Count > 1) {
    //             _chunkAllocator.FreeChunk(ref chunk);
    //             _chunks.Pop();
    //         }
    //
    //         if (setEntity) {
    //             _entityManager.SetEntity(entity, -1, -1);
    //         }
    //
    //         return;
    //     }
    //
    //     ref Chunk lastChunk = ref _chunks.Top;
    //     int last = lastChunk.Count - 1;
    //
    //     (*((int*)chunk.Ptr + entityIndex)) = *((int*)lastChunk.Ptr + last);
    //     for (int i = 0; i < _composition.ComponentCount; i++) {
    //         byte* dest = chunk.Ptr + _offsets[i] + entityIndex * _sizes[i];
    //         byte* source = lastChunk.Ptr + _offsets[i] + last * _sizes[i];
    //
    //         Unsafe.CopyBlockUnaligned(dest, source, _sizes[i]);
    //     }
    //
    //     for (int i = 0; i < _composition.ComponentCount; i++) {
    //         byte* ptr = lastChunk.Ptr + _offsets[i] + last * _sizes[i];
    //
    //         Unsafe.InitBlockUnaligned(ptr, 0, _sizes[i]);
    //     }
    //
    //     int lastId = ((int*)lastChunk.Ptr)[last];
    //     D_Debug.Assert(lastId == _entityManager.GetEntityIdByLocation(lastChunk.ID, last),
    //         "New last id is not equal to old last id."); 
    //
    //     _entityManager.SetEntity(lastId, chunk.ID, entityIndex);
    //
    //     lastChunk.Count--;
    //
    //     if (lastChunk.Count == 0 && _chunks.Count > 1) {
    //         _chunkAllocator.FreeChunk(ref lastChunk);
    //         _chunks.Pop();
    //     }
    //
    //     if (setEntity) {
    //         _entityManager.SetEntity(entity, -1, -1);
    //     }
    // }

    internal NativeMemoryManager<T>[] GetComponents<T>() where T : unmanaged, IEcsComponent {
        int index = FindTypeIndex(ComponentTypeId<T>.ID);

        NativeMemoryManager<T>[] components = new NativeMemoryManager<T>[_chunks.Count];
        int i = 0;
        foreach (Chunk chunk in _chunks) {
            components[i++] = new NativeMemoryManager<T>((T*)(chunk.Ptr + _offsets[index]), chunk.Count);
        }

        return components;
    }
    
    internal IEnumerable<NativeMemoryManager<int>> GetIds() {
        NativeMemoryManager<int>[] ids = new NativeMemoryManager<int>[_chunks.Count];
        int i = 0;
        foreach (Chunk chunk in _chunks) {
            ids[i++] = new NativeMemoryManager<int>((int*)(chunk.Ptr), chunk.Count);
        }

        return ids;
    }

    private void CalculateChunkLayout(int alignment, params ushort[] typeIds) {
        if (typeIds.Length == 0) {
            throw new ArgumentException("No types provided.");
        }

        int[] singleSizes = typeIds.Select(ComponentTypeRegistry.GetTypeSize).ToArray();

        int entitySize = singleSizes.Sum() + sizeof(int); // Add 4 to account for id index

        int[] positions;

        int maxCount = CHUNK_SIZE / entitySize;

        int offset;
        int[] sizes;
        int[] paddings;
        do {
            sizes = singleSizes.Select(s => s * maxCount).ToArray();
            int idSize = sizeof(int) * maxCount;

            positions = new int[typeIds.Length];
            paddings = new int[typeIds.Length];

            offset = idSize;

            positions[0] = offset;
            paddings[0] = 0;

            offset += sizes[0];

            for (int i = 1; i < typeIds.Length; i++) {
                int misalignment = offset % alignment;
                paddings[i] = misalignment == 0 ? 0 : alignment - misalignment;

                offset += paddings[i];
                positions[i] = offset;

                offset += sizes[i];
            }

            if (offset > CHUNK_SIZE) {
                maxCount--;
            }
        } while (offset > CHUNK_SIZE);

        _capacity = maxCount;

        for (int i = 0; i < _composition.ComponentCount; i++) {
            _offsets[i] = (ushort)positions[i];
            _sizes[i] = (ushort)singleSizes[i];
        }

#if DEBUG_DUMP_CHUNK_LAYOUT
        Console.WriteLine($"Chunk Size: {CHUNK_SIZE}");
        Console.WriteLine($"Entities: {maxCount}");
        Console.WriteLine($"Alignment: {alignment}");
        Console.WriteLine($"Last/Wasted: {offset}/{CHUNK_SIZE - offset}");
        Console.WriteLine("Components:");
        for (int i = 0; i < typeIds.Length; i++) {
            Console.WriteLine($"\t#{i}: {typeIds[i]} ({ComponentTypeRegistry.GetType(typeIds[i]).Name}):");
            Console.WriteLine($"\t\tSize: {singleSizes[i]}");
            Console.WriteLine($"\t\tAcc. Size: {sizes[i]}");
            Console.WriteLine($"\t\tPadding: {paddings[i]}");
            Console.WriteLine($"\t\tOffset: {positions[i]}");
            Console.WriteLine();
        }
#endif
    }

    private int FindTypeIndex(ushort typeId) {
        if (_typeIndexCache0 == typeId) {
            return _typeIndexCacheValue0;
        }

        if (_typeIndexCache1 == typeId) {
            return _typeIndexCacheValue1;
        }

        for (int i = 0; i < _composition.ComponentCount; i++) {
            if (_typeIndices[i] != typeId) {
                continue;
            }

            SetCache(typeId, i);

            return i;
        }

        throw new ArgumentException("Invalid component type.");
    }

    private void SetCache(ushort typeId, int index) {
        _cache = !_cache;

        switch (_cache) {
            case false:
                _typeIndexCache0 = typeId;
                _typeIndexCacheValue0 = index;
                break;

            case true:
                _typeIndexCache1 = typeId;
                _typeIndexCacheValue1 = index;
                break;
        }
    }

    private void AllocateChunk() {
        Chunk chunk = _chunkAllocator.AllocateChunk();
        _chunks.Push(chunk.ID, chunk);
    }
}