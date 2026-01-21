namespace LambdaEngine.Core.Common;

public static class MemSize {
    private const nuint KiB = 1024;
    private const nuint MiB = 1024 * KiB;
    private const nuint GiB = 1024 * MiB;
    
    public static nuint FromBytes(int bytes) {
        return (nuint) bytes;
    }
    
    public static nuint FromKBytes(int kibs) {
        return (nuint) kibs * KiB;
    }
    
    public static nuint FromMBytes(int mibs) {
        return (nuint) mibs * MiB;
    }
    
    public static nuint FromGBytes(int gibs) {
        return (nuint) gibs * GiB;
    }
}