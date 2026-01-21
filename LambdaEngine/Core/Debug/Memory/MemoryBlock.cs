namespace LambdaEngine.Core.Debug.Memory;

internal record MemoryBlock(int Size, int Alignment, string? Owner, string? File, int Line, DateTime AllocationTime) {
    public int Size = Size;
    public int Alignment = Alignment;
    public string? Owner = Owner;
    public string? File = File;
    public int Line = Line;
    public DateTime AllocationTime = AllocationTime;
    
    public override string ToString() {
        return $"\tSize: {Size},\n\tAlignment: {Alignment},\n\tOwner: {Owner},\n\tFile: {File},\n\tLine: {Line},\n\tAllocationTime: {AllocationTime}";
    }
}