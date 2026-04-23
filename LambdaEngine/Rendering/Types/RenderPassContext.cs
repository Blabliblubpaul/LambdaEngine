using System.Numerics;
using SDL3;

namespace LambdaEngine.Rendering;

internal readonly ref struct RenderPassContext(
    Span<RenderCommand> renderCommands,
    SDL.GPUColorTargetInfo colorTargetInfo,
    Matrix4x4 cameraMatrix,
    IntPtr gpuCommandBuffer,
    IntPtr swapchainTexture) {
    public readonly Span<RenderCommand> renderCommands = renderCommands;
    public readonly SDL.GPUColorTargetInfo colorTargetInfo = colorTargetInfo;
    public readonly Matrix4x4 cameraMatrix = cameraMatrix;

    public readonly IntPtr gpuCommandBuffer = gpuCommandBuffer;
    public readonly IntPtr swapchainTexture = swapchainTexture;
}