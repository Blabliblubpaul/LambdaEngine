namespace LambdaEngine.Rendering.RenderCommands;

internal readonly struct SpriteRenderCommand(IntPtr textureHandle) {
    public readonly IntPtr TextureHandle = textureHandle;
}