namespace LambdaEngine.Rendering.RenderCommands;

internal readonly struct SpriteRenderCommand(int textureId) {
    public readonly int TextureId = textureId;
}