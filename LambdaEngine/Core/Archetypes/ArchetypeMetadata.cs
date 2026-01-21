namespace LambdaEngine.Core.Archetypes;

internal unsafe struct ArchetypeMetadata {
    private ushort* _offsets;
    private ushort* _sizes;

    private readonly ushort* _typeIndices;
}