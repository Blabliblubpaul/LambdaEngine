using System.Runtime.InteropServices;

namespace LambdaEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct FColorRGBA(float r, float g, float b, float a) {
    public float R = r, G = g, B = b, A = a;
}