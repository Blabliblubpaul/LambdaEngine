using System.Numerics;
using System.Runtime.InteropServices;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Components.Rendering.PrimitiveComponents;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using LambdaEngine.Rendering.RenderCommands;
using SDL3;

namespace LambdaEngine.Rendering;

public class NewRenderSystem : ISystem {
    private EcsWorld _world;

    private IntPtr _renderer;
    
    private EcsQuery _rectPrimitiveQuery;
    private EcsQuery _spriteQuery;

    public void OnSetup(LambdaEngine engine, EcsWorld world) {
        _world = world;
        
        _rectPrimitiveQuery = EcsQuery.Create(_world)
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            // .Include<RotationComponent>()
            .Include<RectPrimitiveComponent>()
            .Include<ColorComponent>()
            .Build();
        
        _spriteQuery = EcsQuery.Create(_world)
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            // .Include<RotationComponent>()
            .Include<SpriteComponent>()
            .Include<ColorComponent>()
            .Build();
        
        LDebug.Log("RenderSystem - setup complete.");
    }

    public void OnStartup() {
        _renderer = WindowManager.GpuDeviceHandle;
    }

    public void OnExecute() {
        QueryCollection<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent> rectPrimitives =
            _rectPrimitiveQuery.Execute<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent>();
        
        QueryCollection<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent> sprites =
            _spriteQuery.Execute<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent>();

        int renderCommandCount = (int)(rectPrimitives.EntityCount + sprites.EntityCount);
        List<RenderCommand> renderCommands = new(renderCommandCount);
        
        // TODO: Handle frustum culling

        // Process Rect Primitives
        foreach (ComponentRef<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent> entity in rectPrimitives.GetComponents()) {
            Vector2 screenPos = Camera.WorldToScreenSpace(entity.Item0.Position);
            Vector2 screenSize = entity.Item2.Size * entity.Item1.Scale * Camera.Zoom;
            
            SDL.FRect dest = new() {
                X = screenPos.X - screenSize.X * 0.5f,
                Y = screenPos.Y - screenSize.Y * 0.5f,
                W = screenSize.X,
                H = screenSize.Y
            };

            renderCommands.Add(RenderCommand.RectPrimitiveCommand(entity.Item2.ZIndex, dest, entity.Item3.Color));
        }
        
        // Process Sprites
        foreach (ComponentRef<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent> entity in sprites.GetComponents()) {
            Vector2 screenPos = Camera.WorldToScreenSpace(entity.Item0.Position);
            Vector2 screenSize = Texture.GetTextureSize(entity.Item2.TextureHandle) * entity.Item1.Scale * Camera.Zoom;
            
            SDL.FRect dest = new() {
                X = screenPos.X - screenSize.X * 0.5f,
                Y = screenPos.Y - screenSize.Y * 0.5f,
                W = screenSize.X,
                H = screenSize.Y
            };

            SpriteRenderCommand spriteData = new(entity.Item2.TextureHandle);
            renderCommands.Add(RenderCommand.SpriteCommand(entity.Item2.ZIndex, dest, entity.Item3.Color, spriteData));
        }
        
        renderCommands.Sort((a, b) => a.RenderKey.CompareTo(b.RenderKey));
        
        LRendering.SetRenderDrawColor(_renderer, WindowManager.BackgroundColor, 255);
        SDL.RenderClear(_renderer);

        // Render commands
        Span<RenderCommand> commands = CollectionsMarshal.AsSpan(renderCommands);
        for (int i = 0; i < renderCommandCount; i++) {
            ref RenderCommand command = ref commands[i];

            LRendering.SetRenderDrawColor(_renderer, command.Color, 255);

            switch (command.RenderType) { 
                case RenderCommandType.PRIMITIVE_RECT:
                    SDL.RenderFillRect(_renderer, command.DestRect);
                    break;
                
                case RenderCommandType.SPRITE: {
                    Vector2 textureSize = Texture.GetTextureSize(command.SpriteData.TextureHandle);

                    SDL.FRect srcRect = new() {
                        X = 0,
                        Y = 0,
                        W = textureSize.X,
                        H = textureSize.Y
                    };

                    LRendering.SetTextureColorMod(command.SpriteData.TextureHandle, command.Color);
                    SDL.RenderTexture(_renderer, command.SpriteData.TextureHandle, in srcRect, in command.DestRect);
                    break;
                }
                
                // Unreachable
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        SDL.RenderPresent(_renderer);
    }

    public void OnShutdown() {
        LDebug.Log("RenderSystem - shutdown complete.");
    }
}