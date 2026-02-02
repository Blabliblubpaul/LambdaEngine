namespace LambdaEngine.Rendering.Types;

// Make a handle class
public readonly struct BlendMode {
    internal readonly IntPtr Handle;
    
    internal BlendMode(IntPtr handle) {
        Handle = handle;
    }
}