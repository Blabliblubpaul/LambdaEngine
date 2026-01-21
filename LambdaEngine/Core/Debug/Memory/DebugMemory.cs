using System.Diagnostics;

namespace LambdaEngine.Core.Debug.Memory;

public static class DebugMemory {
    private static readonly List<MemoryBlock> _blocks = new(1024);
    
    [Conditional("DEBUG")]
    internal static void RegisterBlockAllocation(int size, int alignment, string? owner, string? file, int line, DateTime allocationTime) {
        _blocks.Add(new MemoryBlock(size, alignment, owner, file, line, allocationTime));
    }

    public static void DumbBlockAllocations(string? file = null) {
        Console.WriteLine("================================");
        Console.WriteLine("Dumbing memory block allocations:");

        FileStream? stream = null;
        TextWriter? wr = null;
        
        if (file != null) {
            Console.WriteLine($"Writing to file: {file}");
            
            stream = File.Create(file);
            wr = new StreamWriter(stream);
            Console.SetOut(wr);
            Console.SetError(wr);
        }
        
        Console.WriteLine("================================");
        
        foreach (MemoryBlock memoryBlock in _blocks) {
            Console.WriteLine(memoryBlock);
            Console.WriteLine("\n---------------------------\n");
        }
        
        if (file != null) {
            Console.SetOut(Console.Out);
            Console.SetError(Console.Error);;
            
            stream?.Dispose();
            wr?.Dispose();
        }
    }
}