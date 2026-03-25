using System.Runtime.InteropServices;

namespace LambdaEngine.Core;

public static unsafe class LNative {
    private delegate int MemcmpDelegate(void* b1, void* b2, ulong count);
    
    private static readonly MemcmpDelegate memcmp;
    
    static LNative() {
        if (OperatingSystem.IsWindows()) {
            memcmp = NativeMemcmpWindows;
        }
        else if (OperatingSystem.IsLinux()) {
            memcmp = NativeMemcmpLinux;
        }
        else {
            throw new PlatformNotSupportedException();
        }
    }
    
    public static int NativeMemcmp(void* b1, void* b2, ulong count) {
        return memcmp(b1, b2, count);
    }
    
    #region Memcmp
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
    private static extern int NativeMemcmpWindows(void* b1, void* b2, ulong count);
    
    [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
    private static extern int NativeMemcmpLinux(void* b1, void* b2, ulong count);
    #endregion
}