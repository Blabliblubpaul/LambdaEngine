using System.Runtime.CompilerServices;

namespace LambdaEngine.Core;

public static class LMath {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilDivideInt32(int a, int b) {
        return (a + b - 1) / b;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsPowerOfTwo(int value) {
        return value > 0 && (value & (value - 1)) == 0;
    }
}