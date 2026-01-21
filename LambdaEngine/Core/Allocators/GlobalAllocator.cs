using System.Runtime.InteropServices;

namespace LambdaEngine.Core.Allocators;

// TODO: Make it threadsafe
public static unsafe class GlobalAllocator {
    public static void* Allocate(nuint size) {
        return NativeMemory.Alloc(size);
    }
    
    public static void* AllocateZeroed(nuint size) {
        return NativeMemory.AllocZeroed(size);
    }

    public static void* AllocateAligned(nuint size, nuint alignment) {
        return NativeMemory.AlignedAlloc(size, alignment);
    }

    public static void* AllocateAlignedZeroed(nuint size, nuint alignment) {
        void* ptr = NativeMemory.AlignedAlloc(size, alignment);
        NativeMemory.Clear(ptr, size);

        return ptr;
    }
    
    public static void* Reallocate(void* ptr, nuint size) {
        return NativeMemory.Realloc(ptr, size);
    }
    
    public static void* ReallocateAligned(void* ptr, nuint size, nuint alignment) {
        return NativeMemory.AlignedRealloc(ptr, size, alignment);
    }
    
    public static void Free(void* ptr) {
        NativeMemory.Free(ptr);
    }
    
    public static void FreeAligned(void* ptr) {
        NativeMemory.AlignedFree(ptr);
    }
}