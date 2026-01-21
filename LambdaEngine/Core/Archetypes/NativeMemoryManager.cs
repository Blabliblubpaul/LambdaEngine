using System.Buffers;

namespace LambdaEngine.Core.Archetypes;

/// <summary>
/// A wrapper around an unmanaged block of memory.
/// </summary>
/// <typeparam name="T"></typeparam>
internal unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged {
    private readonly int _length;
    private readonly T* _pointer;

    public NativeMemoryManager(T* pointer, int length) {
        _pointer = pointer;
        _length = length;
    }

    public override Span<T> GetSpan() {
        return new Span<T>(_pointer, _length);
    }

    public override MemoryHandle Pin(int elementIndex = 0) {
        if (elementIndex < 0 || elementIndex >= _length) {
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        }

        return new MemoryHandle(_pointer + elementIndex);
    }

    public override void Unpin() { }

    protected override void Dispose(bool disposing) { }
}