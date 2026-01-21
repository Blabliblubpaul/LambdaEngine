using System.Runtime.InteropServices;

namespace LambdaEngine.Core.Common;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Entity {
    internal int _chunk = - 1;
    public int Index = -1;

    public Entity(int chunk, int index) {
        _chunk = chunk;
        Index = index;
    }
}