using System.Runtime.InteropServices;
using LambdaEngine.Rendering.RenderCommands;
using LambdaEngine.Types;
using SDL3;

namespace LambdaEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct RenderCommand {
     public readonly RenderKey RenderKey;
     public readonly SDL.FRect DestRect;
     public readonly ColorRgb Color;

     public readonly SpriteRenderCommand SpriteData;

     public sbyte ZIndex {
          get => RenderKey.ZIndex;
     }

     public RenderCommandType RenderType {
          get =>  RenderKey.RenderType;
     }

     private RenderCommand(sbyte zIndex, SDL.FRect destRect, ColorRgb color) {
          RenderKey = new RenderKey(zIndex, new RenderPipelineId(0), TextureId.NO_TEXTURE, RenderCommandType.PRIMITIVE_RECT);
          DestRect = destRect;
          Color = color;
     }
     
     public RenderCommand(sbyte zIndex, SDL.FRect destRect, ColorRgb color, SpriteRenderCommand spriteData) {
          RenderKey = new RenderKey(zIndex, new RenderPipelineId(0), TextureId.NO_TEXTURE, RenderCommandType.PRIMITIVE_RECT);
          DestRect = destRect;
          Color = color;
          SpriteData = spriteData;
     }
     
     public static RenderCommand RectPrimitiveCommand(sbyte zIndex, SDL.FRect destRect, ColorRgb color) {
          return new RenderCommand(zIndex, destRect, color);
     }
     
     public static RenderCommand SpriteCommand(sbyte zIndex, SDL.FRect destRect, ColorRgb color, SpriteRenderCommand spriteData) {
          return new RenderCommand(zIndex, destRect, color, spriteData);
     }
}