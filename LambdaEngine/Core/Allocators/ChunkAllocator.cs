using LambdaEngine.Core.Archetypes;

namespace LambdaEngine.Core.Allocators;

internal unsafe class ChunkAllocator : IDisposable {
    public const int MAX_CHUNK_COUNT = ushort.MaxValue;
    public const ushort CHUNK_SIZE = 16_384;
    private ChunkPtr _block;
    private int _capacity;
    
    private readonly Stack<int> _freeChunks = new(32);
    private int _nextChunk = 0;

    private bool _initialized;

    public ChunkAllocator(nuint initBlockSize) {
        if (_initialized) {
            throw new InvalidOperationException("Chunk allocator already initialized.");
        }

        if (initBlockSize % CHUNK_SIZE != 0) {
            throw new ArgumentException($"Invalid block size; has to be a multiple of {CHUNK_SIZE}.");
        }

        _block = (ChunkPtr)GlobalAllocator.AllocateAlignedZeroed(initBlockSize, 512);
        _capacity = (int)(initBlockSize / CHUNK_SIZE);
        
        _initialized = true;
    }
    
    public Chunk AllocateChunk() {
        if (!_initialized) {
            throw new InvalidOperationException("Chunk allocator not initialized.");
        }

        if (!_freeChunks.TryPop(out int offset)) {
            if (_nextChunk >= _capacity) {
                throw new OutOfMemoryException("No more chunks available.");
            }
            
            offset = _nextChunk++;
        }
        
        byte* chunkPtr = _block + offset;

        Chunk chunk = new(chunkPtr, offset);

        return chunk;
    }

    public void FreeChunk(ref Chunk chunk) {
        _freeChunks.Push(chunk.ID);
        
        chunk = default;
    }

    private void ReleaseUnmanagedResources() {
        GlobalAllocator.FreeAligned(_block);
        _block = null;
        
        _initialized = false;
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ChunkAllocator() {
        ReleaseUnmanagedResources();
    }
    
    private readonly struct ChunkPtr(byte* ptr) {
        public readonly byte* Ptr = ptr;
        
        public static byte* operator +(ChunkPtr ptr, int offset) {
            return ptr.Ptr + offset * CHUNK_SIZE;
        }
    
        public static implicit operator byte*(ChunkPtr ptr) {
            return ptr.Ptr;
        }
        
        public static implicit operator ChunkPtr(byte* ptr) {
            return new ChunkPtr(ptr);
        }
    }
}