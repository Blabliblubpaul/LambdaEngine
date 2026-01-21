using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LambdaEngine.Core.Allocators;

internal unsafe class ArchetypeMetadataSlabAllocator : IDisposable {
    public const ushort SLAB_SIZE = 4096;
    private byte* _slabOrigin;

    private byte* _slabCurrent;

    private bool _initialized;

    public ArchetypeMetadataSlabAllocator() {
        if (_initialized) {
            throw new InvalidOperationException("Archetype metadata slab allocator already initialized.");
        }

        _slabOrigin = (byte*)NativeMemory.AlignedAlloc(SLAB_SIZE, 64);
        Unsafe.InitBlock(_slabOrigin, 0, SLAB_SIZE);

#if DEBUG_MEMORY
        DebugMemory.RegisterBlockAllocation(SLAB_SIZE, 64, $"{nameof(Archetype)}:MetadataSlabAllocator", "ArchetypeMetadataSlabAllocator.cs", 21, DateTime.Now);
#endif

        _slabCurrent = _slabOrigin;

        _initialized = true;
    }

    public byte* AllocateSlab(nuint size) {
        if (!_initialized) {
            throw new InvalidOperationException("Slab allocator not initialized.");
        }

        byte* slabPtr = _slabCurrent;
        _slabCurrent += size;

        return slabPtr;
    }

    private void ReleaseUnmanagedResources() {
        NativeMemory.AlignedFree(_slabOrigin);
        _slabOrigin = null;

        _initialized = false;
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ArchetypeMetadataSlabAllocator() {
        ReleaseUnmanagedResources();
    }
}