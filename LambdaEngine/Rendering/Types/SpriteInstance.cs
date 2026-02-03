using System.Numerics;
using System.Runtime.InteropServices;

namespace LambdaEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstance {
    public Vector3 Position;
    public float Rotation;
    
    public float W;
    public float H;
    
    private float padding_a, padding_b;

    public float TexU, TexV, TexW, TexH;
    public FColorRGBA Color;
}