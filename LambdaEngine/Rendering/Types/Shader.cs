namespace LambdaEngine.Rendering;

// Make a handle class
public readonly struct Shader {
    internal readonly IntPtr Handle;
    
    internal Shader(IntPtr handle) {
        Handle = handle;
    }
}