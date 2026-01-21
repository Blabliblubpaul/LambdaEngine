using LambdaEngine.Types;
using SDL3;

namespace LambdaEngine.Rendering;

internal static class LRendering {
    internal static bool SetRenderDrawColor(IntPtr renderer, ColorRgb color, byte alpha) {
        return SDL.SetRenderDrawColor(renderer, color.R, color.G, color.B, alpha);
    }
    
    internal static bool SetTextureColorMod(IntPtr texture, ColorRgb mod) {
        return SDL.SetTextureColorMod(texture, mod.R, mod.G, mod.B);
    }
}