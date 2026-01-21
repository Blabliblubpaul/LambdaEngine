using System.Runtime.InteropServices;
using LambdaEngine.Rendering.RenderCommands;
using LambdaEngine.Types;
using SDL3;

namespace LambdaEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct RenderCommand {
     public readonly RenderCommandType Type;
     public readonly int ZIndex;
     public readonly SDL.FRect DestRect;
     public readonly ColorRgb Color;

     public readonly SpriteRenderCommand SpriteData;

     private RenderCommand(int zIndex, SDL.FRect destRect, ColorRgb color) {
          Type = RenderCommandType.PRIMITIVE_RECT;
          ZIndex = zIndex;
          DestRect = destRect;
          Color = color;
     }
     
     public RenderCommand(int zIndex, SDL.FRect destRect, ColorRgb color, SpriteRenderCommand spriteData) {
          Type = RenderCommandType.SPRITE;
          ZIndex = zIndex;
          DestRect = destRect;
          Color = color;
          SpriteData = spriteData;
     }
     
     public static RenderCommand RectPrimitiveCommand(int zIndex, SDL.FRect destRect, ColorRgb color) {
          return new RenderCommand(zIndex, destRect, color);
     }
     
     public static RenderCommand SpriteCommand(int zIndex, SDL.FRect destRect, ColorRgb color, SpriteRenderCommand spriteData) {
          return new RenderCommand(zIndex, destRect, color, spriteData);
     }
}