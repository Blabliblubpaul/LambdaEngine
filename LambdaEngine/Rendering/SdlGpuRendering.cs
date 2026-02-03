using System.Numerics;
using System.Runtime.InteropServices;
using SDL3;

namespace LambdaEngine.Rendering;

internal static unsafe class SdlGpuRendering {
    private static IntPtr _gpuDevice;
    private static IntPtr _window;
    
    private static RenderPipelineManager _pipelineManager;
    private static TextureManager _textureManager;

    private static IntPtr _gpuVertexBuffer;
    private static uint _gpuVertexBufferCapacity = 0;
    
    private static List<RenderCommand> _renderCommands;

    private static IntPtr _commandBuffer;
    private static IntPtr _swapchainTexture;

    private static uint scTextureWidth;
    private static uint scTextureHeight;

    private static SDL.GPUColorTargetInfo _colorTarget;

    private static IntPtr _renderPass;
    
    private static IntPtr _batchPipeline;
    private static IntPtr _batchTexture;
    
    // TODO: write directly into GPU mapped buffer
    private static GpuVertex[] _cpuVertexBuffer = new GpuVertex[8192];
    private static int _vertexCount = 0;

    public static void InitGpuRendering(IntPtr gpuDevice, IntPtr window) {
        _gpuDevice = gpuDevice;
        _window = window;
        
        _pipelineManager = RenderPipelineManager.Instance;
        _textureManager = TextureManager.Instance;
        
        
    }

    public static void InitRenderFrame(List<RenderCommand> renderCommands) {
        _renderCommands = renderCommands;
        
        renderCommands.Sort((a, b) => a.RenderKey.CompareTo(b.RenderKey));
        
        _commandBuffer = SDL.AcquireGPUCommandBuffer(_gpuDevice);
        if (_commandBuffer == IntPtr.Zero) {
            throw new Exception("Unable to acquire GPU CommandBuffer");
        }

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(_gpuDevice, _window,
                out _swapchainTexture, out scTextureWidth, out scTextureHeight)) {
            throw new Exception("Unable to acquire GPU SwapchainTexture");
        }
    }

    public static void BeginRenderPass() {
        SDL.GPUColorTargetInfo colorTarget = new() {
            Texture = _swapchainTexture,
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store,
            ClearColor = new SDL.FColor(
                WindowManager.BackgroundColor.R / 255f,
                WindowManager.BackgroundColor.G / 255f,
                WindowManager.BackgroundColor.B / 255f,
                1f)
        };
        IntPtr colorTargetPtr = new(&colorTarget);
        
        _renderPass = SDL.BeginGPURenderPass(_commandBuffer, colorTargetPtr, 1, IntPtr.Zero);
    }

    public static void ProcessRenderCommands() {
        SDL.BindGPUGraphicsPipeline(_renderPass, _pipelineManager.DefaultPipeline);
        
        SDL.DrawGPUPrimitives(_renderPass, 3, 1, 0, 0);

        return;
        
        Span<RenderCommand> renderCommands = CollectionsMarshal.AsSpan(_renderCommands);
        for (int i = 0; i < renderCommands.Length;) {
            RenderKey key = renderCommands[i].RenderKey;
            
            StartCommandBatch(key);

            do {
                AddCommandToBatch(in renderCommands[i]);
                i++;
            } while (i < renderCommands.Length && renderCommands[i].RenderKey == key);
            
            FlushBatch();
        }
    }

    public static void EndRenderPass() {
        SDL.EndGPURenderPass(_renderPass);
    }

    public static void SubmitGpuCommands() {
        SDL.SubmitGPUCommandBuffer(_commandBuffer);
    }

    private static void StartCommandBatch(RenderKey key) {
        _batchPipeline = _pipelineManager.Get(key.PipelineId);
        SDL.BindGPUGraphicsPipeline(_renderPass, _batchPipeline);

        if (key.TextureId != TextureId.NO_TEXTURE) {
            _batchTexture = _textureManager.Get(key.TextureId);
            // TODO: texture samplers
            throw new NotImplementedException();
        }
        else {
            _batchTexture = IntPtr.Zero;
        }
    }

    private static void AddCommandToBatch(in RenderCommand cmd) {
        if (_cpuVertexBuffer.Length < _vertexCount + 6) {
            Array.Resize(ref _cpuVertexBuffer, _vertexCount * 2);
        }
        
        // Vector2 topLeft = new (cmd.DestRect.X, cmd.DestRect.Y);
        // Vector2 topRight = new (cmd.DestRect.X + cmd.DestRect.W, cmd.DestRect.Y);
        // Vector2 bottomLeft = new (cmd.DestRect.X, cmd.DestRect.Y + cmd.DestRect.H);
        // Vector2 bottomRight = new (cmd.DestRect.X + cmd.DestRect.W, cmd.DestRect.Y + cmd.DestRect.H);
        //
        // uint color = cmd.Color.ToUint32();
        
        Vector2 uvTL = Vector2.Zero;
        Vector2 uvTR = Vector2.UnitX;
        Vector2 uvBL = Vector2.UnitY;
        Vector2 uvBR = Vector2.One;

        // TODO: Is this needed? or can i just be ignored?
        if (cmd.RenderType == RenderCommandType.PRIMITIVE_RECT) {
            uvTL = uvTR = uvBL = uvBR = Vector2.Zero;
        }
        else if (cmd.RenderType == RenderCommandType.SPRITE) {
            if (_batchTexture != IntPtr.Zero) {
                throw new Exception("Unable to process sprite render command; batch texture is invalid.");
            }
            // TODO: configure uv, store uv in sprite cmd data
            throw new NotImplementedException();
        }
        
        // First triangle
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(topLeft, uvTL, color);
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(bottomLeft, uvBL, color);
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(bottomRight, uvBR, color);
        
        // Second triangle
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(topLeft, uvTL, color);
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(bottomRight, uvBR, color);
        // _cpuVertexBuffer[_vertexCount++] = new GpuVertex(topRight, uvTR, color);
    }
    
    private static void FlushBatch() {
        if (_vertexCount == 0) {
            return;
        }

        if (_gpuVertexBuffer == IntPtr.Zero || _gpuVertexBufferCapacity < _vertexCount) {
            if (_gpuVertexBuffer != IntPtr.Zero) {
                SDL.ReleaseGPUBuffer(_gpuDevice, _gpuVertexBuffer);
            }

            _gpuVertexBufferCapacity = (uint)Math.Max(_vertexCount, 8192);

            SDL.GPUBufferCreateInfo vertexBufferCreateInfo = new() {
                Usage = SDL.GPUBufferUsageFlags.Vertex,
                Size = _gpuVertexBufferCapacity * (uint)sizeof(GpuVertex)
            };
            _gpuVertexBuffer = SDL.CreateGPUBuffer(_gpuDevice, in vertexBufferCreateInfo);
        }
        
        // TODO: Create GPU vertex buffer and upload data
        
        // IntPtr mapped = SDL.MapG
        
        // SDL.DrawGPUPrimitives(_renderPass, (uint)_vertexCount, 1, 0, 0);
    }
}