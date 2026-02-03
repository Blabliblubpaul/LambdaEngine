namespace LambdaEngine.Rendering;

internal readonly struct GpuTexture(IntPtr handle, uint width, uint height) {
    public readonly IntPtr Handle = handle;
    public readonly uint Width = width, Height = height;
}