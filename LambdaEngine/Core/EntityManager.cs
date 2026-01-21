// #define DEBUG_DUMB_ENTITIES

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LambdaEngine.Core.Common;

namespace LambdaEngine.Core;

internal unsafe class EntityManager : IDisposable {
    private Entity* _entities;
    
    private readonly Stack<int> _freeIds = new(64);
    
    private int _nextId = 0;

    private int _capacity;
    private int _count;

    public EntityManager(int capacity) {
        _capacity = capacity;
        
        // TODO: Use Allocators here
        _entities = (Entity*)NativeMemory.AlignedAlloc((nuint)(capacity * sizeof(Entity)), 4);
        Unsafe.InitBlock(_entities, 255, (uint)(capacity * sizeof(Entity)));
    }
    
    public int NextId() {
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### Before NextId: DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
        if (!_freeIds.TryPop(out int entityId)) {
            if (_count == _capacity) {
                throw new OutOfMemoryException("No more entities available.");
            }
            
            entityId = _nextId++;
        }
        
        ref Entity e = ref _entities[entityId];
        e.Index = int.MaxValue;
        e._chunk = int.MaxValue;

        _count++;
        
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### After NextId: DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
        
        return entityId;
    }
    
    public void FreeId(int id) {
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### Before FreeId(Id: {id}): DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
        ref Entity e = ref _entities[id];
        e.Index = int.MaxValue;
        e._chunk = int.MaxValue;
        
        if (id == _nextId - 1) {
            _nextId--;
        }
        else {
            _freeIds.Push(id);
        }
        
        _count--;
        
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### After FreeId(Id: {id}): DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
    }

    public ref Entity GetEntity(int id) {
        return ref _entities[id];
    }
    
    public int GetEntityIndex(int id) {
        return _entities[id].Index;
    }

    public int GetEntityChunk(int id) {
        return _entities[id]._chunk;
    }
    
    public void SetEntity(int id, int chunk, int index) {
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### Before SetEntity(Id: {id}, Chunk: {chunk}, Index: {index}): DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
        *(int*)(_entities + id) = chunk;
        *((int*)(_entities + id) + 1) = index;
        
#if DEBUG_DUMB_ENTITIES
        Console.WriteLine($"##### After SetEntity(Id: {id}, Chunk: {chunk}, Index: {index}): DUMPING ENTITIES: #####");
        for (int i = 0; i < _count; i++) {
            ref Entity entity = ref _entities[i];
            
            Console.WriteLine($"Entity: {i} - Chunk: {entity._chunk} - Index: {entity.Index}");
        }
#endif
    }

    private void ReleaseUnmanagedResources() {
        NativeMemory.AlignedFree(_entities);
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~EntityManager() {
        ReleaseUnmanagedResources();
    }
}