using System.Runtime.InteropServices;

namespace LambdaEngine.Core;

public static unsafe class LNative {
    #region Memcmp
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
    private static extern int NativeMemcmpWindows(void* b1, void* b2, ulong count);
    
    [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
    private static extern int NativeMemcmpMacOs(void* b1, void* b2, ulong count);
    
    [DllImport("libc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
    private static extern int NativeMemcmpLinux(void* b1, void* b2, ulong count);
    #endregion

    public static int NativeMemcmp(void* b1, void* b2, ulong count) {
        if (OperatingSystem.IsWindows()) {
            return NativeMemcmpWindows(b1, b2, count);
        }

        if (OperatingSystem.IsMacOS()) {
            return NativeMemcmpMacOs(b1, b2, count);
            
        }

        if (OperatingSystem.IsLinux()) {
            return NativeMemcmpLinux(b1, b2, count);
        }

        throw new PlatformNotSupportedException();
    }
}