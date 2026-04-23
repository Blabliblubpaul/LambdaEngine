using System.Numerics;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;

namespace LambdaEngine.Rendering.RenderCommandCollectors;

public class SpriteCommandCollector : IRenderCommandCollector {
    private EcsWorld _world;
    private EcsQuery _spriteQuery;

    private NewRenderSystem _renderSystem;

    public void Setup(NewRenderSystem renderSystem, EcsWorld world) {
        _world = world;
        _renderSystem = renderSystem;
        
        _spriteQuery = EcsQuery.Create(_world)
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            // .Include<RotationComponent>()
            .Include<SpriteComponent>()
            .Include<ColorComponent>()
            .Build();
    }

    public void Execute() {
        QueryCollection<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent> sprites =
            _spriteQuery.Execute<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent>();

        int renderCmdIndex = 0;

        // TODO: Handle frustum culling

        // TODO: implement rect primitives
        // Process Rect Primitives
        /*
        foreach (ComponentRef<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent> entity in
                 rectPrimitives.GetComponents()) {
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
        */

        // Process Sprites
        foreach (ComponentRef<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent>
                     entity in sprites.GetComponents()) {
            // TODO: figure out world/cmaera/etc translations
            Vector2 screenPos = Camera.WorldToScreenSpace(entity.Item0.Position);

            int textureId = entity.Item2.TextureId.AsInt32;
            
            // TODO: Fix out of bounds bug.
            (uint Width, uint Height) rawTextureSize = _renderSystem.GetGpuTextureSize(textureId);
            
            Vector2 textureSize = new(rawTextureSize.Width, rawTextureSize.Height);
            Vector2 screenSize = textureSize * entity.Item1.Scale * Camera.Zoom;

            RenderKey key = new(entity.Item2.ZIndex, new RenderPipelineId(0), entity.Item2.TextureId, RenderCommandType.SPRITE);
            
            _renderSystem.RegisterRenderCommand(RenderPass.WORLD, new RenderCommand(key, screenPos, screenSize, 0, entity.Item3.Color));
        }
    }
}