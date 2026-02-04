using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using SDL3;

namespace LambdaEngine.Rendering;

public unsafe class RenderPipelineManager {
    private const uint MAX_PIPELINES = RenderPipelineId.MaxValue;

    public static readonly RenderPipelineManager Instance = new();

    internal IntPtr _defaultTexturePipeline;

    internal bool _hadInit;

    private RenderPipelineManager() { }

    internal void CreateDefaultTexturePipeline(IntPtr gpuDevice, IntPtr window) {
        if (!ShaderManager.Instance._hasDefaultTextureShaders) {
            throw new  Exception("Unable to create default texture pipeline: Default texture shaders not found.");
        }

        IntPtr vertexShader = ShaderManager.Instance._defaultTextureVertexShader;
        IntPtr fragmentShader = ShaderManager.Instance._defaultTextureFragmentShader;
       
        SDL.GPUColorTargetDescription colorTargetDescription = new() {
            Format = SDL.GetGPUSwapchainTextureFormat(gpuDevice, window),
            BlendState = new() {
                EnableBlend = true,
                AlphaBlendOp = SDL.GPUBlendOp.Add,
                ColorBlendOp = SDL.GPUBlendOp.Add,
                SrcColorBlendFactor = SDL.GPUBlendFactor.SrcAlpha,
                SrcAlphaBlendFactor = SDL.GPUBlendFactor.SrcAlpha,
                DstColorBlendFactor = SDL.GPUBlendFactor.OneMinusSrcAlpha,
                DstAlphaBlendFactor = SDL.GPUBlendFactor.OneMinusSrcAlpha,
            }
        };

        _defaultTexturePipeline = SDL.CreateGPUGraphicsPipeline(gpuDevice, new() {
            TargetInfo = new() {
                NumColorTargets = 1,
                ColorTargetDescriptions = new IntPtr(&colorTargetDescription),
            },
            PrimitiveType = SDL.GPUPrimitiveType.TriangleList,
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
        });
        if (_defaultTexturePipeline == IntPtr.Zero) {
            throw new Exception("Failed to create pipeline.");
        }
    }
}