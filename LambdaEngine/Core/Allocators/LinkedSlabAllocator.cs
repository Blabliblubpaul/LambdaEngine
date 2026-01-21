using System.Runtime.InteropServices;

namespace LambdaEngine.Core.Allocators;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LinkedSlabAllocator {
    private const int HEADER_SIZE = 24;
    
    private readonly byte* _start;
    
    private readonly nuint _slabSize;
    private readonly nuint _alignment;
    private int _slabCount;

    public LinkedSlabAllocator(nuint slabSize, nuint alignment = 1) {
        _slabSize = slabSize;
        _alignment = alignment;
        
        // Allocate first slab
        _start = (byte*)NativeMemory.AlignedAlloc(_slabSize, _alignment);
            
        ref Slab start = ref Slab.Get(_start);
        start.This = _start;

        _slabCount = 1;
    }

    private ref Slab Last() {
        if (_start == null) {
            throw new InvalidOperationException("Slab allocator not initialized.");
        }

        ref Slab slab = ref Slab.Get(_start);
        byte* next = slab.Next;
        
        while (next != null) {
            slab = Slab.Get(next);
            next = slab.Next;
        }
        
        return ref slab;
    }

    private byte* AppendSlab() {
        ref Slab slab = ref Last();
        
        byte* nextPtr = (byte*)NativeMemory.AlignedAlloc(_slabSize, _alignment);

        if (nextPtr == null) {
            throw new OutOfMemoryException("Unable to allocate slab.");
        }
        
        slab.Next = nextPtr;
        
        ref Slab last = ref Slab.Get(nextPtr);
        last.This = nextPtr;
        last.Previous = slab.This;
        
        _slabCount++;
        return nextPtr;
    }

    private void FreeLastSlab() {
        ref Slab slab = ref Last();

        if (slab.Previous == null) {
            return;
        }

        ref Slab previous = ref Slab.Get(slab.Previous);
        previous.Next = null;
        
        slab.Previous = null;
        
        NativeMemory.AlignedFree(slab.This);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    private struct Slab {
        public byte* This;
        public byte* Next;
        public byte* Previous;

        public static ref Slab Get(byte* ptr) {
            return ref *(Slab*)ptr;
        }
    }
}