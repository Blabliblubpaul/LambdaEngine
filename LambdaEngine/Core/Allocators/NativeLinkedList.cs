using System.Runtime.InteropServices;

namespace LambdaEngine.Core.Allocators;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct NativeLinkedList<T> where T : unmanaged {
    private const int HEADER_SIZE = 32;
    
    private readonly byte* _start;
    
    private readonly nuint _slabSize;
    private int _slabCount;

    private int _count;
    private int _blockCapacity;

    public int Capacity {
        get => _slabCount * _blockCapacity;
    }

    public int Count {
        get => _count;
    }

    public NativeLinkedList(nuint slabSize) {
        _slabSize = slabSize;
        _blockCapacity = (int)(slabSize - HEADER_SIZE) / sizeof(T);

        _count = 0;

        if (_blockCapacity < 1) {
            throw new ArgumentException("Invalid slab size.");
        }
        
        // Allocate first slab
        _start = (byte*)NativeMemory.AlignedAlloc(_slabSize, 8);
            
        ref Slab start = ref Slab.Get(_start);
        start.This = _start;

        _slabCount = 1;
    }

    public void Add(T item) {
        ref Slab slab = ref Last();
        if (slab.Count == _blockCapacity) {
            slab = ref Slab.Get(AppendSlab());
        }

        ((T*)(slab.This + HEADER_SIZE))[slab.Count] = item;
        
        slab.Count++;
        _count++;
    }
    
    public void Set(int index, T item) {
        if (index < 0 || index >= _count) {
            throw new IndexOutOfRangeException();
        }

        int current = 0;
        ref Slab slab = ref Slab.Get(_start);

        do {
            if (current + slab.Count > index) {
                ((T*)(slab.This + HEADER_SIZE))[index - current] = item;
                return;
            }

            if (slab.Next == null) {
                break;
            }

            current += slab.Count;
            slab = ref Slab.Get(slab.Next);
        } while (true);
        
        throw new IndexOutOfRangeException();
    }

    public ref T Get(int index) {
        if (index < 0 || index >= _count) {
            throw new IndexOutOfRangeException();
        }

        int current = 0;
        ref Slab slab = ref Slab.Get(_start);

        do {
            if (current + slab.Count > index) {
                return ref ((T*)(slab.This + HEADER_SIZE))[index - current];
            }

            if (slab.Next == null) {
                break;
            }

            current += slab.Count;
            slab = ref Slab.Get(slab.Next);
        } while (true);
        
        throw new IndexOutOfRangeException();
    }
    
    public T GetCopy(int index) {
        if (index < 0 || index >= _count) {
            throw new IndexOutOfRangeException();
        }

        int current = 0;
        ref Slab slab = ref Slab.Get(_start);

        do {
            if (current + slab.Count > index) {
                return ((T*)(slab.This + HEADER_SIZE))[index - current];
            }

            if (slab.Next == null) {
                break;
            }

            current += slab.Count;
            slab = ref Slab.Get(slab.Next);
        } while (true);
        
        throw new IndexOutOfRangeException();
    }
    
    // TODO: Shift elements back
    public void RemoveAt(int index) {
        if (index < 0 || index >= _count) {
            throw new IndexOutOfRangeException();
        }

        int current = 0;
        ref Slab slab = ref Slab.Get(_start);

        do {
            if (current + slab.Count > index) {
                slab.Count--;
                _count--;

                if (slab.Count == 0) {
                    FreeLastSlab();
                }
                return;
            }

            if (slab.Next == null) {
                break;
            }

            current += slab.Count;
            slab = ref Slab.Get(slab.Next);
        } while (true);
        
        throw new IndexOutOfRangeException();
    }
    
    public ref T this[int index] {
        get => ref Get(index);
    }

    public void Dispose() {
        ref Slab slab = ref Last();

        while (true) {
            byte* previous = slab.Previous;
            
            NativeMemory.AlignedFree(slab.This);
            
            if (slab.Previous != null) {
                slab = ref Slab.Get(previous);
            }
            else {
                return;
            }
        }
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
        
        byte* nextPtr = (byte*)NativeMemory.AlignedAlloc(_slabSize, 8);

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
        public int Count;

        public static ref Slab Get(byte* ptr) {
            return ref *(Slab*)ptr;
        }
    }
}