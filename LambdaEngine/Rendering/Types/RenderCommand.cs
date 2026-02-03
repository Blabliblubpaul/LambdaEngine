using System.Numerics;
using System.Runtime.InteropServices;
using LambdaEngine.Types;

namespace LambdaEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct RenderCommand {
     public readonly RenderKey RenderKey;
     public readonly Vector2 Position;
     public readonly Vector2 ScreenSize;
     public readonly float Rotation;
     public readonly ColorRgb Color;

     public RenderCommand(RenderKey renderKey, Vector2 position, Vector2 screenSize, float rotation, ColorRgb color) {
          RenderKey = renderKey;
          Position = position;
          ScreenSize = screenSize;
          this.Rotation = rotation;
          Color = color;
     }

     public sbyte ZIndex {
          get => RenderKey.ZIndex;
     }

     public RenderCommandType RenderType {
          get =>  RenderKey.RenderType;
     }
}