using System.Collections;
using LambdaEngine.Core.Allocators;
using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core;

internal class ArchetypeChunkStack : IEnumerable<Chunk> {
    private Chunk[] _denseItems;
    private int[] _denseToId;
    private static int[] _idToDense = new int[ChunkAllocator.MAX_CHUNK_COUNT];

    private int _count;

    public int Count {
        get => _count;
    }
    public int Capacity { get; private set; }

    static ArchetypeChunkStack() {
        Array.Fill(_idToDense, -1);
    }

    public ArchetypeChunkStack(int capacity) {
        Capacity = capacity;
        // _denseItems = (T*)NativeMemory.AlignedAlloc((nuint)(sizeof(T) * capacity), alignment);
        _denseItems = new Chunk[capacity];
        // _denseToId = (int*)NativeMemory.AlignedAlloc((nuint)(sizeof(int) * capacity), 4);
        _denseToId = new int[capacity];
        
        _count = 0;
    }

    public ref Chunk Top {
        get => ref _denseItems[_count - 1];
    }

    public void Push(int id, Chunk item) {
        if (_count == Capacity) {
            Resize(Capacity * 2);
        }
        
        _denseItems[_count] = item;
        _denseToId[_count] = id;
        _idToDense[id] = _count;
        
        _count++;
    }

    public ref Chunk Pop() {
        int id = _denseToId[_count - 1];
        _idToDense[id] = -1;
        
        return ref _denseItems[--_count];
    }
    
    public ref Chunk GetById(int id) {
        int denseIndex = _idToDense[id];
        
        //D_Debug.Assert(denseIndex != -1, "Chunk not found");
        
        if (denseIndex == -1) {
            throw new KeyNotFoundException();
        }

        return ref _denseItems[denseIndex];
    }

    private void Resize(int newCapacity) {
        // _denseItems = (T*)NativeMemory.AlignedRealloc(_denseItems, (nuint)(sizeof(T) * newCapacity), _alignment);
        // _denseToId = (int*)NativeMemory.AlignedRealloc(_denseToId, (nuint)(sizeof(int) * newCapacity), 4);
        Array.Resize(ref _denseItems, newCapacity);
        Array.Resize(ref _denseToId, newCapacity);
        
        Capacity = newCapacity;
    }

    // private Memory<T> GetMemory() {
    //     NativeMemoryManager<T> manager = new(_denseItems, _count);
    //     return manager.Memory;
    // }

    public IEnumerator<Chunk> GetEnumerator() {
        // Memory<T> span = GetMemory();
        //
        // for (int i = 0; i < _count; i++) {
        //     yield return span.Span[i];
        // }
        
        for (int i = 0; i < _count; i++) {
            yield return _denseItems[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}