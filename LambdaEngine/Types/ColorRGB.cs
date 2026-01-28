using System.Runtime.InteropServices;

namespace LambdaEngine.Types;

[StructLayout(LayoutKind.Sequential)]
public struct ColorRgb(byte r, byte g, byte b) {
    public static readonly ColorRgb White = new(255, 255, 255);
    public static readonly ColorRgb Black = new(0, 0, 0);
    public static readonly ColorRgb Red = new(255, 0, 0);
    public static readonly ColorRgb Green = new(0, 255, 0);
    public static readonly ColorRgb Blue = new(0, 0, 255);
    public static readonly ColorRgb Yellow = new(255, 255, 0);
    public static readonly ColorRgb Cyan = new(0, 255, 255);
    public static readonly ColorRgb Magenta = new(255, 0, 255);
    
    public byte R = r;
    public byte G = g;
    public byte B = b;

    public readonly uint ToUint32() {
        throw new NotImplementedException();
    }
}