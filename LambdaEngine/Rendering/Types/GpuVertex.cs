using System.Numerics;
using System.Runtime.InteropServices;

namespace LambdaEngine.Rendering;

// TODO: Is this needed? Maybe use SDL.Vertex
[StructLayout(LayoutKind.Sequential)]
internal struct GpuVertex {
    public Vector2 Position;
    public Vector2 UV;
    public uint Color;
    
    public GpuVertex(Vector2 position, Vector2 uv, uint color) {
        Position = position;
        UV = uv;
        Color = color;
    }

    public GpuVertex(Vector2 position, uint color)
    : this(position, Vector2.Zero, color) { }
}