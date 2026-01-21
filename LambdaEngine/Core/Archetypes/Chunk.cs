namespace LambdaEngine.Core.Archetypes;

public unsafe struct Chunk(byte* ptr, int id) {
    public readonly byte* Ptr = ptr;
    public readonly int ID = id;

    public int Count;
}